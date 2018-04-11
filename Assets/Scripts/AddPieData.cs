using UnityEngine;
using System.Collections;
using ChartAndGraph;
using System;

public class AddPieData : MonoBehaviour
{
    private PieChart pieChart;
    public EventArgs barEventArgs;

    void Start()
    {
        pieChart = GetComponent<PieChart>();
        if (pieChart != null)
        {
//            float val1 = Random.value * 5;
//            float val2 = Random.value * 5;
//            pieChart.DataSource.SetValue("ELECTIVE", "DimensionVal1", val1);
//            pieChart.DataSource.SlideValue("EMERGENCY", "DimensionVal1", val2, 1f);
        }
    }

    public void Clear()
    {
        if (pieChart != null)
        {
            Debug.Log("Clearing pieChart");
            //pieChart.DataSource.ClearCategories();
            pieChart.DataSource.Clear();
        }
    }

    public void AddCategory(string cat, Material mat)
    {
        if (pieChart != null)
        {
            Debug.Log("adding new pieChart category " + cat);
            pieChart.DataSource.AddCategory(cat, mat);
        }
    }

    public void SetValue(string category, float value)
    {
        if (pieChart != null)
        {
            //pieChart.DataSource.StartBatch();
            Debug.Log("Setting category to " + category + " and DimensionVal1 to " + value);
            //pieChart.DataSource.SetValue(category, "DimensionVal1", value);
            pieChart.DataSource.SlideValue(category, value, 0.5f);
            //pieChart.DataSource.EndBatch();
        }
    }

    public void barClicked(PieChart.PieEventArgs args)
    {
        Debug.Log(args.Category);
    }
}
