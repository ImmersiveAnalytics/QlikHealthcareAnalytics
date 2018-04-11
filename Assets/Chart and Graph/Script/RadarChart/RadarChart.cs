using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChartAndGraph
{
    [ExecuteInEditMode]
    public abstract class RadarChart : AnyChart, ISerializationCallbackReceiver
    {
        [SerializeField]
        private float radius = 3f;

        public float Radius
        {
            get { return radius; }
            set
            {
                radius = value;
                GenerateChart();
            }
        }

        [SerializeField]
        private float angle = 0f;

        public float Angle
        {
            get { return angle; }
            set
            {
                angle = value;
                GenerateChart();
            }
        }
        [SerializeField]
        private Material axisPointMaterial;

        public Material AxisPointMaterial
        {
            get
            {
                return axisPointMaterial;
            }
            set
            {
                axisPointMaterial = value;
                GenerateChart();
            }
        }


        [SerializeField]
        private Material axisLineMaterial;
        public Material AxisLineMaterial
        {
            get
            {
                return axisLineMaterial;
            }
            set
            {
                axisLineMaterial = value;
                GenerateChart();
            }
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

        [SerializeField]
        private float axisThickness = 0.1f;

        public float AxisThickness
        {
            get
            {
                return axisThickness;
            }
            set
            {
                axisThickness = value;
                GenerateChart();
            }
        }

        [SerializeField]
        private float axisPointSize = 1f;

        public float AxisPointSize
        {
            get
            {
                return axisPointSize;
            }
            set
            {
                axisPointSize = value;
                GenerateChart();
            }
        }

        [SerializeField]
        private float axisAdd = 0f;

        public float AxisAdd
        {
            get
            {
                return axisAdd;
            }
            set
            {
                axisAdd = value;
                GenerateChart();
            }
        }

        [SerializeField]
        private int totalAxisDevisions = 5;

        public int TotalAxisDevisions
        {
            get
            {
                return totalAxisDevisions;
            }
            set
            {
                totalAxisDevisions = value;
                GenerateChart();
            }
        }

        List<GameObject> mAxisObjects = new List<GameObject>();
        /// <summary>
        /// the radar data
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private RadarChartData Data = new RadarChartData();

        /// <summary>
        /// Holds the radar chart data. including values, categories and groups.
        /// </summary>
        public RadarChartData DataSource { get { return Data; } }

        void HookEvents()
        {
            Data.ProperyUpdated -= DataUpdated;
            Data.ProperyUpdated += DataUpdated;
            ((IInternalRadarData)Data).InternalDataSource.DataStructureChanged -= StructureUpdated;
            ((IInternalRadarData)Data).InternalDataSource.DataStructureChanged += StructureUpdated;
            ((IInternalRadarData)Data).InternalDataSource.DataValueChanged -= ValueChanged; ;
            ((IInternalRadarData)Data).InternalDataSource.DataValueChanged += ValueChanged;
        }

        private void ValueChanged(object sender, DataSource.ChartDataSourceBase.DataValueChangedEventArgs e)
        {
            Invalidate();
        }

        private void StructureUpdated(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void DataUpdated()
        {
            Invalidate();
        }

        protected override LegenedData LegendInfo
        {
            get
            {
                LegenedData legend = new LegenedData();
                if (Data == null)
                    return legend;
                foreach (var column in ((IInternalRadarData)Data).InternalDataSource.Columns)
                {
                    var item = new LegenedData.LegenedItem();
                    var catData = column.UserData as RadarChartData.CategoryData;
                    item.Name = column.Name;
                    if (catData.FillMaterial != null)
                        item.Material = catData.FillMaterial;
                    else
                    {
                        if (catData.LineMaterial != null)
                        {
                            item.Material = catData.LineMaterial;
                        }
                        else
                            item.Material = null;
                    }
                    legend.AddLegenedItem(item);
                }
                return legend;
            }
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
                return true;
            }
        }

        protected override bool SupportsItemLabels
        {
            get
            {
                return true;
            }
        }

        protected override float TotalDepthLink
        {
            get
            {
                return 0f;
            }
        }

        protected override float TotalHeightLink
        {
            get
            {
                return 0f;
            }
        }

        protected override float TotalWidthLink
        {
            get
            {
                return 0f;
            }
        }

        protected override void Update()
        {
            base.Update();
            if (Data != null)
                ((IInternalRadarData)Data).Update();
        }

        /// <summary>
        /// used internally , do not call this method
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            if (Application.isPlaying)
            {
                HookEvents();
            }
            Invalidate();
        }

        protected override void ClearChart()
        {
            base.ClearChart();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();       
        }

        public override void GenerateChart()
        {
            if (gameObject.activeInHierarchy == false)
                return;

            ClearChart();

            base.GenerateChart();
            
            if (((IInternalRadarData)Data).InternalDataSource == null)
                return;

            double[,] data = ((IInternalRadarData)Data).InternalDataSource.getRawData();
            int rowCount = data.GetLength(0);
            int columnCount = data.GetLength(1);
            //restrict to 3 groups
            if (rowCount <3)
                return;

            Vector3[] directions = new Vector3[rowCount];
            float[] angles = new float[rowCount];

            for (int i = 0; i < rowCount; i++)
            {
                float angle = (float)((((float)i) / (float)rowCount) * Math.PI * 2f) + Angle * Mathf.Deg2Rad;
                angles[i] = angle;
                directions[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            }

            Vector3[] path = new Vector3[rowCount];
            Vector3 zAdd = Vector3.zero;

            for (int i = 0; i < TotalAxisDevisions; i++)
            {
                float rad = Radius * ((float)(i + 1) / (float)TotalAxisDevisions);
                for (int j = 0; j < rowCount; j++)
                    path[j] = (directions[j] * rad) + zAdd;
              //  path[rowCount] = path[0];
                zAdd.z += AxisAdd;
                GameObject axisObject = CreateAxisObject(AxisThickness, path);
                mAxisObjects.Add(axisObject);
                axisObject.transform.SetParent(transform, false);

            }

            if (mGroupLabels != null && mGroupLabels.isActiveAndEnabled)
            {
                for (int i = 0; i < rowCount; i++)
                {
                    string group = Data.GetGroupName(i);
                    Vector3 basePosition = directions[i] * Radius;
                    Vector3 breadthAxis = Vector3.Cross(directions[i], Vector3.forward);
                    Vector3 position = basePosition + directions[i] * mGroupLabels.Seperation;
                    position += breadthAxis * mGroupLabels.Location.Breadth;
                    position += new Vector3(0f , 0f, mGroupLabels.Location.Depth);
                    string toSet = mGroupLabels.TextFormat.Format(group, "", "");
                    BillboardText billboard = ChartCommon.CreateBillboardText(mGroupLabels.TextPrefab, transform, toSet, position.x, position.y, position.z, angles[i],transform,hideHierarchy, mGroupLabels.FontSize, mGroupLabels.FontSharpness);
                    billboard.UserData = group;
                    TextController.AddText(billboard);
                }
            }

            double maxValue = Data.GetMaxValue();

            if (maxValue > 0.000001f)
            { 

                for (int i = 0; i < columnCount; i++)
                {
                    for (int j = 0; j < rowCount; j++)
                    {
                        float rad = ((float)(data[j, i] / maxValue)) * Radius;
                        path[j] = directions[j] * rad;
                    }
                  //  path[rowCount] = path[0];
                    GameObject category = CreateCategoryObject(path, i);
                    category.transform.SetParent(transform, false);
                }

                if (mItemLabels != null && mItemLabels.isActiveAndEnabled)
                {
                    float angle = mItemLabels.Location.Breadth;
                    float blend = (angle / 360f);
                    blend -= Mathf.Floor(blend);
                    blend *= rowCount;
                    int index = (int)blend;
                    int nextIndex = (index + 1) % rowCount;
                    blend = blend - Mathf.Floor(blend);
                    for (int i = 0; i < TotalAxisDevisions; i++)
                    { 
                        float factor = ((float)(i + 1) / (float)TotalAxisDevisions);
                        float rad = Radius * factor + mItemLabels.Seperation;
                        string value = ChartAdancedSettings.Instance.FormatFractionDigits(mItemLabels.FractionDigits,(float)(maxValue * factor));
                        Vector3 position = Vector3.Lerp(directions[index] , directions[nextIndex] , blend) * rad;
                        position.z = mItemLabels.Location.Depth;
                        string toSet = mItemLabels.TextFormat.Format(value, "", "");
                        BillboardText billboard = ChartCommon.CreateBillboardText(mItemLabels.TextPrefab, transform, toSet, position.x, position.y, position.z,0f, transform,hideHierarchy, mItemLabels.FontSize, mItemLabels.FontSharpness);
                        billboard.UserData = (float)(maxValue * factor);
                        TextController.AddText(billboard);
                    }
                }
            }

        }

        protected abstract GameObject CreateCategoryObject(Vector3[] path, int category);
        protected abstract GameObject CreateAxisObject(float thickness, Vector3[] path);

        public void OnBeforeSerialize()
        {
            ((IInternalRadarData)Data).OnBeforeSerialize();
        }

        public void OnAfterDeserialize()
        {
            ((IInternalRadarData)Data).OnAfterDeserialize();
        }

        protected override bool HasValues(AxisBase axis)
        {
            return false;
        }

        protected override double MaxValue(AxisBase axis)
        {
            return 0.0;
        }

        protected override double MinValue(AxisBase axis)
        {
            return 0.0;
        }
    }
}
