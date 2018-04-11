using ChartAndGraph.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChartAndGraph
{
    [Serializable]
    public class GraphData : IInternalGraphData
    {
        private event EventHandler DataChanged;
        private bool mSuspendEvents = false;

        [Serializable]
        public class CategoryData
       {
            public string Name;
            public List<Vector2> Data = new List<Vector2>();
            public float? MaxX, MaxY, MinX, MinY;
            public Material LineMaterial;
            public MaterialTiling LineTiling;
            public float LineThickness = 1f;
            public Material FillMaterial;
            public bool StetchFill = false;
            public Material PointMaterial;
            public float PointSize = 5f;
            public PathGenerator LinePrefab;
            public FillPathGenerator FillPrefab;
            public GameObject DotPrefab;
            public float Depth = 0f;
        }

        class VectorComparer : IComparer<Vector2>
        {
            public int Compare(Vector2 x, Vector2 y)
            {
                if (x.x < y.x)
                    return -1;
                if (x.x > y.x)
                    return 1;
                return 0;

            }
        }

        [Serializable]
        class SerializedCategory
        {
            public string Name;
            [NonCanvasAttribute]
            public float Depth;
            [HideInInspector]
            public Vector2[] data;
            [HideInInspector]
            public float? MaxX, MaxY, MinX, MinY;
            [NonCanvasAttribute]
            public PathGenerator LinePrefab;
            public Material Material;
            public MaterialTiling LineTiling;
            [NonCanvasAttribute]
            public FillPathGenerator FillPrefab;
            public Material InnerFill;
            public float LineThickness = 1f;
            public bool StetchFill = false;
            [NonCanvasAttribute]
            public GameObject DotPrefab;
            public Material PointMaterial;
            public float PointSize;
        }

        VectorComparer mComparer = new VectorComparer();
        Dictionary<string, CategoryData> mData = new Dictionary<string, CategoryData>();

        [SerializeField]
        SerializedCategory[] mSerializedData = new SerializedCategory[0];
        private void RaiseDataChanged()
        {
            if (mSuspendEvents)
                return;
            if (DataChanged != null)
                DataChanged(this, EventArgs.Empty);
        }
        /// <summary>
        /// call this to suspend chart redrawing while updating the data of the chart
        /// </summary>
        public void StartBatch()
        {
            mSuspendEvents = true;
        }
        /// <summary>
        /// call this after StartBatch , this will apply all the changed made between the StartBatch call to this call
        /// </summary>
        public void EndBatch()
        {
            mSuspendEvents = false;
            RaiseDataChanged();
        }
        event EventHandler IInternalGraphData.InternalDataChanged
        {
            add
            {
                DataChanged += value;
            }

            remove
            {
                DataChanged -= value;
            }
        }
        /// <summary>
        /// rename a category. throws and exception on error
        /// </summary>
        /// <param name="prevName"></param>
        /// <param name="newName"></param>
        public void RenameCategory(string prevName,string newName)
        {
            if (prevName == newName)
                return;
            if(mData.ContainsKey(newName))
                throw new ArgumentException(String.Format("A category named {0} already exists", newName));
            CategoryData cat = mData[prevName];
            mData.Remove(prevName);
            cat.Name = newName;
            mData.Add(newName, cat);
            RaiseDataChanged();
        }

        /// <summary>
        /// Adds a new category to the graph chart. each category corrosponds to a graph line. 
        /// </summary>
        /// <param name="category"></param>
        /// <param name="material"></param>
        /// <param name="innerFill"></param>
        public void AddCategory(string category, Material lineMaterial,float lineThickness,MaterialTiling lineTiling, Material innerFill,bool strechFill,Material pointMaterial,float pointSize)
        {
            if (mData.ContainsKey(category))
                throw new ArgumentException(String.Format("A category named {0} already exists", category));
            CategoryData data = new CategoryData();
            mData.Add(category, data);
            data.Name = category;
            data.LineMaterial = lineMaterial;
            data.FillMaterial = innerFill;
            data.LineThickness = lineThickness;
            data.LineTiling = lineTiling;
            data.StetchFill = strechFill;
            data.PointMaterial = pointMaterial;
            data.PointSize = pointSize;
            RaiseDataChanged();
        }

        public void AddCategory3DGraph(string category,PathGenerator linePrefab, Material lineMaterial, float lineThickness, MaterialTiling lineTiling, FillPathGenerator fillPrefab, Material innerFill, bool strechFill,GameObject pointPrefab, Material pointMaterial, float pointSize,float depth)
        {
            if (mData.ContainsKey(category))
                throw new ArgumentException(String.Format("A category named {0} already exists", category));
            if (depth < 0f)
                depth = 0f;
            CategoryData data = new CategoryData();
            mData.Add(category, data);
            data.Name = category;
            data.LineMaterial = lineMaterial;
            data.FillMaterial = innerFill;
            data.LineThickness = lineThickness;
            data.LineTiling = lineTiling;
            data.StetchFill = strechFill;
            data.PointMaterial = pointMaterial;
            data.PointSize = pointSize;
            data.LinePrefab = linePrefab;
            data.FillPrefab = fillPrefab;
            data.DotPrefab = pointPrefab;
            data.Depth = depth;
            RaiseDataChanged();
        }

        /// <summary>
        /// sets the line style for the category
        /// </summary>
        /// <param name="category"></param>
        /// <param name="lineMaterial"></param>
        /// <param name="lineThickness"></param>
        /// <param name="lineTiling"></param>
        public void SetCategoryLine(string category,Material lineMaterial,float lineThickness, MaterialTiling lineTiling)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }
            CategoryData data = mData[category];
            data.LineMaterial = lineMaterial;
            data.LineThickness = lineThickness;
            data.LineTiling = lineTiling;
            RaiseDataChanged();
        }

        /// <summary>
        /// clears all graph data
        /// </summary>
        public void Clear()
        {
            mData.Clear();
        }

        /// <summary>
        /// removed a category from the DataSource. returnes true on success , or false if the category does not exist
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public bool RemoveCategory(string category)
        {
            return mData.Remove(category);
        }

        /// <summary>
        /// sets the point style for the selected category. set material to null for no points
        /// </summary>
        /// <param name="category"></param>
        /// <param name="pointMaterial"></param>
        /// <param name="pointSize"></param>
        public void SetCategoryPoint(string category,Material pointMaterial,float pointSize)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }

            CategoryData data = mData[category];
            data.PointMaterial = pointMaterial;
            data.PointSize = pointSize;
            RaiseDataChanged();
        }

        /// <summary>
        /// sets the prefabs for a 3d graph category,
        /// </summary>
        /// <param name="category"></param>
        /// <param name="linePrefab"></param>
        /// <param name="fillPrefab"></param>
        /// <param name="dotPrefab"></param>
        public void Set3DCategoryPrefabs(string category,PathGenerator linePrefab,FillPathGenerator fillPrefab, GameObject dotPrefab)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }
            CategoryData data = mData[category];
            data.LinePrefab = linePrefab;
            data.DotPrefab = dotPrefab;
            data.FillPrefab = fillPrefab;
            RaiseDataChanged();
        }

        /// <summary>
        /// sets the depth for a 3d graph category
        /// </summary>
        /// <param name="category"></param>
        /// <param name="depth"></param>
        public void Set3DCategoryDepth(string category,float depth)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }
            if (depth < 0)
                depth = 0f;
            CategoryData data = mData[category];
            data.Depth = depth;
            RaiseDataChanged();
        }

        /// <summary>
        /// sets the fill style for the selected category.set the material to null for no fill
        /// </summary>
        /// <param name="category"></param>
        /// <param name="fillMaterial"></param>
        /// <param name="strechFill"></param>
        public void SetCategoryFill(string category,Material fillMaterial,bool strechFill)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }
            CategoryData data = mData[category];
            data.FillMaterial = fillMaterial;
            data.StetchFill = strechFill;
            RaiseDataChanged();
        }

        /// <summary>
        /// clears all the data for the selected category
        /// </summary>
        /// <param name="category"></param>
        public void ClearCategory(string category)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }
            mData[category].MaxX = null;
            mData[category].MaxY = null;
            mData[category].MinX = null;
            mData[category].MinY = null;
            mData[category].Data.Clear();
            RaiseDataChanged();
        }

        /// <summary> 
        /// adds a point to the category. having the point x,y values as dates
        /// <param name="category"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddPointToCategory(string category, DateTime x, DateTime y)
        {
            double xVal = ChartDateUtility.DateToValue(x);
            double yVal = ChartDateUtility.DateToValue(y);
            AddPointToCategory(category, (float)xVal, (float)yVal);
        }

        /// <summary>
        /// adds a point to the category. having the point x value as date
        /// <param name="category"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddPointToCategory(string category, DateTime x, float y)
        {
            double xVal = ChartDateUtility.DateToValue(x);
            AddPointToCategory(category, (float)xVal,y);
        }
        /// <summary>
        /// adds a point to the category. having the point y value as date
        /// </summary>
        /// <param name="category"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddPointToCategory(string category,float x,DateTime y)
        {
            double yVal = ChartDateUtility.DateToValue(y);
            AddPointToCategory(category, x, (float)yVal);
        }

        /// <summary>
        /// adds a point to the category. The points are sorted by their x value automatically
        /// </summary>
        /// <param name="category"></param>
        /// <param name="point"></param>
        public void AddPointToCategory(string category, float x,float y)
        {
            if (mData.ContainsKey(category) == false)
            {
                Debug.LogWarning("Invalid category name. Make sure the category is present in the graph");
                return;
            }
            Vector2 point = new Vector2(x, y);
            CategoryData data = mData[category];
            List<Vector2> points = data.Data;

            if (data.MaxX.HasValue == false || data.MaxX.Value < point.x)
                data.MaxX = point.x;
            if (data.MinX.HasValue == false || data.MinX.Value > point.x)
                data.MinX = point.x;
            if (data.MaxY.HasValue == false || data.MaxY.Value < point.y)
                data.MaxY = point.y;
            if (data.MinY.HasValue == false || data.MinY.Value > point.y)
                data.MinY = point.y;

            if (points.Count > 0)
            {
                if (points[points.Count - 1].x < point.x)
                {
                    points.Add(point);
                    return;
                }
            }

            int search = points.BinarySearch(point, mComparer);
            if (search < 0)
                search = ~search;
            points.Insert(search, point);
            RaiseDataChanged();
        }

        double IInternalGraphData.GetMaxValue(int axis)
        {
            double? res = null;
            foreach (CategoryData cat in mData.Values)
            {
                if (axis == 0)
                {
                    if (res.HasValue == false || res.Value < cat.MaxX)
                        res = cat.MaxX;
                }
                else
                {
                    if (res.HasValue == false || res.Value < cat.MaxY)
                        res = cat.MaxY;
                }
            }
            if (res.HasValue == false)
                return (ChartCommon.IsInEditMode) ? 10.0 : 0.0;
            return res.Value;
        }

        double IInternalGraphData.GetMinValue(int axis)
        {
            double? res = null;
            foreach (CategoryData cat in mData.Values)
            {
                if (axis == 0)
                {
                    if (res.HasValue == false || res.Value > cat.MinX)
                        res = cat.MinX;
                }
                else
                {
                    if (res.HasValue == false || res.Value > cat.MinY)
                        res = cat.MinY;
                }
            }
            if (res.HasValue == false)
                return 0.0;
            return res.Value;
        }

        void IInternalGraphData.OnAfterDeserialize()
        {
            if (mSerializedData == null)
                return;
            mData.Clear();
            mSuspendEvents = true;
            for (int i = 0; i < mSerializedData.Length; i++)
            {
                SerializedCategory cat = mSerializedData[i];
                if (cat.Depth < 0)
                    cat.Depth = 0f;
                string name = cat.Name;
                AddCategory3DGraph(name,cat.LinePrefab, cat.Material, cat.LineThickness, cat.LineTiling,cat.FillPrefab, cat.InnerFill,cat.StetchFill,cat.DotPrefab,cat.PointMaterial,cat.PointSize,cat.Depth);
                CategoryData data = mData[name];
                data.Data.AddRange(cat.data);
                data.MaxX = cat.MaxX;
                data.MaxY = cat.MaxY;
                data.MinX = cat.MinX;
                data.MinY = cat.MinY;
            }
            mSuspendEvents = false;
        }

        void IInternalGraphData.OnBeforeSerialize()
        {
            List<SerializedCategory> serialized = new List<SerializedCategory>();
            foreach (KeyValuePair<string, CategoryData> pair in mData)
            {
                SerializedCategory cat = new SerializedCategory();
                cat.Name = pair.Key;
                cat.MaxX = pair.Value.MaxX;
                cat.MinX = pair.Value.MinX;
                cat.MaxY = pair.Value.MaxY;
                cat.MinY = pair.Value.MinY;
                cat.LineThickness = pair.Value.LineThickness;
                cat.StetchFill = pair.Value.StetchFill;
                cat.Material = pair.Value.LineMaterial;
                cat.LineTiling = pair.Value.LineTiling;
                cat.InnerFill = pair.Value.FillMaterial;
                cat.data = pair.Value.Data.ToArray();
                cat.PointSize = pair.Value.PointSize;
                cat.PointMaterial = pair.Value.PointMaterial;
                cat.LinePrefab = pair.Value.LinePrefab;
                cat.Depth = pair.Value.Depth;
                cat.DotPrefab = pair.Value.DotPrefab;
                cat.FillPrefab = pair.Value.FillPrefab;
                if (cat.Depth < 0)
                    cat.Depth = 0f;
                serialized.Add(cat);
            }
            mSerializedData = serialized.ToArray();
        }
        int IInternalGraphData.TotalCategories
        {
            get { return mData.Count; }
        }
        IEnumerable<CategoryData> IInternalGraphData.Categories
        {
            get
            {
                return mData.Values;
            }
        }
    }
}
