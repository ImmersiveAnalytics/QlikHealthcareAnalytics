using UnityEngine;
using System.Collections;
using ChartAndGraph;

public class GraphChartFeed : MonoBehaviour
{
	void Start ()
    {
        GraphChartBase graph = GetComponent<GraphChartBase>();
        if (graph != null)
        {
            graph.DataSource.StartBatch();
            graph.DataSource.ClearCategory("Player 1");
            graph.DataSource.ClearCategory("Player 2");
            for (int i = 0; i <70; i++)
            {
                graph.DataSource.AddPointToCategory("Player 1",Random.value*10f,Random.value*10f);
                graph.DataSource.AddPointToCategory("Player 2", Random.value * 10f, Random.value * 10f);
            }
            graph.DataSource.EndBatch();
        }
    }
}
