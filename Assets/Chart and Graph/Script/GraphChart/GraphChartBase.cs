using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChartAndGraph
{
    /// <summary>
    /// the graph chart class.
    /// </summary>
    [ExecuteInEditMode]
    public abstract class GraphChartBase : AxisChart ,ISerializationCallbackReceiver
    {
        [SerializeField]
        [Tooltip("The height ratio of the chart")]
        protected float heightRatio = 300;
        [SerializeField]
        [Tooltip("The width ratio of the chart")]
        protected float widthRatio = 600;

        /// <summary>
        /// The height ratio of the chart
        /// </summary>
        [SerializeField]
        public float HeightRatio
        {
            get { return heightRatio; }
            set
            {
                heightRatio = value;
                Invalidate();
            }
        }
        /// <summary>
        /// The width ratio of the chart
        /// </summary>
        public float WidthRatio
        {
            get { return widthRatio; }
            set
            {
                widthRatio = value;
                Invalidate();
            }
        }

        /// <summary>
        /// the graph data
        /// </summary>
        [HideInInspector]
        [SerializeField]
        protected GraphData Data = new GraphData();

        /// <summary>
        /// Holds the graph chart data. including values and categories
        /// </summary>
        public GraphData DataSource { get { return Data; } }


        protected override LegenedData LegendInfo
        {
            get
            {
                LegenedData data = new LegenedData();
                if (Data == null)
                    return data;
                foreach(var cat in ((IInternalGraphData)Data).Categories)
                {
                    LegenedData.LegenedItem item = new LegenedData.LegenedItem();
                    item.Name = cat.Name;
                    if(cat.FillMaterial != null)
                        item.Material = cat.FillMaterial;
                    else
                        item.Material = cat.LineMaterial;
                    data.AddLegenedItem(item);
                }
                return data;
            }
        }

        private void HookEvents()
        {
            ((IInternalGraphData)Data).InternalDataChanged += GraphChart_InternalDataChanged;
        }

        private void GraphChart_InternalDataChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        protected override void Start()
        {
            base.Start();
            if (ChartCommon.IsInEditMode == false)
            {
                HookEvents();
            }
            Invalidate();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (ChartCommon.IsInEditMode == false)
            {
                HookEvents();
            }
            Invalidate();
        }

        protected override void OnLabelSettingChanged()
        {
            base.OnLabelSettingChanged();
            Invalidate();
        }
        protected override void OnAxisValuesChanged()
        {
            base.OnAxisValuesChanged();
            Invalidate();
        }
        protected override void OnLabelSettingsSet()
        {
            base.OnLabelSettingsSet();
            Invalidate();
        }

        protected Vector2 interpolateInRect(Rect rect,Vector2 point)
        {
            float x = rect.x + rect.width * point.x;
            float y = rect.y + rect.height * point.y;
            return new Vector2(x, y);
        }
        protected Vector2 TransformPoint(Rect viewRect,Vector3 point ,Vector2 min,Vector2 range)
        {
            return interpolateInRect(viewRect, new Vector2((point.x - min.x) / range.x, (point.y - min.y) / range.y));
        }
        protected void TransformPoints(Vector3[] points,Rect viewRect,Vector2 min,Vector2 max)
        {
            Vector2 range = max - min;
            if (range.x <= 0.0001f || range.y < 0.0001f)
                return;

            for(int i=0; i<points.Length; i++)
            {
                Vector2 point = points[i];
                points[i] = interpolateInRect(viewRect,new Vector2((point.x - min.x) / range.x, (point.y - min.y) / range.y));                
            }
        }


        protected override void ValidateProperties()
        {
            base.ValidateProperties();
            if (heightRatio < 0f)
                heightRatio = 0f;
            if (widthRatio < 0f)
                widthRatio = 0f;
        }
        protected override bool SupportsCategoryLabels
        {
            get
            {
                return false;
            }
        }

        protected override bool SupportsGroupLables
        {
            get
            {
                return false;
            }
        }

        protected override bool SupportsItemLabels
        {
            get
            {
                return false;
            }
        }

        protected override float TotalHeightLink
        {
            get
            {
                return heightRatio;
            }
        }

        protected override float TotalWidthLink
        {
            get
            {
                return widthRatio;
            }
        }

        protected override float TotalDepthLink
        {
            get
            {
                return 0.0f;
            }
        }

        protected override bool HasValues(AxisBase axis)
        {
            return true; //all axis have values in the graph chart
        }

        protected override double MaxValue(AxisBase axis)
        {
            if (axis == null)
                return 0.0;
            if (axis == mHorizontalAxis)
                return ((IInternalGraphData)Data).GetMaxValue(0);
            if (axis == mVerticalAxis)
                return ((IInternalGraphData)Data).GetMaxValue(1);
            return 0.0;
        }

        protected override double MinValue(AxisBase axis)
        {
            if (axis == null)
                return 0.0;
            if (axis == mHorizontalAxis)
                return ((IInternalGraphData)Data).GetMinValue(0);
            if (axis == mVerticalAxis)
                return ((IInternalGraphData)Data).GetMinValue(1);
            return 0.0;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (Data != null)
                ((IInternalGraphData)Data).OnBeforeSerialize();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (Data != null)
                ((IInternalGraphData)Data).OnAfterDeserialize();
        }
    }
}
