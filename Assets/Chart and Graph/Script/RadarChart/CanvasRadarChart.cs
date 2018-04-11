using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChartAndGraph
{
    public class CanvasRadarChart : RadarChart
    {
        protected RadarFill CreateFillObject(GameObject conatiner)
        {
            GameObject obj = ChartCommon.CreateChartItem();
            ChartCommon.HideObject(obj, hideHierarchy);
            obj.AddComponent<ChartItem>();
            obj.AddComponent<RectTransform>();
            obj.AddComponent<CanvasRenderer>();
            RadarFill fill = obj.AddComponent<RadarFill>();
            obj.transform.SetParent(conatiner.transform, false);
            obj.transform.localScale = new Vector3(1f, 1f, 1f);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            return fill;
        }

        protected CanvasLines CreateLinesObject(GameObject conatiner)
        {
            GameObject obj = ChartCommon.CreateChartItem();
            ChartCommon.HideObject(obj, hideHierarchy);
            obj.AddComponent<ChartItem>();
            obj.AddComponent<RectTransform>();
            obj.AddComponent<CanvasRenderer>();
            CanvasLines lines = obj.AddComponent<CanvasLines>();
            obj.transform.SetParent(conatiner.transform,false);
            obj.transform.localScale = new Vector3(1f, 1f, 1f);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            return lines;
        }

        protected override GameObject CreateAxisObject(float thickness, Vector3[] path)
        {
            Vector3[] newPath = new Vector3[path.Length + 1];
            path.CopyTo(newPath, 0);
            newPath[path.Length] = path[0];
            path = newPath;
            List<CanvasLines.LineSegement> seg = new List<CanvasLines.LineSegement>();
            seg.Add(new CanvasLines.LineSegement(path));
            GameObject conatiner = ChartCommon.CreateChartItem();
            ChartCommon.HideObject(conatiner, hideHierarchy);
            conatiner.transform.SetParent(transform,false);
            conatiner.transform.localScale = new Vector3(1f, 1f, 1f);
            conatiner.transform.localPosition = Vector3.zero;
            conatiner.transform.localRotation = Quaternion.identity;

            if (AxisLineMaterial != null && AxisThickness > 0f)
            {
                CanvasLines lines = CreateLinesObject(conatiner);
                lines.material = AxisLineMaterial;
                lines.Thickness = thickness;
                lines.SetLines(seg);
            }

            if (AxisPointMaterial != null && AxisPointSize > 0f)
            {
                CanvasLines points = CreateLinesObject(conatiner);
                points.material = AxisPointMaterial;
                points.MakePointRender(AxisPointSize);
                points.SetLines(seg);
            }
                       
            return conatiner;
        }

        public override void GenerateChart()
        {
            base.GenerateChart();
            if (TextController != null && TextController.gameObject)
                TextController.gameObject.transform.SetAsLastSibling();
        }

        protected override GameObject CreateCategoryObject(Vector3[] path, int category)
        {
            Vector3[] newPath = new Vector3[path.Length + 1];
            path.CopyTo(newPath, 0);
            newPath[path.Length] = path[0];
            path = newPath;
            List<CanvasLines.LineSegement> seg = new List<CanvasLines.LineSegement>();
            seg.Add(new CanvasLines.LineSegement(path));
            RadarChartData.CategoryData cat = ((IInternalRadarData)DataSource).getCategoryData(category);
            GameObject container = ChartCommon.CreateChartItem();
            ChartCommon.HideObject(container, hideHierarchy);
            container.transform.SetParent(transform, false);
            container.transform.localScale = new Vector3(1f, 1f, 1f);
            container.transform.localPosition = Vector3.zero;
            container.transform.localRotation = Quaternion.identity;

            if (cat.FillMaterial != null)
            {
                RadarFill fill = CreateFillObject(container);
                fill.material = cat.FillMaterial;
                fill.SetPath(path, Radius);
            }

            if (cat.LineMaterial != null && cat.LineThickness > 0)
            {
                CanvasLines lines = CreateLinesObject(container);
                lines.material = cat.LineMaterial;
                lines.Thickness = cat.LineThickness;
                lines.SetLines(seg);
            }

            if (cat.PointMaterial != null && cat.PointSize > 0f)
            {
                CanvasLines points = CreateLinesObject(container);
                points.material = cat.PointMaterial;
                points.MakePointRender(cat.PointSize);
                points.SetLines(seg);
            }
            return container;
        }
    }
}
