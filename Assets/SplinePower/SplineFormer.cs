using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

static public class SplinePowerVector3Extensions
{
    static public Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);

        var at3 = (1f - t) * (1f - t) * (1f - t);
        var at2 = (1f - t) * (1f - t);
        var at1 = (1f - t);
        var t2 = t * t;
        var t3 = t * t * t;

        return at3 * p0 + 3 * t * at2 * p1 + 3 * t2 * at1 * p2 + t3 * p3;
    }

    static public float InverseLerp(Vector3 from, Vector3 to, Vector3 projected)
    {
        Vector3 line = to - from;
        return Mathf.InverseLerp(0f, line.magnitude, projected.magnitude);
    }

    static public Vector3 Mul(this Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }
}

[ExecuteInEditMode]
[AddComponentMenu("Spline Power!/SplineFormer")]
public class SplineFormer : MonoBehaviour
{
    public float Coefficient = 0.25f;

    public float LoftAngle = 0f;

    public Vector3 LoftDirection = Vector3.forward;
    public Vector3 SegmentScale = Vector3.one;

    public List<LoftingGroup> LoftingGroups;

    public bool Loop = false;

    [HideInInspector]
    public List<SplineNode> Nodes = new List<SplineNode>();

    public int SegmentsNumber;
    public bool QuadraticSmooth = true;
    public bool RollerCoasterFix = false;

    public ExportOptionsContainer ExportOptions = new ExportOptionsContainer();
    public VisualOptionsContainer VisualOptions = new VisualOptionsContainer();

    private bool _needAddNode = false;

    private bool _needRebuildMesh = true;
    private float _splineLength;

    /// <summary>
    /// Gets the end node.
    /// </summary>
    /// <value>The end node.</value>
    //public SplineNode EndNode { get { return Nodes[Nodes.Count - 1]; } }
    private SplineNode _endNode;
    public SplineNode EndNode
    {
        get { return _endNode; }
    }

    public LoftingGroup FirstGroup
    {
        get
        {
            if (LoftingGroups == null) LoftingGroups = new List<LoftingGroup>();
            if (LoftingGroups.Count == 0) LoftingGroups.Add(new LoftingGroup());
            if (LoftingGroups[0] == null) LoftingGroups[0] = new LoftingGroup();
            return LoftingGroups[0];
        }
    }

    /// <summary>
    /// Gets the spline lenght.
    /// </summary>
    /// <value>The spline lenght.</value>
    public float SplineLenght
    {
        get
        {
            return _splineLength;
        }
    }

    /// <summary>
    /// Gets the start node.
    /// </summary>
    /// <value>The start node.</value>
    //public SplineNode StartNode { get { return Nodes[0]; } }
    private SplineNode _startNode;
    public SplineNode StartNode
    {
        get { return _startNode; }
    }

    /// <summary>
    /// Requests to add one new node. Node will be added during the next Update call
    /// </summary>
    public void AddNewNode()
    {
        _needAddNode = true;
    }

    /// <summary>
    /// Adds the node immediately.
    /// </summary>
    /// <returns>added node</returns>
    /// <param name="after">node after which should be the new one.</param>
    public SplineNode AddNodeImmediately(SplineNode after = null)
    {
        GameObject go = new GameObject();
        go.transform.parent = transform;
        go.transform.position = transform.position;
        var node = go.AddComponent<SplineNode>();
        node.SplineFormer = this;
        if (Nodes.Count < 1)
        {
            Nodes.Add(node);
        }
        else
        {
            if (after == null)
            {
                Nodes.Add(node);
            }
            else
            {
                Nodes.Insert(Nodes.IndexOf(after), node);
            }
        }

        RefreshNodesValues();
        Nodes.ForEach(p => p.Update());

        return node;
    }

    private void RefreshNodesValues()
    {
        if (Nodes.Count < 1) return;
        _endNode = Nodes[Nodes.Count - 1];
        _startNode = Nodes[0];
    }

    public float GetNodeT(float fullT, out int index)
    {
        float fullDistance = SplineLenght;

        float lenght = 0f;
        var node = _startNode;
        while (node.RightNeighbor != null)
        {
            float dist = node.DistanceToNext / fullDistance;
            if (fullT > lenght + dist)
            {
                lenght += dist;
                node = node.RightNeighbor;
            }
            else
            {
                index = node.Number;
                return (fullT - lenght) / dist;
            }
        }

        index = Nodes.Count - 1;
        return 1f;
    }

    /// <summary>
    /// Invalidates the mesh. The mesh will be updated during the next Update call.
    /// </summary>
    public void InvalidateMesh()
    {
        _needRebuildMesh = true;
    }

    public void RebuildAllGroups()
    {
        foreach (var group in LoftingGroups)
        {
            if (group.IsValid)
            {
                RebuildMesh(group);
            }
        }
    }

    /// <summary>
    /// Recalculates the values.
    /// </summary>
    public void RecalculateValues()
    {
        Nodes.ForEach(p => p.RecalculateNeighborsValues());
        RecalculateSplineLength();
    }

    private void Awake()
    {
        Initialize();
        _needRebuildMesh = true;
        Clear();
        Fix();

        if (Application.isPlaying)
        {
            TryToRebuildMesh();
        }
        else
        {
            var formers = FindObjectsOfType<SplineFormer>();
            foreach (SplineFormer splineFormer in formers)
            {
                splineFormer.InvalidateMesh();
                splineFormer.TryToRebuildMesh();
            }
        }
    }

    /// <summary>
    /// Fix any incidents and does some preparings
    /// </summary>
    private void Fix()
    {
        RefreshNodesValues();

        if (LoftDirection.sqrMagnitude < 0.1f) LoftDirection = Vector3.forward;
        SegmentScale = Vector3.Max(SegmentScale, 0.001f * Vector3.one);

        if (LoftingGroups == null)
        {
            LoftingGroups = new List<LoftingGroup>();
            LoftingGroups.Add(new LoftingGroup());
        }

        int removed = Nodes.RemoveAll(p => p == null || p.transform == null || p.SplineFormer != this);
        if (removed > 0)
        {
            Debug.Log(String.Format("Removed: {0} nodes", removed));
            InvalidateMesh();
        }

        if (Nodes.Count < 1)
        {
            AddNodeImmediately();
            InvalidateMesh();
        }

        if (Nodes.Count < 2)
        {
            var node = AddNodeImmediately();
            node.transform.position += Vector3.forward * 5;
            node.RefreshNode();
            InvalidateMesh();
        }

        if (Loop && Nodes.Count < 3)
        {
            var node = AddNodeImmediately();
            node.transform.position += Vector3.left * 5;
            node.RefreshNode();
            InvalidateMesh();
        }

        if (Loop && Nodes.Count < 4)
        {
            var node = AddNodeImmediately();
            node.transform.position += Vector3.right * 5;
            node.RefreshNode();
            InvalidateMesh();
        }

        if (Loop)
        {
            _endNode.transform.position = _startNode.transform.position;
        }

        foreach (var group in LoftingGroups)
        {
            if (group.IsValid)
            {
                if (group.ResultMesh == null)
                {
                    if (group.MeshCollider != null && group.MeshCollider.sharedMesh != null)
                    {
                        group.ResultMesh = group.MeshCollider.sharedMesh;
                    }
                    else if (group.MeshFilter != null && group.MeshFilter.sharedMesh != null)
                    {
                        group.ResultMesh = group.MeshFilter.sharedMesh;
                    }
                    else
                    {
                        group.ResultMesh = new Mesh();
                    }
                    group.ResultMesh.MarkDynamic();
                }

                group.ActiveSegmentsNumber = Mathf.Clamp(
                    SegmentsNumber,
                    1,
                    (int)(group.IntervalLength * SegmentsNumber));

                group.Fix();
            }
        }
    }

    /// <summary>
    /// Process full recalculations and rebuilds the mesh
    /// </summary>
    private void FullMeshRebuild()
    {
        RecalculateValues();
        RebuildAllGroups();
    }

    private void OnDrawGizmos()
    {
        if (VisualOptions.ShowNodeLinks)
        {
            Gizmos.color = Color.white;
            for (int i = 1; i < Nodes.Count; i++)
            {
                Gizmos.DrawLine(Nodes[i - 1].transform.position, Nodes[i].transform.position);
            }
        }

        if (!VisualOptions.ShowSegmentsPath) return;

        Gizmos.color = Color.red;
        var ltw = transform.localToWorldMatrix;
        var wtl = transform.worldToLocalMatrix;

        float segmentStep = 1.0f / SegmentsNumber;
        Vector3[] points = new Vector3[SegmentsNumber];
        for (int s = 0; s < SegmentsNumber; s++)
        {
            float resultT = segmentStep * s + segmentStep;

            int nodeIndex;
            float nodeT = GetNodeT(resultT, out nodeIndex);

            if (nodeIndex < 0)
            {
                nodeIndex = 0;
            }
            else if (nodeIndex > Nodes.Count - 1)
            {
                nodeIndex = Nodes.Count - 1;
            }

            int nextIndex = nodeIndex + 1;

            if (nodeIndex >= Nodes.Count - 1)
            {
                if (Loop)
                {
                    nextIndex = 0;
                }
                else
                {
                    nodeIndex = Nodes.Count - 2;
                    nextIndex = Nodes.Count - 1;
                }
            }

            SplineNode fromNode = Nodes[nodeIndex];
            SplineNode toNode = Nodes[nextIndex];

            Vector3 startRPos = fromNode.LocalPosition;
            Vector3 endRPos = toNode.LocalPosition;

            Vector3 startR_RightP = fromNode.RightP;
            Vector3 endRPos_LeftP = toNode.LeftP;

            points[s] =
                startRPos
                + SplinePowerVector3Extensions.Bezier(
                startRPos - startRPos,
                startR_RightP - startRPos,
                endRPos_LeftP - startRPos,
                endRPos - startRPos, nodeT) + (Vector3)(wtl * transform.position);

            if (s > 0)
            {
                Gizmos.DrawLine(ltw * points[s - 1], ltw * points[s]);
            }
        }
    }

    private void OnEnable()
    {
        Initialize();
        InvalidateMesh();
    }

    private void Initialize()
    {
        if (LoftingGroups == null)
        {
            LoftingGroups = new List<LoftingGroup>();
            LoftingGroups.Add(new LoftingGroup());
        }

        if (ExportOptions == null) ExportOptions = new ExportOptionsContainer();
        if (VisualOptions == null) VisualOptions = new VisualOptionsContainer();
    }

    /// <summary>
    /// Rebuilds the mesh for the specified lofting group
    /// </summary>
    /// <param name="group">lofting group</param>
    private void RebuildMesh(LoftingGroup group)
    {
        if (!group.IsValid)
        {
            throw new ArgumentException("SegmentMesh and MeshFilter can't be null");
        }

        LoftDirection.Normalize();
        var result = group.ResultMesh;

        float segmentStep = group.IntervalLength / group.ActiveSegmentsNumber;

        for (int s = 0; s < group.ActiveSegmentsNumber; s++)
        {
            ManagedMesh segment = group.ResultSegments[s];
            segment.Clear();

            ManagedMesh source = group.mSegmentMesh;

            if (s == 0 && group.mStartPiece != null)
            {
                source = group.mStartPiece;
            }

            if (s == group.ActiveSegmentsNumber - 1 && group.mEndPiece != null)
            {
                source = group.mEndPiece;
            }

            Vector3 pivotShift = Vector3.Project(source.bounds.center, LoftDirection);
            Vector3 startSPos = pivotShift - source.bounds.extents.Mul(LoftDirection);
            Vector3 endSPos = pivotShift + source.bounds.extents.Mul(LoftDirection);
            Vector3 sourceDir = (endSPos - startSPos);
            float sourceLenght = sourceDir.magnitude;

            bool flipSegment = RollerCoasterFix && _flipNext;

            Vector3 dirS = LoftDirection;
            Vector3[] sourceVertices = source.vertices;
            Vector3[] resultVertices = new Vector3[source.vertexCount];

            Vector3[] sourceNormals = source.normals;
            Vector3[] resultNormals = new Vector3[source.vertexCount];

            Vector4[] sourceTangents = source.tangents;
            Vector4[] resultTangents = new Vector4[source.vertexCount];

            float segScaler = ((((float)(group.ActiveSegmentsNumber - s) + 1f) / (float)group.ActiveSegmentsNumber) * 2f);
            //Debug.Log("SegScaler: " + segScaler);

            for (int i = 0; i < source.vertexCount; i++)
            {
                Vector3 curSPos = sourceVertices[i] - startSPos;
                Vector3 projected = Vector3.Project(curSPos, dirS);
                Vector3 perp = curSPos - projected;
//                perp.Scale(SegmentScale);
                perp.Scale(SegmentScale * segScaler);
                float sourceT = Mathf.InverseLerp(0f, sourceLenght, projected.magnitude);
                float resultT = group.StartPosition
                    + segmentStep * s + segmentStep * sourceT;

                int nodeIndex;
                float nodeT = GetNodeT(resultT, out nodeIndex);

                if (nodeIndex < 0)
                {
                    nodeIndex = 0;
                }
                else if (nodeIndex > Nodes.Count - 1)
                {
                    nodeIndex = Nodes.Count - 1;
                }

                int nextIndex = nodeIndex + 1;

                if (nodeIndex >= Nodes.Count - 1)
                {
                    if (Loop)
                    {
                        nextIndex = 0;
                    }
                    else
                    {
                        nodeIndex = Nodes.Count - 2;
                        nextIndex = Nodes.Count - 1;
                    }
                }

                SplineNode fromNode = Nodes[nodeIndex];
                SplineNode toNode = Nodes[nextIndex];

                Vector3 startRPos = fromNode.LocalPosition;
                Vector3 endRPos = toNode.LocalPosition;

                Vector3 nodesDir = (endRPos - startRPos);

                Vector3 upwardVector = Vector3.up;
                float additionalAngle = 0f;

                if (RollerCoasterFix && s > 0 && nodeIndex > 0)
                {
                    SplineNode prevNode = Nodes[nodeIndex - 1];
                    Vector3 prevNodesDir = (startRPos - prevNode.LocalPosition);
                    Vector3 prevNodesDirXZ = prevNodesDir;
                    prevNodesDirXZ.y = 0;

                    Vector3 nodesDirXZ = nodesDir;
                    nodesDirXZ.y = 0;

                    float dot = Vector3.Dot(prevNodesDirXZ, nodesDirXZ);

                    if (Vector3.Dot(Vector3.up, nodesDir) > 0.5f)
                    {
                        upwardVector = Vector3.left;
                        additionalAngle = -90f * Mathf.Abs(Vector3.Dot(LoftDirection, Vector3.left));
                        _flipNext = true;
                    }
                    else if (Vector3.Dot(Vector3.up, nodesDir) < -0.5f)
                    {
                        upwardVector = Vector3.right;
                        additionalAngle = 90f * Mathf.Abs(Vector3.Dot(LoftDirection, Vector3.left));
                        _flipNext = false;
                    }
                    else
                    {
                        if (flipSegment)
                        {
                            additionalAngle = 180f;
                        }
                    }
                }

                Vector3 startR_RightP = fromNode.RightP;

                Vector3 endRPos_RightP = toNode.RightP;
                Vector3 endRPos_LeftP = toNode.LeftP;

                float loftAngle = LoftAngle +
                                  Mathf.Lerp(fromNode.AdditionalLoftAngle, toNode.AdditionalLoftAngle, nodeT);

                Vector3 dirR = nodesDir.normalized;

                if (Loop || (nodeIndex > 0 && nodeIndex + 2 < Nodes.Count))
                {
                    dirR = (startR_RightP - startRPos).normalized;
                    Vector3 dirN = (endRPos_RightP - endRPos).normalized;
                    float t = QuadraticSmooth ? (nodeT * nodeT) : nodeT;
                    dirR = Vector3.Slerp(dirR, dirN, t);
                }

                Quaternion directionRot =
                    Quaternion.LookRotation(dirR, upwardVector) * Quaternion.Inverse(Quaternion.LookRotation(dirS, upwardVector));

                Quaternion loftRot =
                    Quaternion.AngleAxis(additionalAngle + loftAngle, dirR);

                resultVertices[i] =
                    startRPos
                    + loftRot * (directionRot * perp)
                    + SplinePowerVector3Extensions.Bezier(
                    startRPos - startRPos,
                    startR_RightP - startRPos,
                    endRPos_LeftP - startRPos,
                    endRPos - startRPos, nodeT);

                if (group.ProcessOriginNormals)
                {
                    resultNormals[i] = loftRot * directionRot * sourceNormals[i];
                }

                if (group.ProcessOriginTangents)
                {
                    resultTangents[i] = loftRot * directionRot * sourceTangents[i];
                }
            }

            segment.vertices = resultVertices;
            segment.triangles = source.triangles;
            segment.uv = source.uv;
            segment.normals = resultNormals;
            segment.tangents = resultTangents;
        }

        for (int s = 0; s < group.ResultSegments.Length; s++)
        {
            if (group.RecalculateNormals)
            {
                group.ResultSegments[s].RecalculateNormals();
            }

            if (s > 0)
                if (group.SmoothNormals || group.Weld)
                {
                    WeldAndSmooth(group, group.ResultSegments[s - 1], group.ResultSegments[s]);
                }

            if (Loop && s == 0)
            {
                if (group.SmoothNormals || group.Weld)
                {
                    WeldAndSmooth(group, group.ResultSegments[group.ResultSegments.Length - 1], group.ResultSegments[s]);
                }
            }
        }

        CombineSegments(result, group.ResultSegments);

#if UNITY_EDITOR
        UnityEditor.MeshUtility.SetMeshCompression(result, UnityEditor.ModelImporterMeshCompression.High);
        result.name = ExportOptions.GetName(this, group);
#endif
        ;
        result.hideFlags = HideFlags.DontSave;

        group.ResultMesh = result;

        if (group.MeshFilter != null)
        {
            group.MeshFilter.sharedMesh = null;
            group.MeshFilter.sharedMesh = result;
        }

        if (group.MeshCollider != null)
        {
            group.MeshCollider.sharedMesh = null;
            group.MeshCollider.sharedMesh = result;
        }
    }

    private void CombineSegments(Mesh mesh, ManagedMesh[] resultSegments)
    {
        ManagedMesh result = ManagedMesh.Combine(resultSegments);
        mesh.Clear();

        mesh.vertices = result.vertices;
        mesh.triangles = result.triangles;
        mesh.uv = result.uv;
        mesh.normals = result.normals;
        mesh.tangents = result.tangents;

        mesh.RecalculateBounds();
    }

    /// <summary>
    /// Recalculates the spline lenght.
    /// </summary>
    /// <returns>The spline lenght.</returns>
    private float RecalculateSplineLength()
    {
        if (Nodes.Count < 2)
        {
            return 0f;
        }
        else
        {
            float length = 0f;
            var node = _startNode;
            while (node.RightNeighbor != null)
            {
                length += node.DistanceToNext;
                node = node.RightNeighbor;
            }

            _splineLength = length;
            return _splineLength;
        }
    }

    private void Update()
    {
        if (_needAddNode)
        {
            _needAddNode = false;
            AddNodeImmediately();
        }

        TryToRebuildMesh();
    }

    private void TryToRebuildMesh()
    {
        if (_needRebuildMesh)
        {
            _needRebuildMesh = false;
            Fix();
            FullMeshRebuild();
        }
    }

    private void WeldAndSmooth(LoftingGroup group, ManagedMesh meshA, ManagedMesh meshB)
    {
        float weldDistanceSqr = group.WeldingDistance * group.WeldingDistance;

        var verticesA = meshA.vertices;
        var verticesB = meshB.vertices;

        var normalsA = meshA.normals;
        var normalsB = meshB.normals;

        int i, j, an, bn;
        an = verticesA.Length;
        bn = verticesB.Length;

        for (i = 0; i < an; i++)
            for (j = 0; j < bn; j++)
            {
                // if any one inside limit distance...
                if (Vector3.SqrMagnitude(verticesA[i] - verticesB[j]) < weldDistanceSqr)
                {
                    if (group.Weld)
                    {
                        verticesB[j] = verticesA[i];
                    }

                    if (group.SmoothNormals && Vector3.Dot(normalsA[i], normalsB[j]) > 0)
                    {
                        normalsA[i] = Vector3.Slerp(normalsA[i], normalsB[j], 0.5f);
                        normalsB[j] = normalsA[i];
                    }
                }
            }

        meshA.vertices = verticesA;
        meshB.vertices = verticesB;

        meshA.normals = normalsA;
        meshB.normals = normalsB;
    }

    public void Clear()
    {
        foreach (var group in LoftingGroups)
        {
            group.ResultMesh = null;

            if (group.MeshFilter != null)
            {
                if (group.MeshFilter.sharedMesh != null)
                {
                    group.MeshFilter.sharedMesh.Clear();
                }
                group.MeshFilter.sharedMesh = null;
            }

            if (group.MeshCollider != null)
            {
                if (group.MeshCollider.sharedMesh != null)
                {
                    group.MeshCollider.sharedMesh.Clear();
                }
                group.MeshCollider.sharedMesh = null;
            }
        }

        InvalidateMesh();
    }

    private static Mesh _dummyBox;
    private bool _flipNext;

    public static Mesh DummyBox
    {
        get
        {
            if (_dummyBox == null)
            {
                _dummyBox = new Mesh();
                var mesh = _dummyBox;
                mesh.Clear();

                float length = 1f;
                float width = 1f;
                float height = 1f;

                #region Vertices

                Vector3 p0 = new Vector3(-length * .5f, -width * .5f, height * .5f);
                Vector3 p1 = new Vector3(length * .5f, -width * .5f, height * .5f);
                Vector3 p2 = new Vector3(length * .5f, -width * .5f, -height * .5f);
                Vector3 p3 = new Vector3(-length * .5f, -width * .5f, -height * .5f);

                Vector3 p4 = new Vector3(-length * .5f, width * .5f, height * .5f);
                Vector3 p5 = new Vector3(length * .5f, width * .5f, height * .5f);
                Vector3 p6 = new Vector3(length * .5f, width * .5f, -height * .5f);
                Vector3 p7 = new Vector3(-length * .5f, width * .5f, -height * .5f);

                Vector3[] vertices = new Vector3[]
                {
	                // Bottom
	                p0, p1, p2, p3,

	                // Left
	                p7, p4, p0, p3,

	                // Front
	                p4, p5, p1, p0,

	                // Back
	                p6, p7, p3, p2,

	                // Right
	                p5, p6, p2, p1,

	                // Top
	                p7, p6, p5, p4
                };

                #endregion Vertices

                #region Normales

                Vector3 up = Vector3.up;
                Vector3 down = Vector3.down;
                Vector3 front = Vector3.forward;
                Vector3 back = Vector3.back;
                Vector3 left = Vector3.left;
                Vector3 right = Vector3.right;

                Vector3[] normales = new Vector3[]
                {
	                // Bottom
	                down, down, down, down,

	                // Left
	                left, left, left, left,

	                // Front
	                front, front, front, front,

	                // Back
	                back, back, back, back,

	                // Right
	                right, right, right, right,

	                // Top
	                up, up, up, up
                };

                #endregion Normales

                #region UVs

                Vector2 _00 = new Vector2(0f, 0f);
                Vector2 _10 = new Vector2(1f, 0f);
                Vector2 _01 = new Vector2(0f, 1f);
                Vector2 _11 = new Vector2(1f, 1f);

                Vector2[] uvs = new Vector2[]
                {
	                // Bottom
	                _11, _01, _00, _10,

	                // Left
	                _11, _01, _00, _10,

	                // Front
	                _11, _01, _00, _10,

	                // Back
	                _11, _01, _00, _10,

	                // Right
	                _11, _01, _00, _10,

	                // Top
	                _11, _01, _00, _10,
                };

                #endregion UVs

                #region Triangles

                int[] triangles = new int[]
                {
	                // Bottom
	                3, 1, 0,
	                3, 2, 1,

	                // Left
	                3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
	                3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,

	                // Front
	                3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
	                3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,

	                // Back
	                3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
	                3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,

	                // Right
	                3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
	                3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,

	                // Top
	                3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
	                3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,
                };

                #endregion Triangles

                mesh.vertices = vertices;
                mesh.normals = normales;
                mesh.uv = uvs;
                mesh.triangles = triangles;

                mesh.RecalculateBounds();
                ;
            }

            return _dummyBox;
        }
    }

    [Serializable]
    public class LoftingGroup
    {

        public MeshCollider MeshCollider;
        public MeshFilter MeshFilter;
        public bool ProcessOriginNormals = true;
        public bool ProcessOriginTangents = true;
        public bool RecalculateNormals = false;

        [NonSerialized]
        public Mesh ResultMesh;

        public Mesh SegmentMesh;
        public Mesh StartPiece;
        public Mesh EndPiece;

        public bool SmoothNormals = false;
        public bool Weld = true;
        public float WeldingDistance = 0.01f;

        public float StartPosition = 0f;
        public float EndPosition = 1f;
        public int ActiveSegmentsNumber;

        [NonSerialized]
        public ManagedMesh mSegmentMesh;
        public ManagedMesh mStartPiece;
        public ManagedMesh mEndPiece;

        [NonSerialized]
        public ManagedMesh[] ResultSegments;

        public float IntervalLength { get { return EndPosition - StartPosition; } }

        public bool IsValid
        {
            get
            {
                return SegmentMesh != null
                    && (MeshCollider != null || MeshFilter != null)
                    && CheckInterval();
            }
        }

        private bool CheckInterval()
        {
            StartPosition = Mathf.Clamp01(StartPosition);
            EndPosition = Mathf.Clamp01(EndPosition);
            return EndPosition > StartPosition;
        }

        public void Fix()
        {
            mSegmentMesh = (SegmentMesh != null) ? new ManagedMesh(SegmentMesh) : null;
            mStartPiece = (StartPiece != null) ? new ManagedMesh(StartPiece) : null;
            mEndPiece = (EndPiece != null) ? new ManagedMesh(EndPiece) : null;

            //Prevents mesh buffer overflow
            ActiveSegmentsNumber = Mathf.Clamp(
                ActiveSegmentsNumber,
                1,
                ushort.MaxValue / mSegmentMesh.vertexCount);

            if (ResultSegments == null || ResultSegments.Length != ActiveSegmentsNumber)
            {
                var newResultSegments = new ManagedMesh[ActiveSegmentsNumber];

                if (ResultSegments == null)
                {
                    ResultSegments = new ManagedMesh[0];
                }

                int minLenght = Math.Min(ResultSegments.Length, ActiveSegmentsNumber);

                for (int s = 0; s < minLenght; s++)
                {
                    newResultSegments[s] = ResultSegments[s];
                }

                for (int s = minLenght; s < ActiveSegmentsNumber; s++)
                {
                    newResultSegments[s] = new ManagedMesh();
                }

                for (int s = ActiveSegmentsNumber; s < ResultSegments.Length; s++)
                {
                    ResultSegments[s].Clear();
                }

                ResultSegments = newResultSegments;
            }
        }
    }

    [Serializable]
    public class ManagedMesh
    {
        public int vertexCount;
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uv;
        public Vector3[] normals;
        public Vector4[] tangents;
        public Bounds bounds;

        public ManagedMesh()
        {
            Clear();
        }

        public ManagedMesh(Mesh mesh)
        {
            if (mesh != null)
            {
                vertexCount = mesh.vertexCount;
                vertices = mesh.vertices;
                triangles = mesh.triangles;
                uv = mesh.uv;
                normals = mesh.normals;
                tangents = mesh.tangents;
                bounds = mesh.bounds;
            }
            else
            {
                Clear();
            }
        }

        public void Clear()
        {
            vertexCount = 0;
            vertices = null;
            triangles = null;
            uv = null;
            normals = null;
            tangents = null;
        }

        public void RecalculateNormals()
        {
            for (int i = 0; i < triangles.Length; i += 3)
            {
                var i1 = triangles[i + 0];
                var i2 = triangles[i + 1];
                var i3 = triangles[i + 2];

                var v1 = vertices[i1];
                var v2 = vertices[i2];
                var v3 = vertices[i3];

                Vector3 normal = Vector3.Cross(v3 - v2, v1 - v2);
                normal.Normalize();

                normals[i1] = normal;
                normals[i2] = normal;
                normals[i3] = normal;
            }
        }

        public static ManagedMesh Combine(ManagedMesh[] segments)
        {
            ManagedMesh result = new ManagedMesh();

            Vector3[] vertices = new Vector3[segments.Sum(p => p.vertices.Length)];
            int[] triangles = new int[segments.Sum(p => p.triangles.Length)];
            Vector2[] uv = new Vector2[segments.Sum(p => p.uv.Length)];
            Vector3[] normals = new Vector3[segments.Sum(p => p.normals.Length)];
            Vector4[] tangents = new Vector4[segments.Sum(p => p.tangents.Length)];

            int verticesShift = 0;
            int trianglesShift = 0;
            int uvShift = 0;
            int normalsShift = 0;
            int tangentsShift = 0;

            for (int k = 0; k < segments.Length; k++)
            {
                if (k > 0)
                {
                    var previousSegment = segments[k - 1];
                    verticesShift += previousSegment.vertices.Length;
                    trianglesShift += previousSegment.triangles.Length;
                    uvShift += previousSegment.uv.Length;
                    normalsShift += previousSegment.normals.Length;
                    tangentsShift += previousSegment.tangents.Length;
                }

                var segment = segments[k];

                for (int i = 0; i < segment.vertices.Length; i++)
                {
                    vertices[verticesShift + i] = segment.vertices[i];
                }

                for (int i = 0; i < segment.triangles.Length; i++)
                {
                    triangles[trianglesShift + i] = verticesShift + segment.triangles[i];
                }

                for (int i = 0; i < segment.uv.Length; i++)
                {
                    uv[uvShift + i] = segment.uv[i];
                }

                for (int i = 0; i < segment.normals.Length; i++)
                {
                    normals[normalsShift + i] = segment.normals[i];
                }

                for (int i = 0; i < segment.tangents.Length; i++)
                {
                    tangents[tangentsShift + i] = segment.tangents[i];
                }
            }

            result.vertices = vertices;
            result.triangles = triangles;
            result.uv = uv;
            result.normals = normals;
            result.tangents = tangents;

            return result;
        }
    }

    [Serializable]
    public class VisualOptionsContainer
    {
        public float NodeSize = 0.5f;
        public bool ShowTangentNodes = true;
        public bool ShowTangents = false;
        public bool ShowNodeLinks = true;
        public bool ShowSegmentsPath = false;
    }

    [Serializable]
    public class ExportOptionsContainer
    {
        public string DefaultFolder = "Generated/Splines";
        public bool ShowSaveAsDialog = true;
        public bool ShowExportResultDialog = true;
        public bool ExtendedNaming = false;
        public bool AddObjectName = true;
        public bool AddLoftingGroupIndex = true;
        public string CustomName = "result";
        public bool AddDateTime = false;
        public bool AddAutoIncrementNumber = false;

        [SerializeField]
        private int _uk;

        public string GetName(SplineFormer splineFormer, LoftingGroup group)
        {
            if (!ExtendedNaming)
            {
                return String.Format("{0} Lg#{1} result", splineFormer.gameObject.name, splineFormer.LoftingGroups.IndexOf(group));
            }

            List<string> parts = new List<string>(10);
            if (AddObjectName)
                parts.Add(splineFormer.gameObject.name);
            if (AddLoftingGroupIndex)
                parts.Add(String.Format("Lg#{0}", splineFormer.LoftingGroups.IndexOf(group)));
            if (!String.IsNullOrEmpty(CustomName))
                parts.Add(CustomName);
            if (AddDateTime)
                parts.Add(DateTime.Now.ToString());
            if (AddAutoIncrementNumber)
            {
                _uk++;
                parts.Add(_uk.ToString());
            }

            string name = String.Join(" ", parts.ToArray());
            name = CleanFileName(name);
            return name;
        }

        private static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), "_"));
        }
    }

    public void Remap(SplineNode node, SplineFormer toFormer)
    {
        RemoveNodeImmediately(node);
        toFormer.Nodes.Add(node);
        node.SplineFormer = toFormer;
        InvalidateMesh();
        toFormer.InvalidateMesh();
    }

    public void RemoveNodeImmediately(SplineNode node)
    {
        Nodes.Remove(node);
        RefreshNodesValues();
        InvalidateMesh();
    }
}