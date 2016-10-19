// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.VR.WSA;

namespace HoloToolkit.Unity
{
    /// <summary>
    /// GazeManager determines the location of the user's gaze, hit position and normals.
    /// </summary>
    public partial class GazeManager : Singleton<GazeManager>
    {
        [Tooltip("Maximum gaze distance, in meters, for calculating a hit.")]
        public float MaxGazeDistance = 15.0f;

        [Tooltip("Select the layers raycast should target.")]
        public LayerMask RaycastLayerMask = Physics.DefaultRaycastLayers;

        /// <summary>
        /// Physics.Raycast result is true if it hits a hologram.
        /// </summary>
        public bool Hit { get; private set; }

        /// <summary>
        /// HitInfo property gives access
        /// to RaycastHit public members.
        /// </summary>
        public RaycastHit HitInfo { get; private set; }

        /// <summary>
        /// Position of the intersection of the user's gaze and the holograms in the scene.
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// RaycastHit Normal direction.
        /// </summary>
        public Vector3 Normal { get; private set; }

        /// <summary>
        /// Object currently being focused on.
        /// </summary>
        public GameObject FocusedObject { get; private set; }

        private GazeStabilizer gazeStabilizer;
        private Vector3 gazeOrigin;
        private Vector3 gazeDirection;

        void Awake()
        {
            /* TODO: DEVELOPER CODING EXERCISE 3.a */

            // 3.a: GetComponent GazeStabilizer and assign it to gazeStabilizer.
            gazeStabilizer = GetComponent<GazeStabilizer>();
        }

        private void Update()
        {
            gazeOrigin = Camera.main.transform.position;
            gazeDirection = Camera.main.transform.forward;

            // 3.a: Using gazeStabilizer, call function UpdateHeadStability.
            // Pass in gazeOrigin and Camera's main transform rotation.
            gazeStabilizer.UpdateHeadStability(gazeOrigin, Camera.main.transform.rotation);

            // 3.a: Using gazeStabilizer, get the StableHeadPosition and
            // assign it to gazeOrigin.
            gazeOrigin = gazeStabilizer.StableHeadPosition;

            UpdateRaycast();
        }

        /// <summary>
        /// Calculates the Raycast hit position and normal.
        /// </summary>
        private void UpdateRaycast()
        {
            // Get the raycast hit information from Unity's physics system.
            RaycastHit hitInfo;
            Hit = Physics.Raycast(gazeOrigin,
                           gazeDirection,
                           out hitInfo,
                           MaxGazeDistance,
                           RaycastLayerMask);

            GameObject oldFocusedObject = FocusedObject;
            // Update the HitInfo property so other classes can use this hit information.
            HitInfo = hitInfo;

            if (Hit)
            {
                // If the raycast hits a hologram, set the position and normal to match the intersection point.
                Position = hitInfo.point;
                Normal = hitInfo.normal;
                FocusedObject = hitInfo.collider.gameObject;
            }
            else
            {
                // If the raycast does not hit a hologram, default the position to last hit distance in front of the user,
                // and the normal to face the user.
                // 2.a: Assign Position to be gazeOrigin plus MaxGazeDistance times gazeDirection.
                Position = gazeOrigin + (gazeDirection * MaxGazeDistance);
                // 2.a: Assign Normal to be the user's gazeDirection.
                Normal = gazeDirection;

                FocusedObject = null;
            }

            // Check if the currently hit object has changed
            if (oldFocusedObject != FocusedObject)
            {
                if (oldFocusedObject != null)
                {
                    oldFocusedObject.SendMessage("OnGazeLeave", SendMessageOptions.DontRequireReceiver);
                }
                if (FocusedObject != null)
                {
                    FocusedObject.SendMessage("OnGazeEnter", SendMessageOptions.DontRequireReceiver);
                    Debug.Log("New Object Hit");
                }
            }
        }

    }
}