using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;

public class Sankey : MonoBehaviour
{

    public Mesh SegmentMesh;
    public Material _material1;
    public Material _material2;
    public Material _material3;
    public GameObject patient;
    public GameObject Canvas;

    private List<GameObject> pathGOs = new List<GameObject>();
    private static float chartHeight = 1.0f;
    private static float chartLength = 1.0f;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // Gets new data from Sense
    public void NewData(string d)
    {
        // Remove any existing sankey
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        transform.rotation = Quaternion.identity;
        transform.position = new Vector3(0, 0, 0);
        pathGOs.Clear();

        Debug.Log("data: " + d);
        JSONNode JPaths = JSON.Parse(d);
        //var pathList = new List<Path>();
        var Paths = new Dictionary<int, Dictionary<string, int>>();
        var CompletedPaths = new Dictionary<int, float>();
        var Groups = new Dictionary<int, Dictionary<string, float>>();
        int MaxSteps = 0;
        for (int i = 0; i < JPaths.AsArray.Count; i++)
        {
            // Splice path
            string p = JPaths[i];
            //Debug.Log("path: " + p);
            string path = p.Substring(1, p.Length - 2);
            string[] nodes = path.Split(',');
            if (nodes.Length < 2)
                continue;

            for (int j = 0; j < nodes.Length; j++)
            {
                string n = nodes[j];
                //Debug.Log("node: " + n);
                if (Paths.ContainsKey(j))
                {
                    if (Paths[j].ContainsKey(n))
                        Paths[j][n]++;
                    else
                        Paths[j][n] = 1;
                }
                else
                {
                    Paths.Add(j, new Dictionary<string, int>());
                    Paths[j][n] = 1;
                }
                MaxSteps = MaxSteps > Paths[j][n] ? MaxSteps : Paths[j][n];
            }
        }

        for (int i = 0; i < JPaths.AsArray.Count; i++)
        {
            string p = JPaths[i];
            string path = p.Substring(1, p.Length - 2);
            string[] nodes = path.Split(',');
            if (nodes.Length < 2)
                continue;
            if (nodes.Length < Paths.Count)
                Paths[Paths.Count - 1][nodes[nodes.Length - 1]] = 1;
        }
        Debug.Log("Last pos length: " + Paths[Paths.Count - 1].Count);

        for (int i = 0; i < JPaths.AsArray.Count; i++)
        {
            // Splice path
            string p = JPaths[i];
            Debug.Log("path: " + p);
            string path = p.Substring(1, p.Length - 2);
            string[] nodes = path.Split(',');
            if (nodes.Length < 2)
                continue;

            GameObject go = createSpline();
            Vector3 pp = transform.position;
            //            go.transform.position = new Vector3(pp.x, chartHeight / (float)JPaths.AsArray.Count * (float)i, pp.z);
            go.transform.position = new Vector3(pp.x, pp.y, pp.z);
            pathGOs.Add(go);

            // Setup nodes
            SplineFormer sf = go.GetComponent<SplineFormer>();

            for (int j = 0; j < nodes.Length; j++)
            {
                string n = nodes[j];
                //Debug.Log("node: " + n);

                //                float zStep= chartLength / (nodes.Length-1);
                float zStep = chartLength / MaxSteps;
                //                float zPos = zStep * (float)j;
                float zPos;
                zPos = j < (nodes.Length - 1) ? zStep * (float)j : chartLength;

                if (j < nodes.Length - 1)
                {
                    if (CompletedPaths.ContainsKey(j))
                        CompletedPaths[j]++;
                    else
                        CompletedPaths.Add(j, 1f);
                }
                else
                {
                    if (CompletedPaths.ContainsKey(Paths[Paths.Count - 1].Count))
                        CompletedPaths[Paths[Paths.Count - 1].Count]++;
                    else
                        CompletedPaths.Add(Paths[Paths.Count - 1].Count, 1f);
                }

                SplineNode sn = null;
                if (j > 1)
                {
                    sn = sf.AddNodeImmediately();
                    Vector3 f = sn.transform.position;
                    float hOffset;
                    if (j < nodes.Length - 1)
                    {
                        if (Groups.ContainsKey(j))
                        {
                            if (Groups[j].ContainsKey(n))
                            {
                                hOffset = Groups[j][n];
                            }
                            else
                            {
                                Groups[j][n] = chartHeight / (float)(Paths[j].Count + 1) * CompletedPaths[j];
                                hOffset = Groups[j][n];
                            }
                        }
                        else
                        {
                            Groups.Add(j, new Dictionary<string, float>());
                            Groups[j][n] = chartHeight / (float)(Paths[j].Count + 1) * CompletedPaths[j];
                            hOffset = Groups[j][n];
                        }
                    }
                    else
                    {
                        //hOffset = chartHeight / (float)(Paths[Paths.Count - 1].Count + 1) * CompletedPaths[Paths[Paths.Count - 1].Count];
                        if (Groups.ContainsKey(Paths.Count - 1))
                        {
                            if (Groups[Paths.Count - 1].ContainsKey(n))
                            {
                                hOffset = Groups[Paths.Count - 1][n];
                            }
                            else
                            {
                                Groups[Paths.Count - 1][n] = chartHeight / (float)(Paths[Paths.Count - 1].Count + 1) * CompletedPaths[Paths[Paths.Count - 1].Count];
                                hOffset = Groups[Paths.Count - 1][n];
                            }
                        }
                        else
                        {
                            Groups.Add(Paths.Count - 1, new Dictionary<string, float>());
                            Groups[Paths.Count - 1][n] = chartHeight / (float)(Paths[Paths.Count - 1].Count + 1) * CompletedPaths[Paths[Paths.Count - 1].Count];
                            hOffset = Groups[Paths.Count - 1][n];
                        }
                    }


                    f = f + Vector3.up * hOffset;
                    sn.transform.position = new Vector3(f.x, f.y, zPos);
                }
                else if (j == 1)
                {
                    sn = sf.Nodes[j];
                    Vector3 f = sn.transform.position;
                    float hOffset;
                    if (j < nodes.Length - 1)
                    {
                        //Debug.Log("Path count: " + Paths[j][n]);
                        if (Paths[j][n] > 1)
                        {
                            if (Groups.ContainsKey(j))
                            {
                                if (Groups[j].ContainsKey(n))
                                {
                                    hOffset = Groups[j][n];
                                }
                                else
                                {
                                    Groups[j][n] = chartHeight / (float)(Paths[j].Count + 1) * CompletedPaths[j];
                                    hOffset = Groups[j][n];
                                }
                            }
                            else
                            {
                                Groups.Add(j, new Dictionary<string, float>());
                                Groups[j][n] = chartHeight / (float)(Paths[j].Count + 1) * CompletedPaths[j];
                                hOffset = Groups[j][n];
                            }
                        }
                        else
                            hOffset = chartHeight / (float)(Paths[j].Count + 1) * CompletedPaths[j];
                    }
                    else
                    {
                        //hOffset = chartHeight / (float)(Paths[Paths.Count - 1].Count + 1) * CompletedPaths[Paths[Paths.Count - 1].Count];
                        if (Groups.ContainsKey(Paths.Count - 1))
                        {
                            if (Groups[Paths.Count - 1].ContainsKey(n))
                            {
                                hOffset = Groups[Paths.Count - 1][n];
                            }
                            else
                            {
                                Groups[Paths.Count - 1][n] = chartHeight / (float)(Paths[Paths.Count - 1].Count + 1) * CompletedPaths[Paths[Paths.Count - 1].Count];
                                hOffset = Groups[Paths.Count - 1][n];
                            }
                        }
                        else
                        {
                            Groups.Add(Paths.Count - 1, new Dictionary<string, float>());
                            Groups[Paths.Count - 1][n] = chartHeight / (float)(Paths[Paths.Count - 1].Count + 1) * CompletedPaths[Paths[Paths.Count - 1].Count];
                            hOffset = Groups[Paths.Count - 1][n];
                        }
                    }

                    f = f + Vector3.up * hOffset;
                    sn.transform.position = new Vector3(f.x, f.y, zPos);
                }
                else if (j == 0)
                {
                    sn = sf.Nodes[j];
                    Vector3 f = sn.transform.position;

                    float yStep = chartHeight / (float)JPaths.AsArray.Count;
                    ////                    f = f + Vector3.up * -((float)i-1f) * yStep;
                    //                    f = f + Vector3.up * chartHeight / (float)(Paths[j].Count+1);
                    //Debug.Log("NumNodesAtPos " + j + ": " + Paths[j].Count + " for path " + i + " = " + chartHeight / (float)(Paths[j].Count + 1));

                    if (Groups.ContainsKey(j))
                    {
                        if (!Groups[j].ContainsKey(n))
                        {
                            Groups[j][n] = chartHeight / (float)(Paths[j].Count + 1);
                        }
                    }
                    else
                    {
                        Groups.Add(j, new Dictionary<string, float>());
                        Groups[j][n] = chartHeight / (float)(Paths[j].Count + 1);
                    }

                    f = f + Vector3.up * Groups[j][n];
                    sn.transform.position = f;
                }

                // Add Sphere Nodes to nodes
                GameObject spNode = (GameObject)Instantiate(Resources.Load("SankeyNode"), sn.transform.position, sn.transform.rotation);
                Vector3 vec = new Vector3(0f, 1f, 0f);
                spNode.transform.Rotate(vec, 180f);
                spNode.transform.SetParent(sn.transform);
                float _nodeScaler = Mathf.Lerp(0.1f, 0.2f, Paths[j][n] / MaxSteps);
                spNode.transform.localScale = new Vector3(_nodeScaler, _nodeScaler, _nodeScaler);

                // Add Text Labels to nodes
                GameObject label = spNode.transform.GetChild(0).gameObject;
                label.transform.RotateAround(sn.transform.position, Vector3.up, 90);
                GameObject txt = label.transform.GetChild(0).gameObject;
                txt.GetComponent<UnityEngine.UI.Text>().text = n;

                // Add Tap Handler
                SelectMgr hit = spNode.AddComponent<SelectMgr>();
            }
            sf.InvalidateMesh();


        }

        // Move Sankey above patient
        Vector3 pPos = patient.transform.position;
        pPos.x -= 0.4f;
        pPos.y += 0.8f;
        pPos.z -= 0.5f;
        gameObject.transform.position = pPos;
        Quaternion pRot = patient.transform.rotation;
        pRot.y -= Mathf.PI * 0.25f;
        gameObject.transform.rotation = pRot;

        ReTag(gameObject.transform, LayerMask.NameToLayer("Sankey"));
    }

    void ReTag(Transform _transform, int _layer)
    {
        foreach (Transform child in _transform) { child.gameObject.layer = _layer; ReTag(child, _layer); }
    }

    public GameObject createSpline()
    {
        // Create new GameObject to hold spline
        GameObject go = new GameObject("Spline");
        go.transform.SetParent(gameObject.transform, false);

        // Create a new SplineFormer
        SplineFormer _splineFormer = go.AddComponent<SplineFormer>();
        _splineFormer.SegmentsNumber = 20;
        _splineFormer.Coefficient = 0.3f;
        _splineFormer.LoftDirection = new Vector3(0, 1, 0);
        _splineFormer.SegmentScale = new Vector3(0.04f, 0.04f, 0.04f);
        _splineFormer.LoftingGroups[0].Weld = false;

        // hide nodes
        _splineFormer.VisualOptions.NodeSize = 0.05f;
        _splineFormer.VisualOptions.ShowTangentNodes = false;
        _splineFormer.VisualOptions.ShowTangents = false;
        _splineFormer.VisualOptions.ShowNodeLinks = false;

        // associate segment mesh
        _splineFormer.LoftingGroups[0].SegmentMesh = SegmentMesh;
        // create & associate MeshFilter
        MeshFilter _meshFilter = go.AddComponent<MeshFilter>();
        _splineFormer.LoftingGroups[0].MeshFilter = _meshFilter;
        // create & associate MeshCollider
        MeshCollider _meshCollider = go.AddComponent<MeshCollider>();
        _splineFormer.LoftingGroups[0].MeshCollider = _meshCollider;
        // create MeshRenderer & Material
        MeshRenderer _meshMeshRenderer = go.AddComponent<MeshRenderer>();
        _meshMeshRenderer.material = new Material(Shader.Find("Standard"));

        Color[] palette = new Color[7] { Color.blue, Color.cyan, Color.gray, Color.green, Color.magenta, Color.red, Color.yellow};
        _meshMeshRenderer.material.color = palette[pathGOs.Count % palette.Length];

        return go;
    }


    // Update Buttons with exclusion data from Sense
    public void updateUI(string d)
    {
        Debug.Log("data: " + d);
        JSONNode JPaths = JSON.Parse(d);

        if (JPaths["smokers"]["Y"].ToString() == "\"X\"")
            Canvas.transform.Find("Toggle-Smoker-Y").GetComponent<Toggle>().interactable = false;
        else
            Canvas.transform.Find("Toggle-Smoker-Y").GetComponent<Toggle>().interactable = true;
        if (JPaths["smokers"]["N"].ToString() == "\"X\"")
            Canvas.transform.Find("Toggle-Smoker-N").GetComponent<Toggle>().interactable = false;
        else
            Canvas.transform.Find("Toggle-Smoker-N").GetComponent<Toggle>().interactable = true;
        if (JPaths["BMI"]["Y"].ToString() == "\"X\"")
            Canvas.transform.Find("Toggle-BMI-Y").GetComponent<Toggle>().interactable = false;
        else
            Canvas.transform.Find("Toggle-BMI-Y").GetComponent<Toggle>().interactable = true;
        if (JPaths["BMI"]["N"].ToString() == "\"X\"")
            Canvas.transform.Find("Toggle-BMI-N").GetComponent<Toggle>().interactable = false;
        else
            Canvas.transform.Find("Toggle-BMI-N").GetComponent<Toggle>().interactable = true;
        if (JPaths["diabetes"]["Y"].ToString() == "\"X\"")
            Canvas.transform.Find("Toggle-Diabetes-Y").GetComponent<Toggle>().interactable = false;
        else
            Canvas.transform.Find("Toggle-Diabetes-Y").GetComponent<Toggle>().interactable = true;
        if (JPaths["diabetes"]["N"].ToString() == "\"X\"")
            Canvas.transform.Find("Toggle-Diabetes-N").GetComponent<Toggle>().interactable = false;
        else
            Canvas.transform.Find("Toggle-Diabetes-N").GetComponent<Toggle>().interactable = true;
        if (JPaths["gender"]["Male"].ToString() == "\"X\"")
            Canvas.transform.Find("Toggle-Male").GetComponent<Toggle>().interactable = false;
        else
            Canvas.transform.Find("Toggle-Male").GetComponent<Toggle>().interactable = true;
        if (JPaths["gender"]["Female"].ToString() == "\"X\"")
            Canvas.transform.Find("Toggle-Female").GetComponent<Toggle>().interactable = false;
        else
            Canvas.transform.Find("Toggle-Female").GetComponent<Toggle>().interactable = true;

    }

    // Clear Buttons with exclusion data from Sense
    public void clearUI()
    {
        Canvas.transform.Find("Toggle-Smoker-Y").GetComponent<Toggle>().interactable = true;
        Canvas.transform.Find("Toggle-Smoker-Y").GetComponent<Toggle>().isOn = false;
        Canvas.transform.Find("Toggle-Smoker-N").GetComponent<Toggle>().interactable = true;
        Canvas.transform.Find("Toggle-Smoker-N").GetComponent<Toggle>().isOn = false;
        Canvas.transform.Find("Toggle-BMI-Y").GetComponent<Toggle>().interactable = true;
        Canvas.transform.Find("Toggle-BMI-Y").GetComponent<Toggle>().isOn = false;
        Canvas.transform.Find("Toggle-BMI-N").GetComponent<Toggle>().interactable = true;
        Canvas.transform.Find("Toggle-BMI-N").GetComponent<Toggle>().isOn = false;
        Canvas.transform.Find("Toggle-Diabetes-Y").GetComponent<Toggle>().interactable = true;
        Canvas.transform.Find("Toggle-Diabetes-Y").GetComponent<Toggle>().isOn = false;
        Canvas.transform.Find("Toggle-Diabetes-N").GetComponent<Toggle>().interactable = true;
        Canvas.transform.Find("Toggle-Diabetes-N").GetComponent<Toggle>().isOn = false;
        Canvas.transform.Find("Toggle-Male").GetComponent<Toggle>().interactable = true;
        Canvas.transform.Find("Toggle-Male").GetComponent<Toggle>().isOn = false;
        Canvas.transform.Find("Toggle-Female").GetComponent<Toggle>().interactable = true;
        Canvas.transform.Find("Toggle-Female").GetComponent<Toggle>().isOn = false;

    }
}
