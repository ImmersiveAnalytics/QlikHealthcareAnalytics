using UnityEngine;
using System.Collections;
using ChartAndGraph;
using System;

public class AddBarData : MonoBehaviour
{
    public BarChart barChart;
    public EventArgs barEventArgs;

    void Start()
    {
        barChart = GetComponent<BarChart>();
        if (barChart != null)
        {
//            float val1 = Random.value * 5;
//            float val2 = Random.value * 5;
//            barChart.DataSource.SetValue("ELECTIVE", "DimensionVal1", val1);
//            barChart.DataSource.SlideValue("EMERGENCY", "DimensionVal1", val2, 1f);
        }
    }

    public void Clear()
    {
        if (barChart != null)
        {
            Debug.Log("Clearing barchart");
            barChart.DataSource.ClearCategories();
            //barChart.DataSource.Clear();
        }
    }

    public void AddCategory(string cat, Material mat)
    {
        if (barChart != null)
        {
            Debug.Log("adding new barchart category " + cat);
            barChart.DataSource.AddCategory(cat, mat);
            
        }
    }

    public void SetValue(string category, float value)
    {
        if (barChart != null)
        {
            //barChart.DataSource.StartBatch();
            Debug.Log("Setting category to " + category + " and DimensionVal1 to " + value);
            //barChart.DataSource.SetValue(category, "DimensionVal1", value);
            barChart.DataSource.SlideValue(category, "DimensionVal1", value, 0.5f);
            //barChart.DataSource.EndBatch();
            barChart.DataSource.MinValue = 0;
        }
    }

    public void barClicked(BarChart.BarEventArgs args)
    {
        Debug.Log(args.Category);
    }
}
