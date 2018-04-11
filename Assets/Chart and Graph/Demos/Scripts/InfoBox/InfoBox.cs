using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace ChartAndGraph
{
    /// <summary>
    /// this class demonstrates the use of chart events
    /// </summary>
    public class InfoBox : MonoBehaviour
    {
        public PieChart[] PieChart;
        public BarChart[] BarChart;
        public Text infoText; 
         
        void BarHovered(BarChart.BarEventArgs args)
        {
            infoText.text = string.Format("({0},{1}) : {2}", args.Category, args.Group, args.Value);
        }

        void PieHovered(PieChart.PieEventArgs args)
        {
            infoText.text = string.Format("{0} : {1}", args.Category, args.Value);
        }

        void NonHovered()
        {
            infoText.text = "";
        }

        public void HookChartEvents()
        {
            if (PieChart != null)
            {
                foreach (PieChart pie in PieChart)
                {
                    if (pie == null)
                        continue;
                    pie.PieHovered.AddListener(PieHovered);        // add listeners for the pie chart events
                    pie.NonHovered.AddListener(NonHovered);
                }
            }
            if (BarChart != null)
            {
                foreach (BarChart bar in BarChart)
                {
                    if (bar == null)
                        continue;
                    bar.BarHovered.AddListener(BarHovered);        // add listeners for the bar chart events
                    bar.NonHovered.AddListener(NonHovered);
                }
            }
        }

        // Use this for initialization
        void Start()
        {
            HookChartEvents();
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}