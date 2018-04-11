using UnityEngine;
using System.Collections;
using HoloToolkit.Unity.InputModule;

/// <summary>
/// FocusedObjectMessageReceiver class shows how to handle messages sent by FocusedObjectMessageSender.
/// This particular implementatoin controls object appearance by changing its color when focused.
/// </summary>
public class MyFocusedObjectMessageReceiver : MonoBehaviour, IFocusable
{
    [Tooltip("Object color changes to this when focused.")]
    public Color FocusedColor = Color.red;

    private Material material;
    private Color originalColor;

    private void Start()
    {
        material = GetComponent<Renderer>().material;
        originalColor = material.color;
    }

    //    public void OnGazeEnter()
    public void OnFocusEnter()
    {
        material.color = FocusedColor;
        GameObject label = transform.GetChild(0).gameObject;
        label.SetActive(true);

    }

//    public void OnGazeLeave()
    public void OnFocusExit()
    {
        material.color = originalColor;
        GameObject label = transform.GetChild(0).gameObject;
        label.SetActive(false);
    }
}