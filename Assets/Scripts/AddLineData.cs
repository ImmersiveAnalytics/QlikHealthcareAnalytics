using UnityEngine;
using System.Collections;
using ChartAndGraph;
using System;

public class AddLineData : MonoBehaviour
{
    private GraphChart lineChart;
    public EventArgs lineEventArgs;

    void Start()
    {
        lineChart = GetComponent<GraphChart>();
        if (lineChart != null)
        {
//            float val1 = Random.value * 5;
//            float val2 = Random.value * 5;
//            pieChart.DataSource.SetValue("ELECTIVE", "DimensionVal1", val1);
//            pieChart.DataSource.SlideValue("EMERGENCY", "DimensionVal1", val2, 1f);
        }
    }

    public void Clear()
    {
        if (lineChart != null)
        {
            Debug.Log("Clearing lineChart");
//            lineChart.DataSource.Clear();
            lineChart.DataSource.ClearCategory("line1");
        }
    }


    public void AddCategory(string cat, Material mat)
    {
        if (lineChart != null)
        {
            Debug.Log("adding new lineChart category " + cat);
            //lineChart.DataSource.AddCategory(cat, mat, 1f, 
        }
    }


    public void SetValue(string category, float value)
    {
        if (lineChart != null)
        {
            //lineChart.DataSource.StartBatch();
            Debug.Log("Setting point at " + category + " to " + value);
            //lineChart.DataSource.AddPointToCategory("line1", category, value);
            lineChart.DataSource.AddPointToCategory("line1", DateTime.Parse(category), value);
            //lineChart.DataSource.EndBatch();
        }
    }

//    public void pointClicked(GraphChart. args)
//    {
//        Debug.Log(args.Category);
//    }
}
