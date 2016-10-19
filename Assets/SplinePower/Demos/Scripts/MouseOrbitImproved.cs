using UnityEngine;
using System.Collections;


public class MouseOrbitImproved : MonoBehaviour
{

    public Transform target;
    public float distance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float distanceMin = .5f;
    public float distanceMax = 15f;

    float x = 0.0f;
    float y = 0.0f;

    // Use this for initialization
    private void Start()
    {
        Screen.lockCursor = true;

        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
    }

    private float cx;
    private float cy;

    private void LateUpdate()
    {
        if (target)
        {
            float lerpK = 0.25f;
            float zoomSpeed = 10f;

            cx = Mathf.Lerp(cx, Input.GetAxis("Mouse X") *  xSpeed * distance * 0.0032f, lerpK);
            cy = Mathf.Lerp(cy, Input.GetAxis("Mouse Y") * ySpeed * 0.025f, lerpK);

            x += cx;
            y -= cy;

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);

            distance = Mathf.Lerp(distance, Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, distanceMin, distanceMax), lerpK);

            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            transform.rotation = rotation;
            transform.position = position;

        }

        if (Input.GetKeyDown(KeyCode.End))
        {
            Screen.lockCursor = (Screen.lockCursor == false) ? true : false;
        }
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 210, 50), "");
        GUI.Label(new Rect(15, 15, 200, 50), "Mouse - spin around\nMouse Wheel - zoom in/zoom out");
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
