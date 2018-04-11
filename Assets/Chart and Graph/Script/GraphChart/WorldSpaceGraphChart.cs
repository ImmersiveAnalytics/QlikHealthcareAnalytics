using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChartAndGraph
{
    public class WorldSpaceGraphChart : GraphChartBase
    {

        private float totalDepth = 0f;

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
                return TextCamera;
            }
        }

        protected override float TextIdleDistanceLink
        {
            get
            {
                return TextIdleDistance;
            }
        }
       
        protected override float TotalDepthLink
        {
            get
            {
                return totalDepth;
            }
        }
        protected override void OnPropertyUpdated()
        {
            base.OnPropertyUpdated();
            Invalidate();
        }

        private GameObject CreatePointObject(GraphData.CategoryData data)
        {
            GameObject obj = GameObject.Instantiate(data.DotPrefab);
            ChartCommon.HideObject(obj, hideHierarchy);
            if (obj.GetComponent<ChartItem>() == null)
                obj.AddComponent<ChartItem>();
            obj.transform.SetParent(transform);
            obj.transform.localScale = new Vector3(1f, 1f, 1f);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            return obj;
        }

        private FillPathGenerator CreateFillObject(GraphData.CategoryData data)
        {
            GameObject obj = GameObject.Instantiate(data.FillPrefab.gameObject);
            ChartCommon.HideObject(obj, hideHierarchy);
            FillPathGenerator fill = obj.GetComponent<FillPathGenerator>();
            if (obj.GetComponent<ChartItem>() == null)
                obj.AddComponent<ChartItem>();
            obj.transform.SetParent(transform);
            obj.transform.localScale = new Vector3(1f, 1f, 1f);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            return fill;
        }

        private PathGenerator CreateLineObject(GraphData.CategoryData data)
        {
            GameObject obj = GameObject.Instantiate(data.LinePrefab.gameObject);
            ChartCommon.HideObject(obj, hideHierarchy);
            PathGenerator lines = obj.GetComponent<PathGenerator>();
            if (obj.GetComponent<ChartItem>() == null)
                obj.AddComponent<ChartItem>();
            obj.transform.SetParent(transform);
            obj.transform.localScale = new Vector3(1f, 1f, 1f);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            return lines;
        }

        public override void GenerateChart()
        {
            if (gameObject.activeInHierarchy == false)
                return;
            base.GenerateChart();
            ClearChart();

            if (Data == null)
                return;
            
            float minX = (float)((IInternalGraphData)Data).GetMinValue(0);
            float minY = (float)((IInternalGraphData)Data).GetMinValue(1);
            float maxX = (float)((IInternalGraphData)Data).GetMaxValue(0);
            float maxY = (float)((IInternalGraphData)Data).GetMaxValue(1);
            Vector2 min = new Vector2(minX, minY);
            Vector2 max = new Vector2(maxX, maxY);

            Rect viewRect = new Rect(0f, 0f, widthRatio, heightRatio);

            int index = 0;
            int total = ((IInternalGraphData)Data).TotalCategories + 1;
            float positiveDepth = 0f;
            float maxThickness = 0f;

            foreach (GraphData.CategoryData data in ((IInternalGraphData)Data).Categories)
            {
                maxThickness = Math.Max(data.LineThickness, maxThickness);
                Vector3[] points = data.Data.Select(x => ((Vector3)x)).ToArray();
                TransformPoints(points, viewRect, min, max);
                if (points.Length == 0 && ChartCommon.IsInEditMode)
                {
                    int tmpIndex = total - 1 - index;
                    float y1 = (((float)tmpIndex) / (float)total);
                    float y2 = (((float)tmpIndex + 1) / (float)total);
                    Vector3 pos1 = interpolateInRect(viewRect, new Vector2(0f, y1));
                    Vector3 pos2 = interpolateInRect(viewRect, new Vector2(0.5f, y2));
                    Vector3 pos3 = interpolateInRect(viewRect, new Vector2(1f, y1));
                    points = new Vector3[] { pos1, pos2, pos3 };
                    index++;
                }

                /*if (data.FillMaterial != null)
                {
                    CanvasLines fill = CreateDataObject(data);
                    fill.material = data.FillMaterial;
                    fill.SetLines(list);
                    fill.MakeFillRender(viewRect, data.StetchFill);
                }*/

                if (data.Depth > 0)
                    positiveDepth = Mathf.Max(positiveDepth, data.Depth);

                if (data.LinePrefab != null)
                {
                    PathGenerator lines = CreateLineObject(data);
                //    float tiling = 1f;

                    if (data.LineTiling.EnableTiling == true && data.LineTiling.TileFactor > 0f)
                    {
                        float length = 0f;
                        for (int i = 1; i < points.Length; i++)
                            length += (points[i - 1] - points[i]).magnitude;
                      //  tiling = length / data.LineTiling.TileFactor;
                    }

                    lines.Generator(points, data.LineThickness, false);
                    Vector3 tmp = lines.transform.localPosition;
                    tmp.z = data.Depth;
                    lines.transform.localPosition = tmp;
                    if (data.LineMaterial != null)
                    {
                        Renderer rend = lines.GetComponent<Renderer>();
                        if (rend != null)
                            rend.material = data.LineMaterial;
                    }
                }
                totalDepth = positiveDepth + maxThickness*2f;
                if (data.DotPrefab != null)
                {
                    for (int i = 0; i < points.Length; i++)
                    {
                        GameObject point = CreatePointObject(data);
                        point.transform.localScale = new Vector3(data.PointSize, data.PointSize, data.PointSize);
                        if (data.PointMaterial != null)
                        {
                            Renderer rend = point.GetComponent<Renderer>();
                            if (rend != null)
                                rend.material = data.PointMaterial;
                        }
                        point.transform.localPosition = points[i] + new Vector3(0f,0f,data.Depth);
                    }
                }

                if (data.FillPrefab != null)
                {
                    FillPathGenerator fill = CreateFillObject(data);
                    Vector3 tmp = fill.transform.localPosition;
                    tmp.z = data.Depth;
                    fill.transform.localPosition = tmp;
                    if (data.LinePrefab == null || !(data.LinePrefab is SmoothPathGenerator))
                    {
                        fill.SetLineSmoothing(false, 0, 0f);
                    }
                    else
                    {
                        SmoothPathGenerator smooth = ((SmoothPathGenerator)data.LinePrefab);
                        fill.SetLineSmoothing(true, smooth.JointSmoothing, smooth.JointSize);
                    }

                    fill.SetGraphBounds(viewRect.yMin, viewRect.yMax);
                    fill.SetStrechFill(data.StetchFill);
                    fill.Generator(points, data.LineThickness * 1.01f, false);
                    if (data.FillMaterial != null)
                    {
                        Renderer rend = fill.GetComponent<Renderer>();
                        if (rend != null)
                            rend.material = data.FillMaterial;
                    }
                }

                
            }
            GenerateAxis();
        }
    }
}
