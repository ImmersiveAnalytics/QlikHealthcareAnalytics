using UnityEngine;
using System.Collections;

public class KartScript : MonoBehaviour
{
    public enum ControlType { Player, Bot };

    public ControlType controlType;

    private float _maxSpeed = 8;
    private SplineFormer _splineFormer;
    private SplineNode _closestNode;
    private float _accelerateAxis;
    private float _turnAxis;

    private SplineNode _nextNode;
    private SplineNode _currentNode;

    // Use this for initialization
    void Start()
    {
        _splineFormer = GameObject.Find("Road").GetComponent<SplineFormer>();        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        _closestNode = GetClosest();

        if (controlType == KartScript.ControlType.Player)
        {
            _accelerateAxis = Input.GetAxis("Vertical");
            _turnAxis = Input.GetAxis("Horizontal");
        }
        else if (controlType == ControlType.Bot)
        {           
            if (_currentNode == null)
            {
                _currentNode = _closestNode;
            }
            if (_nextNode == null)
            {
                _nextNode = _currentNode.RightNeighbor;
                if (_nextNode == null) _nextNode = _splineFormer.StartNode;
            }

            if (Vector3.Distance(transform.position, _nextNode.transform.position) < 5)
            {
                _currentNode = _nextNode;
                _nextNode = _currentNode.RightNeighbor;
                if (_nextNode == null) _nextNode = _splineFormer.StartNode;
            }

            var direction = _nextNode.transform.position - _currentNode.transform.position;
            var angleBetween = Quaternion.FromToRotation(transform.forward, direction).eulerAngles.y;
            float turnLerp = 1f * Time.deltaTime;
            float accelerateLerp = 2f * Time.deltaTime;

            if (angleBetween > 180) angleBetween -= 360;

            if (angleBetween > 10)
            {
                _turnAxis = Mathf.Lerp(_turnAxis, 1, turnLerp);
            }
            else if (angleBetween < -10)
            {
                _turnAxis = Mathf.Lerp(_turnAxis, -1, turnLerp);
            }
            else
            {
                _turnAxis = Mathf.Lerp(_turnAxis, 0, turnLerp);
            }
                       
            if (Mathf.Abs(angleBetween) < 90)
            {
                _accelerateAxis = Mathf.Lerp(_accelerateAxis, 1, accelerateLerp);
            }
            else
            {
                _accelerateAxis = Mathf.Lerp(_accelerateAxis, 0, accelerateLerp);
            }

            _maxSpeed = 10;//dirty cheat

            if (Mathf.Abs(_accelerateAxis) > 0.7)//a little bit of drunk driving
            {
                _turnAxis = Mathf.Lerp(_turnAxis, Mathf.Cos(Time.timeSinceLevelLoad * 1.5f), Mathf.Abs(_accelerateAxis) - (GetComponent<Rigidbody>().velocity.magnitude * 2));
            }
        }

        if (GetComponent<Rigidbody>().velocity.magnitude < _maxSpeed)
        {
            float force = 500 * Time.deltaTime;
            GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * _accelerateAxis * force, ForceMode.Acceleration);
        }

        
        float angle = 180 * Time.deltaTime;
        transform.Rotate(Vector3.up, _turnAxis * angle);

        if (transform.position.y < -15)
        {
            transform.position = _closestNode.transform.position + Vector3.up * 5;
        }

        GetComponent<AudioSource>().pitch = GetComponent<Rigidbody>().velocity.magnitude / _maxSpeed * 8;
    }

    SplineNode GetClosest()
    {
        SplineNode resultNode = null;
        float resultDistance = float.MaxValue;
        foreach (var node in _splineFormer.Nodes.ToArray())
        {
            float currentDistance = Vector3.Distance(node.transform.position, transform.position);
            if (currentDistance < resultDistance)
            {
                resultDistance = currentDistance;
                resultNode = node;
            }
        }

        return resultNode;
    }
}
