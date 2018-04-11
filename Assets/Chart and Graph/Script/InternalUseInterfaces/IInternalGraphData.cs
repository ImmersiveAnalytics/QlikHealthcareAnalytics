using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChartAndGraph
{
    interface IInternalGraphData
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="axis">0 for horizontal 1 for vertical</param>
        /// <returns></returns>
        double GetMinValue(int axis);
        double GetMaxValue(int axis);
        void OnBeforeSerialize();
        void OnAfterDeserialize();
        event EventHandler InternalDataChanged;
        int TotalCategories { get; }
        IEnumerable<GraphData.CategoryData> Categories { get; }
    }
}
