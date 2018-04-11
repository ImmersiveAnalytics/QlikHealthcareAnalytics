using ChartAndGraph;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// posistions world space text on a special canvas used for billboarding.
/// </summary>
[RequireComponent(typeof(ChartItem))]
[ExecuteInEditMode]
public class TextController : MonoBehaviour
{
    public Camera Camera = null;
    Canvas mCanvas;
    RectTransform mRect;
    List<BillboardText> mText = new List<BillboardText>();
    public float PlaneDistance = 4;
    Vector3[] mPlaneCorners = new Vector3[4];
    private AnyChart mPrivateParent;
//    private bool mUnderCanvas = false;
    internal List<BillboardText> Text
    { get { return mText; } }
    internal AnyChart mParent
    {
        get { return mPrivateParent; }
        set
        {
            mPrivateParent = value;
            if (mPrivateParent != null)
            {
                Camera = ((IInternalUse)mPrivateParent).InternalTextCamera;
                PlaneDistance = ((IInternalUse)mPrivateParent).InternalTextIdleDistance;
                SafeCanvas.planeDistance = PlaneDistance;
            }
        }
    }
    void Start()
    {
        EnsureCanvas();

    }


    void EnsureCanvas()
    {
        if (mCanvas == null)
        {
            mCanvas = GetComponentInParent<Canvas>();
            if (mCanvas == null)
            {
                mCanvas = gameObject.AddComponent<Canvas>();
                gameObject.AddComponent<CanvasScaler>();
                gameObject.AddComponent<GraphicRaycaster>();
                if (mParent != null && mParent.VRSpaceText)
                {
                    mCanvas.renderMode = RenderMode.WorldSpace;
                }
                else
                {
                    mCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                }
                mCanvas.planeDistance = PlaneDistance;
                Camera = EnsureCamera();
                mRect = mCanvas.GetComponent<RectTransform>();
                //  mCanvas.pixelPerfect = true;
                CanvasScaler scaler = GetComponent<CanvasScaler>();
               scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            }
            else
            {
                //mUnderCanvas = true;
            }
        }
    }
    Canvas SafeCanvas
    {
        get
        {
            EnsureCanvas();
            return mCanvas;
        }
    }
    void OnDestory()
    {
        DestroyAll();
    }

    public void DestroyAll()
    {
        for(int i=0; i<mText.Count; i++)
        {
            if (mText[i] != null)
            {
                if (mText[i].UIText != null)
                    ChartCommon.SafeDestroy(mText[i].UIText.gameObject);
                if (mText[i].RectTransformOverride != null)
                    ChartCommon.SafeDestroy(mText[i].RectTransformOverride.gameObject);
                mText[i].UIText = null;
                mText[i].RectTransformOverride = null;
                ChartCommon.SafeDestroy(mText[i].gameObject);
            }
        }
        mText.Clear();
    }

    public void AddText(BillboardText billboard)
    {
        if (billboard.UIText == null)
            return;

        mText.Add(billboard);
        TextDirection dir = billboard.Rect.GetComponent<TextDirection>();

        if (dir != null)
            dir.SetTextController(this);

        billboard.Rect.SetParent(transform, false);
        billboard.Rect.localRotation = Quaternion.identity;

        //Vector3 scale = new Vector3(1f/transform.lossyScale.x, 1f/transform.lossyScale.y, 1f/transform.lossyScale.z);//SafeCanvas.transform.localScale;
        //Vector3 scale = SafeCanvas.transform.localScale;

        //if (mUnderCanvas)
        //  billboard.UIText.transform.localScale = scale;
        //else
        billboard.UIText.transform.localScale = new Vector3(billboard.Scale, billboard.Scale, 1f);

        //        ContentSizeFitter fitter = billboard.UIText.gameObject.GetComponent<ContentSizeFitter>();
        //        if(fitter == null)
        //            fitter = billboard.UIText.gameObject.AddComponent<ContentSizeFitter>();
        //        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        //        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        RectTransform rect = billboard.Rect;
  //   rect.anchoredPosition3D = new Vector3();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        billboard.Rect.position = billboard.transform.position;
    }

    Camera EnsureCamera()
    {
        if (Camera == null)
            return AssignCamera(Camera.main);
        else
            return AssignCamera(Camera);
    }

    Camera AssignCamera(Camera camera)
    {
        Canvas canvas = SafeCanvas;
        if (canvas.worldCamera != camera)
            canvas.worldCamera = camera;
        return camera;
    }

    void Update()
    {

    }

    Vector3 ProjectPointOnPlane(Vector3 planeNormal , Vector3 planePoint , Vector3 point )
    {
        planeNormal.Normalize();
        float distance = -Vector3.Dot(planeNormal.normalized, (point - planePoint));
        return point + planeNormal* distance;
    }
    void CalculatePlane(Camera cam,RectTransform transform,out Vector3 center,out Vector3 normal)
    {
        mRect.GetWorldCorners(mPlaneCorners);
        center = new Vector3();
        for(int i=0; i<mPlaneCorners.Length; i++)
        {
            center += mPlaneCorners[i];
        }
        center *= 0.25f;
        Vector3 a = mPlaneCorners[1] - mPlaneCorners[0];
        Vector3 b = mPlaneCorners[2] - mPlaneCorners[0];
        normal = Vector3.Cross(a, b).normalized;
    }
    
    public void ApplyTextPosition()
    {
        //if (mUnderCanvas)
        //{
        Camera = EnsureCamera();

        if (mParent != null && mParent.VRSpaceText)
        {
            mCanvas.transform.rotation = Camera.transform.rotation;
            mCanvas.transform.localScale = new Vector3(mParent.VRSpaceScale, mParent.VRSpaceScale, mParent.VRSpaceScale);
        }

        foreach (BillboardText billboard in mText)
        {
                float scale = 1f;
                if (mPrivateParent != null && mPrivateParent.KeepOrthoSize)
                {

                    if (Camera != null && Camera.orthographic && Camera.orthographicSize > 0.1f)
                    {
                        scale = 5f / Camera.orthographicSize;    
                    }
                }
                billboard.Rect.position = billboard.transform.position;
                billboard.UIText.transform.localScale = new Vector3(billboard.Scale* scale, billboard.Scale*scale, 1f);
            }
            return;
        //}
/*        Canvas canvas = SafeCanvas;
        if (mParent != null)
        {
            Camera = mParent.TextCamera;
            PlaneDistance = mParent.TextIdleDistance;
            canvas.planeDistance = PlaneDistance;
        }

        Camera cam = EnsureCamera();
        if (cam == null)
        {
            if (mWarn == false)
                Debug.LogWarning("Chart Warning: No main camera set , please set the Chart's camera using \"TextCamera\" in the inspector");
            mWarn = true;
            return;
        }
        mWarn = false;
        Vector3 planeCenter;
        Vector3 planeNormal;
        CalculatePlane(cam, mRect, out planeCenter, out planeNormal);
        //Debug.DrawLine(planeCenter, planeCenter+ planeNormal,Color.red);
        foreach (BillboardText text in mText)
        {
            //  RectTransformUtility.
            Vector3 point = text.transform.position;
            Vector3 pointOnPlane = ProjectPointOnPlane(planeNormal, planeCenter, text.transform.position);
            //Debug.DrawLine(point, pointOnPlane);
            Vector3 worldVec = (pointOnPlane - point);
            Vector3 vec = mRect.worldToLocalMatrix * worldVec;
            float dist = vec.magnitude;
            if (Vector3.Dot(worldVec, cam.transform.forward) > 0)
                dist = -dist;
            
            Vector2 viewport = cam.WorldToViewportPoint(pointOnPlane);
          //  Debug.DrawLine(viewport, pointOnPlane);
//            Vector3 anchored = new Vector3(
  //              (mRect.sizeDelta.x * viewport.x) - (mRect.sizeDelta.x * mRect.anchorMin.x),
    //            (mRect.sizeDelta.y * viewport.y) - (mRect.sizeDelta.y * mRect.anchorMin.y),
      //          dist);
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam,pointOnPlane);
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mRect, screen, cam, out pos);
            //            float scaleDist = mPlaneDistance + worldVec.magnitude;
            //          if (scaleDist <= 0)
            //            scaleDist = 0.1f;
            // float scale = scaleDist/ mPlaneDistance;
            //text.Rect.localScale = new Vector3(scale, scale, 1f);
            text.Rect.localPosition = new Vector3(pos.x, pos.y , dist);
        }*/
    }

    void LateUpdate()
    {
        ApplyTextPosition();
    }
}