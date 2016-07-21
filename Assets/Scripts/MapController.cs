using UnityEngine;
using System.Collections;
using PathologicalGames;
using System.Collections.Generic;

public class MapController : MonoBehaviour {

    public Transform StartEndNodePrefab;
    public Transform NodePrefab;
    public Transform RoadPrefab;

    public int HorizontalNodeNumber;
    public int VerticalNodeNumber;
    public float NodeMinDistance;
    public float RoadMinDistance;

    List<Node> _nodeList = new List<Node>();
    Vector2 _startNodeIndex;
    Vector2 _endNodeIndex;
    int _upStep;
    int _downStep;
    int _leftStep;
    int _rightStep;
    int _totalStep;

    // Use this for initialization
    void Start() {
        GenerateMap();
    }

    // Update is called once per frame
    void Update() {

    }

    void GenerateMap() {
        SetStartEndNode();
        SetStep();
        WalkStepCreateMap();
    }

    void SetStartEndNode() {
        do {
            if (Random.value > 0.5f) {
                _startNodeIndex = new Vector2((HorizontalNodeNumber - 1) / 2.0f, Random.Range(0, 2) * (VerticalNodeNumber - 1));
            } else {
                _startNodeIndex = new Vector2(Random.Range(0, 2) * (HorizontalNodeNumber - 1), (VerticalNodeNumber - 1) / 2.0f);
            }
            if (Random.value > 0.5f) {
                _endNodeIndex = new Vector2((HorizontalNodeNumber - 1) / 2.0f, Random.Range(0, 2) * (VerticalNodeNumber - 1));
            } else {
                _endNodeIndex = new Vector2(Random.Range(0, 2) * (HorizontalNodeNumber - 1), (VerticalNodeNumber - 1) / 2.0f);
            }
        } while (_startNodeIndex == _endNodeIndex);
        Transform startNode = PoolManager.Pools["TilePool"].Spawn(StartEndNodePrefab, _startNodeIndex * NodeMinDistance, Quaternion.identity);
        Transform endNode = PoolManager.Pools["TilePool"].Spawn(StartEndNodePrefab, _endNodeIndex * NodeMinDistance, Quaternion.identity);
        _nodeList.Add(startNode.GetComponent<Node>());
        _nodeList.Add(endNode.GetComponent<Node>());
    }

    void SetStep() {
        Vector2 stepDistance = _endNodeIndex - _startNodeIndex;
        int horizontalRandomStep = Random.Range(1, HorizontalNodeNumber);
        int verticalRandomStep = Random.Range(1, VerticalNodeNumber);
        if (stepDistance.x > 0) {
            _leftStep = horizontalRandomStep;
            _rightStep = (int)stepDistance.x + horizontalRandomStep;
        } else if (stepDistance.x < 0) {
            _rightStep = horizontalRandomStep;
            _leftStep = -(int)stepDistance.x + horizontalRandomStep;
        } else {
            _leftStep = _rightStep = horizontalRandomStep;
        }
        if (stepDistance.y > 0) {
            _downStep = verticalRandomStep;
            _upStep = (int)stepDistance.y + verticalRandomStep;
        } else if (stepDistance.y < 0) {
            _upStep = verticalRandomStep;
            _downStep = -(int)stepDistance.y + verticalRandomStep;
        } else {
            _upStep = _downStep = verticalRandomStep;
        }
        _totalStep = _upStep + _downStep + _leftStep + _rightStep;
    }

    void WalkStepCreateMap() {
        Vector2 currentNodePosition = _nodeList[0].transform.position;
        while (_totalStep > 0) {
            Vector2 walkDirection = PickDirection(currentNodePosition);
            bool horizontalWalk = walkDirection.x != 0 ? true : false;
            CheckCreateRoadAtPoint(currentNodePosition + walkDirection * RoadMinDistance, horizontalWalk);
            //Move To Current Node Position
            currentNodePosition += walkDirection * NodeMinDistance;
            CheckCreateNodeAtPoint(currentNodePosition, walkDirection);
            _totalStep -= 1;
        }
    }

    Vector2 PickDirection(Vector2 currentNodePosition) {
        List<string> pickableDirection = new List<string>() { "up", "down", "left", "right" };
        if (currentNodePosition.x / NodeMinDistance == 0) {
            pickableDirection.Remove("left");
        } else if (currentNodePosition.x / NodeMinDistance == HorizontalNodeNumber - 1) {
            pickableDirection.Remove("right");
        }
        if (currentNodePosition.y / NodeMinDistance == 0) {
            pickableDirection.Remove("down");
        } else if (currentNodePosition.y / NodeMinDistance == VerticalNodeNumber - 1) {
            pickableDirection.Remove("up");
        }
        if (_upStep == 0 && pickableDirection.Contains("up")) {
            pickableDirection.Remove("up");
        }
        if (_downStep == 0 && pickableDirection.Contains("down")) {
            pickableDirection.Remove("down");
        }
        if (_leftStep == 0 && pickableDirection.Contains("left")) {
            pickableDirection.Remove("left");
        }
        if (_rightStep == 0 && pickableDirection.Contains("right")) {
            pickableDirection.Remove("right");
        }

        string direction = pickableDirection[Random.Range(0, pickableDirection.Count)];
        Vector2 moveDirection = Vector2.zero;
        if (direction == "up") {
            _upStep -= 1;
            moveDirection = Vector2.up;
        } else if (direction == "down") {
            _downStep -= 1;
            moveDirection = Vector2.down;
        } else if (direction == "left") {
            _leftStep -= 1;
            moveDirection = Vector2.left;
        } else if (direction == "right") {
            _rightStep -= 1;
            moveDirection = Vector2.right;
        }
        Debug.Log(_totalStep);
        Debug.Log(_upStep);
        Debug.Log(_downStep);
        Debug.Log(_leftStep);
        Debug.Log(_rightStep);
        Debug.Log(moveDirection);
        return moveDirection;
    }

    void CheckCreateRoadAtPoint(Vector2 point, bool horizontal) {
        Collider2D col = Physics2D.OverlapPoint(point, LayerMask.GetMask(new string[] { "Tile" }));
        if (col == null) {
            if (horizontal) {
                PoolManager.Pools["TilePool"].Spawn(RoadPrefab, point, Quaternion.Euler(0, 0, 90));
            } else {
                PoolManager.Pools["TilePool"].Spawn(RoadPrefab, point, Quaternion.identity);
            }
        }
    }

    void CheckCreateNodeAtPoint(Vector2 point, Vector2 walkDirection) {
        Collider2D previousCol = Physics2D.OverlapPoint(point - walkDirection * NodeMinDistance, LayerMask.GetMask(new string[] { "Tile" }));
        Collider2D col = Physics2D.OverlapPoint(point, LayerMask.GetMask(new string[] { "Tile" }));
        if (col == null) {
            col = PoolManager.Pools["TilePool"].Spawn(NodePrefab, point, Quaternion.identity).GetComponent<Collider2D>();
            _nodeList.Add(col.GetComponent<Node>());
        }
        if (walkDirection.x > 0) {
            previousCol.GetComponent<Node>().RightNode = col.GetComponent<Node>();
            col.GetComponent<Node>().LeftNode = previousCol.GetComponent<Node>();
        } else if (walkDirection.x < 0) {
            previousCol.GetComponent<Node>().LeftNode = col.GetComponent<Node>();
            col.GetComponent<Node>().RightNode = previousCol.GetComponent<Node>();
        }
        if (walkDirection.y > 0) {
            previousCol.GetComponent<Node>().UpNode = col.GetComponent<Node>();
            col.GetComponent<Node>().DownNode = previousCol.GetComponent<Node>();
        } else if (walkDirection.y < 0) {
            previousCol.GetComponent<Node>().DownNode = col.GetComponent<Node>();
            col.GetComponent<Node>().UpNode = previousCol.GetComponent<Node>();
        }
    }
}
