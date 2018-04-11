using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChartAndGraph
{
    [Serializable]
    class WorldSpacePieChart : PieChart
    {
        /// <summary>
        /// If this value is set all the text in the chart will be rendered to this specific camera. otherwise text is rendered to the main camera
        /// </summary>
        [SerializeField]
        [Tooltip("If this value is set all the text in the chart will be rendered to this specific camera. otherwise text is rendered to the main camera")]
        private Camera textCamera;

        /// <summary>
        /// If this value is set all the text in the chart will be rendered to this specific camera. otherwise text is rendered to the main camera
        /// </summary>
        public Camera TextCamera
        {
            get { return textCamera; }
            set
            {
                textCamera = value;
                OnPropertyUpdated();
            }
        }
        /// <summary>
        /// The distance from the camera at which the text is at it's original size.
        /// </summary>
        [SerializeField]
        [Tooltip("The distance from the camera at which the text is at it's original size")]
        private float textIdleDistance = 20f;

        /// <summary>
        /// The distance from the camera at which the text is at it's original size.
        /// </summary>
        public float TextIdleDistance
        {
            get { return textIdleDistance; }
            set
            {
                textIdleDistance = value;
                OnPropertyUpdated();
            }
        }
        protected override Camera TextCameraLink
        {
            get
            {
                return textCamera;
            }
        }

        protected override float TextIdleDistanceLink
        {
            get
            {
                return textIdleDistance;
            }
        }

        [SerializeField]
        [Tooltip("depth size of the pie slices")]
        private float depth;

        [SerializeField]
        [Tooltip("pie prefab of the pie slices")]
        private GameObject prefab;

        public WorldSpacePieChart()
        {
            radius = 2;
        }

        /// <summary>
        /// pie prefab of the pie slices
        /// </summary>
        public GameObject Prefab
        {
            get { return prefab; }
            set
            {
                prefab = value;
                OnPropertyUpdated();
            }
        }

        /// <summary>
        /// depth size of the pie slices
        /// </summary>
        public float Depth
        {
            get { return depth; }
            set
            {
                depth = value;
                OnPropertyUpdated();
            }
        }
        protected override float DepthLink
        {
            get
            {
                return depth;
            }
        }

        protected override IPieGenerator PreparePieObject(out GameObject pieObject)
        {
            if (Prefab == null)
                pieObject = new GameObject();
            else
                pieObject = GameObject.Instantiate(Prefab);
            ChartCommon.EnsureComponent<MeshFilter>(pieObject);
            ChartCommon.EnsureComponent<MeshRenderer>(pieObject);
            return  ChartCommon.EnsureComponent<WorldSpacePieGenerator>(pieObject);
        }
        protected override void ValidateProperties()
        {
            base.ValidateProperties();
            if (depth < 0f)
                depth = 0f;
        }
        protected override Material LineMaterialLink
        {
            get
            {
                return null;
            }
        }
        protected override float LineThicknessLink
        {
            get
            {
                return 0f;
            }
        }
        protected override float LineSpacingLink
        {
            get
            {
                return 0f;
            }
        }
    }
}
