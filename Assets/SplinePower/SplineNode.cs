using UnityEngine;

[ExecuteInEditMode]
public class SplineNode : MonoBehaviour
{
    public enum SplineTangentGuidesMode { Auto, Manual }
    /// <summary>
    /// Spline Former Host
    /// </summary>
    [HideInInspector]
    public SplineFormer SplineFormer;

    public float AdditionalLoftAngle;
    public SplineTangentGuidesMode TangentGuidesMode;

    public float LeftPCoefficient = 1f;
    public float RightPCoefficient = 1f;

    private float _distanceFromStart;
    private float _distanceToNext;
    private Vector3 _lastPostion;

    private Vector3 _leftP;
    private Vector3 _localPosition;
    private int _number;
    private Vector3 _rightP;

    private bool _fresh = true;

    /// <summary>
    /// Summary lenght of the path from the start node to this node
    /// </summary>
    /// <value>The distance from start.</value>
    public float DistanceFromStart
    {
        get
        {
            return _distanceFromStart;
        }
    }

    /// <summary>
    /// The lenght of the path from this node to the next node
    /// </summary>
    /// <value>The distance to next.</value>
    public float DistanceToNext
    {
        get { return _distanceToNext; }
    }

    /// <summary>
    /// Gets the left neighbor.
    /// </summary>
    /// <value>The left neighbor.</value>
    public SplineNode LeftNeighbor
    {
        get
        {
            if (this == SplineFormer.StartNode)
            {
                return null;
            }

            PrepareIfFresh();

            if (_number - 1 < 0 || _number - 1 >= SplineFormer.Nodes.Count)
            {
                Debug.Log("Something is terribly wrong! Try to delete this node.");
            }

            return SplineFormer.Nodes[_number - 1];
        }
    }

    private void PrepareIfFresh()
    {
        if (_fresh)
        {
            _fresh = false;
            RecalculateBasicValues();
        }
    }

    /// <summary>
    /// Gets the left guide.
    /// </summary>
    /// <value>The left guide.</value>
    public Vector3 LeftP
    {
        get
        {
            return _leftP;
        }
    }

    /// <summary>
    /// Gets the local position.
    /// </summary>
    /// <value>The local position.</value>
    public Vector3 LocalPosition
    {
        get
        {
            return _localPosition;
        }
    }

    /// <summary>
    /// Current number of the node. Can serve as index.
    /// </summary>
    /// <value>The number.</value>
    public int Number
    {
        get { return _number; }
    }

    /// <summary>
    /// Gets the right neighbor.
    /// </summary>
    /// <value>The right neighbor.</value>
    public SplineNode RightNeighbor
    {
        get
        {
            if (this == SplineFormer.EndNode)
            {
                return null;
            }

            PrepareIfFresh();

            int nextIndex = _number + 1;
            if (nextIndex >= SplineFormer.Nodes.Count || nextIndex < 0)
            {
                Debug.Log("Something is terribly wrong! Try to delete this node.");
            }

            nextIndex = Mathf.Clamp(nextIndex, 0, SplineFormer.Nodes.Count - 1);
            return SplineFormer.Nodes[nextIndex];
        }
    }

    /// <summary>
    /// Gets the right guide.
    /// </summary>
    /// <value>The right guide.</value>
    public Vector3 RightP
    {
        get
        {
            return _rightP;
        }
    }

    /// <summary>
    /// Recalculates the local position.
    /// </summary>
    public void RecalculateLocalPosition()
    {
        _localPosition = SplineFormer.transform.InverseTransformPoint(transform.position);
    }

    /// <summary>
    /// Split this node on two.
    /// </summary>
    public void Split()
    {
        var newNode = SplineFormer.AddNodeImmediately(this);
        newNode.transform.position = this.transform.position;
        newNode.transform.position += (LeftP - LocalPosition).normalized * 2f * SplineFormer.VisualOptions.NodeSize;
        this.transform.position += (RightP - LocalPosition).normalized * 2f * SplineFormer.VisualOptions.NodeSize;
    }

    /// <summary>
    /// Recalculates values that depends on neighbors' state.
    /// </summary>
    public void RecalculateNeighborsValues()
    {
        Clean();
        if (SplineFormer == null) return;

        RecalculateBasicValues();

        if (RightNeighbor != null)
        {
            RecalculateRightP();
            RightNeighbor.RecalculateLeftP();

            //_distanceToNext = Vector3.Distance(LocalPosition, RightNeighbor.LocalPosition);
            _distanceToNext =
                Vector3.Distance(_localPosition, _rightP) +
                Vector3.Distance(_rightP, RightNeighbor._leftP) +
                Vector3.Distance(RightNeighbor._leftP, RightNeighbor._localPosition);
        }
        else
        {
            _distanceToNext = 0;
        }

        RecalculateDistanceFromStart();
        RecalculateRightP();
        RecalculateLeftP();
    }


    /// <summary>
    /// Refreshs the node.
    /// </summary>
    public void RefreshNode()
    {
        Clean();
        if (SplineFormer == null)
            return;

        RecalculateBasicValues();
        RecalculateLocalPosition();

        if (_lastPostion != LocalPosition)
        {
            SplineFormer.InvalidateMesh();
        }

        _lastPostion = LocalPosition;
    }

    // Update is called once per frame
    public void Update()
    {
        if (!Application.isPlaying)
        {
            RefreshNode();
        }
    }

    /// <summary>
    /// Destroys this node if it's no longer conected to the host SplineFormer
    /// </summary>
    private void Clean()
    {
        if (SplineFormer == null
            || SplineFormer.transform == null
            || !SplineFormer.Nodes.Contains(this)
            )
        {
            SplineFormer = null;
            GameObject.DestroyImmediate(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        if (SplineFormer == null ||
            (SplineFormer.Loop && this == SplineFormer.EndNode))
        {
            return;
        }

        var nodePosition = transform.position;
        var rightNodePosition = SplineFormer.transform.TransformPoint(RightP);
        var leftNodePosition = SplineFormer.transform.TransformPoint(LeftP);

        Gizmos.color = Color.Lerp(Color.red, Color.blue, Number / (float)(SplineFormer.Nodes.Count - 1));
        Gizmos.DrawSphere(nodePosition, SplineFormer.VisualOptions.NodeSize);

        if (SplineFormer.VisualOptions.ShowTangents)
        {
            Gizmos.DrawLine(nodePosition, rightNodePosition);
            Gizmos.DrawLine(nodePosition, leftNodePosition);
        }

        if (SplineFormer.VisualOptions.ShowTangentNodes)
        {
            if (TangentGuidesMode == SplineTangentGuidesMode.Auto)
            {
                Gizmos.color = new Color(1, 1, 0, 1f);
            }
            else
            {
                Gizmos.color = new Color(1, 0, 0, 1f);
            }

            Gizmos.DrawSphere(rightNodePosition, SplineFormer.VisualOptions.NodeSize * 0.5f);
            Gizmos.DrawSphere(leftNodePosition, SplineFormer.VisualOptions.NodeSize * 0.5f);
        }
    }

    /// <summary>
    /// Recalculates the basic values.
    /// </summary>
    private void RecalculateBasicValues()
    {
        _number = SplineFormer.Nodes.IndexOf(this);
        transform.SetSiblingIndex(_number);
        name = "node" + Number;
        RecalculateLocalPosition();
    }

    /// <summary>
    /// Recalculates DistanceFromStart.
    /// </summary>
    /// <returns>The distance from start.</returns>
    private float RecalculateDistanceFromStart()
    {
        float lenght = 0f;
        var node = SplineFormer.StartNode;
        while (node != this)
        {
            lenght += node.DistanceToNext;
            node = node.RightNeighbor;
        }

        _distanceFromStart = lenght;
        return _distanceFromStart;
    }

    /// <summary>
    /// Recalculates the left guide.
    /// </summary>
    private void RecalculateLeftP()
    {
        if (!SplineFormer.Loop)
        {
            if (LeftNeighbor == null)
            {
                _leftP = LocalPosition;
                return;
            }
            else if (RightNeighbor == null)
            {
                _leftP = LocalPosition;
                return;
            }
        }

        Vector3 rightLocalPosition;
        Vector3 leftLocalPosition;
        Vector3 centerLocalPosition;

        centerLocalPosition = LocalPosition;
        if (LeftNeighbor == null)
        {
            leftLocalPosition = SplineFormer.EndNode.LeftNeighbor.LocalPosition;
        }
        else
        {
            leftLocalPosition = LeftNeighbor.LocalPosition;
        }

        if (RightNeighbor == null)
        {
            rightLocalPosition = SplineFormer.StartNode.RightNeighbor.LocalPosition;
        }
        else
        {
            rightLocalPosition = RightNeighbor.LocalPosition;
        }

        var hand = -(rightLocalPosition - leftLocalPosition);
        hand = hand * (leftLocalPosition - centerLocalPosition).magnitude / hand.magnitude;

        float coefficient = SplineFormer.Coefficient;
        if (TangentGuidesMode == SplineTangentGuidesMode.Manual)
        {
            coefficient = LeftPCoefficient;
        }
        _leftP = centerLocalPosition + hand * coefficient;
    }

    /// <summary>
    /// Recalculates the right guide.
    /// </summary>
    private void RecalculateRightP()
    {
        if (!SplineFormer.Loop)
        {
            if (LeftNeighbor == null)
            {
                _rightP = LocalPosition;
                return;
            }
            else if (RightNeighbor == null)
            {
                _rightP = LocalPosition;
                return;
            }
        }

        Vector3 rightLocalPosition;
        Vector3 leftLocalPosition;
        Vector3 centerLocalPosition;

        centerLocalPosition = LocalPosition;
        if (LeftNeighbor == null)//if (SplineFormer.Loop)
        {
            leftLocalPosition = SplineFormer.EndNode.LeftNeighbor.LocalPosition;
        }
        else
        {
            leftLocalPosition = LeftNeighbor.LocalPosition;
        }

        if (RightNeighbor == null)//if (SplineFormer.Loop)
        {
            rightLocalPosition = SplineFormer.StartNode.RightNeighbor.LocalPosition;
        }
        else
        {
            rightLocalPosition = RightNeighbor.LocalPosition;
        }

        var hand = (rightLocalPosition - leftLocalPosition);
        hand = hand * (rightLocalPosition - centerLocalPosition).magnitude / hand.magnitude;

        float coefficient = SplineFormer.Coefficient;
        if (TangentGuidesMode == SplineTangentGuidesMode.Manual)
        {
            coefficient = RightPCoefficient;
        }
        _rightP = centerLocalPosition + hand * coefficient;

    }

    // Use this for initialization
    private void Awake()
    {
        if (SplineFormer != null)
            RecalculateBasicValues();
    }

    private void Start()
    {
        RecalculateBasicValues();
        RefreshNode();

        var host = GetComponentInParent<SplineFormer>();
        if (host != null && SplineFormer != null && SplineFormer != host)
        {
            SplineFormer.Remap(this, host);
        }
    }

    public void OnDestroy()
    {
        if (SplineFormer != null)
        {
            SplineFormer.RemoveNodeImmediately(this);
        }
    }


}