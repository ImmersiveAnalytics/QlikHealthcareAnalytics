using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// repsents a chart item text that is billboarded in a unity scene
/// </summary>
public class BillboardText : MonoBehaviour
{
    private RectTransform mRect;
    public Text UIText { get; set; }
    public RectTransform RectTransformOverride;
    public object UserData { get; set; }
    [NonSerialized]
    public float Scale = 1f;
    public bool parentSet = false;
    public RectTransform Rect
    {
        get
        {
            if (UIText == null)
                return null;
            if (RectTransformOverride != null)
                return RectTransformOverride;
            if (mRect == null)
                mRect = UIText.GetComponent<RectTransform>();
            return mRect;
        }
    }
}
