using UnityEngine;
using System.Collections;

public class Randomizer : MonoBehaviour
{

    private SplineFormer splineFormer;
    // Use this for initialization
    void Start()
    {
        splineFormer = GetComponent<SplineFormer>();
    }

    void Update()
    {
        splineFormer.LoftAngle = 180 * Mathf.Sin(Time.timeSinceLevelLoad);
        splineFormer.InvalidateMesh();
    }
}
