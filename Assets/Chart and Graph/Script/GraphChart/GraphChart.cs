using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChartAndGraph
{
    public class GraphChart : GraphChartBase, ICanvas
    {
        private Vector2 mLastSetSize = Vector2.zero;

        private CanvasLines CreateDataObject(GraphData.CategoryData data)
        {
            GameObject obj = new GameObject();
            ChartCommon.HideObject(obj, hideHierarchy);
            obj.AddComponent<ChartItem>();
            obj.AddComponent<RectTransform>();
            obj.AddComponent<CanvasRenderer>();
            CanvasLines lines = obj.AddComponent<CanvasLines>();
            obj.transform.SetParent(transform);
            obj.transform.localScale = new Vector3(1f, 1f, 1f);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            return lines;
        }
        protected override void Update()
        {
            base.Update();
            RectTransform trans = GetComponent<RectTransform>();
            if (trans != null && trans.hasChanged)
            {
                if (mLastSetSize != trans.rect.size)
                {
                    Invalidate();
                }
            }
        }
        public override void GenerateChart()
        {
            if (gameObject.activeInHierarchy == false)
                return;
            base.GenerateChart();
            ClearChart();

            if (Data == null)
                return;

            GenerateAxis();
            float minX = (float)((IInternalGraphData)Data).GetMinValue(0);
            float minY = (float)((IInternalGraphData)Data).GetMinValue(1);
            float maxX = (float)((IInternalGraphData)Data).GetMaxValue(0);
            float maxY = (float)((IInternalGraphData)Data).GetMaxValue(1);
            Vector2 min = new Vector2(minX, minY);
            Vector2 max = new Vector2(maxX, maxY);

            Rect viewRect = new Rect(0f, 0f, widthRatio, heightRatio);

            int index = 0;
            int total = ((IInternalGraphData)Data).TotalCategories + 1;
            foreach (GraphData.CategoryData data in ((IInternalGraphData)Data).Categories)
            {

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

                List<CanvasLines.LineSegement> list = new List<CanvasLines.LineSegement>();
                list.Add(new CanvasLines.LineSegement(points));

                if (data.FillMaterial != null)
                {
                    CanvasLines fill = CreateDataObject(data);
                    fill.material = data.FillMaterial;
                    fill.SetLines(list);
                    fill.MakeFillRender(viewRect, data.StetchFill);
                }

                CanvasLines lines = CreateDataObject(data);
                float tiling = 1f;
                if (data.LineTiling.EnableTiling == true && data.LineTiling.TileFactor > 0f)
                {
                    float length = 0f;
                    for (int i = 1; i < points.Length; i++)
                        length += (points[i - 1] - points[i]).magnitude;
                    tiling = length / data.LineTiling.TileFactor;
                }

                lines.Thickness = data.LineThickness;
                lines.Tiling = tiling;
                lines.material = data.LineMaterial;
                lines.SetLines(list);

                if (data.PointMaterial != null)
                {
                    CanvasLines dots = CreateDataObject(data);
                    dots.material = data.PointMaterial;
                    dots.SetLines(list);
                    dots.MakePointRender(data.PointSize);
                }
            }
            FitCanvas();
        }
        void FitCanvas()
        {
            RectTransform trans = GetComponent<RectTransform>();
            GameObject fixPosition = new GameObject();
            ChartCommon.HideObject(fixPosition, hideHierarchy);
            fixPosition.AddComponent<ChartItem>();
            fixPosition.transform.position = transform.position;
            while (gameObject.transform.childCount > 0)
                transform.GetChild(0).SetParent(fixPosition.transform, false);
            fixPosition.transform.SetParent(transform, false);
            fixPosition.transform.localScale = new Vector3(1f, 1f, 1f);
            float widthScale = trans.rect.size.x / WidthRatio;
            float heightScale = trans.rect.size.y / HeightRatio;
            float uniformScale = Math.Min(widthScale, heightScale);
            fixPosition.transform.localScale = new Vector3(uniformScale, uniformScale, uniformScale);
            fixPosition.transform.localPosition = new Vector3(-WidthRatio * uniformScale * 0.5f, -HeightRatio * uniformScale * 0.5f, 0f);
            mLastSetSize = trans.rect.size;
        }
    }
}
