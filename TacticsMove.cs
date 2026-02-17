using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsMove : MonoBehaviour 
{
    public bool turn = false;
    public bool moving = false;
    public bool canMove = false;
    public bool hasMoved = false;
    public bool canCancelMove = true;
    public Tile currentTile;
    public Tile actualTargetTile;
    public Tile prevTargetTile;
    public List<Tile> selectableTiles = new List<Tile>();

    protected Stack<Tile> path = new Stack<Tile>();
    protected Tile hoveredTile;

    private bool _boostActived = false; 
    private GameObject _boostObject;
    private GameObject[] _tiles;
    private Vector3 _velocity = new Vector3();
    private Vector3 _heading = new Vector3();
    private float _halfHeight = 0;
    private bool _fallingDown = false;
    private bool _jumpingUp = false;
    private bool _movingEdge = false;
    private Vector3 _jumpTarget;
    private Vector3 _lastPosition;
    private Quaternion _lastRotation;
    private TacticStatus _unitStatus;

    protected void Init()
    {
        _tiles = GameObject.FindGameObjectsWithTag("Tile");
        
        _unitStatus = GetComponent<TacticStatus>();
        
        _halfHeight = GetComponent<Collider>().bounds.extents.y;
        TurnManager.AddUnit(this.gameObject);
        
        currentTile = GetTargetTile(this.gameObject);
        actualTargetTile = currentTile;

        //transform.position = new Vector3 (transform.position.x, actualTargetTile.transform.position.y + 1.9f, transform.position.z);
    }

    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
    }

    public void GetCurrentTile()
    {
        currentTile = GetTargetTile(this.gameObject);
        currentTile.current = true;        
    }

    // Recherche des tuiles sélectionnables
    public void FindSelectableTiles()
    {
        if (canMove)
        {
            RemoveSelectableTiles();
            ComputeAdjacencyLists(_unitStatus.jumpHeight, null);
            GetCurrentTile();

            Queue<Tile> process = new Queue<Tile>();
            process.Enqueue(currentTile);
            
            currentTile.visited = true;

            while (process.Count > 0)
            {
                Tile t = process.Dequeue();
                selectableTiles.Add(t);
                t.selectable = true;

                if (t.distance < _unitStatus.move)
                {
                    foreach (Tile tile in t.adjacencyList)
                    {
                        if (!tile.visited)
                        {
                            tile.parent = t;
                            tile.visited = true;
                            tile.distance = 1 + t.distance;
                            process.Enqueue(tile);
                        }
                    }
                }
            }
        }
    }

    // Fonction pour gérer le mouvement
    public void MoveToTile(Tile tile)
    {
        _lastPosition = gameObject.transform.position;
        _lastRotation = gameObject.transform.rotation;
        prevTargetTile = currentTile;

        path.Clear();
        tile.target = true;

        Tile next = tile;
        while (next != null)
        {
            path.Push(next);
            next = next.parent;
        }
    }

    public void Move()
    {
        if (path.Count > 0)
        {
            Tile t = path.Peek();
            Vector3 target = t.transform.position;
            target.y += _halfHeight + t.GetComponent<Collider>().bounds.extents.y;

            if (Vector3.Distance(transform.position, target) >= 0.05f)
            {
                bool jump = transform.position.y != target.y;

                if (jump)
                {
                    Jump(target);
                }
                else
                {
                    CalculateHeading(target);
                    SetHorizontalVelocity();
                }

                transform.forward = _heading;
                transform.position += _velocity * Time.deltaTime;
            }
            else
            {
                transform.position = target;
                path.Pop();
            }
        }
        else
        {
            RemoveSelectableTiles();
            moving = false;
            canMove = false;
            hasMoved = true;

            actualTargetTile.current = true;

            hoveredTile = null;
        }
    }

    public void UndoMove()
    {
        if (prevTargetTile != null)
        {
            transform.position = new Vector3(prevTargetTile.transform.position.x, _lastPosition.y , prevTargetTile.transform.position.z);
            transform.rotation = _lastRotation;
            
            path.Clear();
            
            moving = false;
            canMove = false;

            actualTargetTile.current = false;
            actualTargetTile = prevTargetTile;
            actualTargetTile.current = true;
            prevTargetTile = null;

            if(_boostActived)
            {
                _boostObject.SetActive(true);

                _unitStatus.currentActPoint -= _unitStatus.maxActPoint;
                _boostActived = false;
                _boostObject = null;
            }
        }
    }

    public Tile GetTargetTile(GameObject target)
    {
        RaycastHit hit;
        Tile tile = null;
        float objectHeight = 1f;

        Collider targetCollider = target.GetComponent<Collider>();
        if (targetCollider != null)
        {
            objectHeight = targetCollider.bounds.size.y;
        }
        else
        {
            Renderer targetRenderer = target.GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                objectHeight = targetRenderer.bounds.size.y;
            }
        }

        if (Physics.Raycast(target.transform.position, -Vector3.up, out hit, objectHeight))
        {
            tile = hit.collider.GetComponent<Tile>();
        }

        return tile;
    }

    public void ComputeAdjacencyLists(float jumpHeight, Tile target)
    {
        foreach (GameObject tile in _tiles)
        {
            Tile t = tile.GetComponent<Tile>();
            t.FindNeighbors(jumpHeight, target, false);
        }
    }

    public void RemoveSelectableTiles()
    {
        if (currentTile != null)
        {
            currentTile.visited = false;
            //currentTile.current = false;
            currentTile = null;
        }

        foreach (Tile tile in selectableTiles)
        {
            tile.Reset();
        }

        selectableTiles.Clear();
    }

    void CalculateHeading(Vector3 target)
    {
        _heading = target - transform.position;
        _heading.Normalize();
    }

    void SetHorizontalVelocity()
    {
        _velocity = _heading * _unitStatus.moveSpeed;
    }

    void Jump(Vector3 target)
    {
        if (_fallingDown)
        {
            FallDownward(target);
        }
        else if (_jumpingUp)
        {
            JumpUpward(target);
        }
        else if (_movingEdge)
        {
            MoveToEdge();
        }
        else
        {
            PrepareJump(target);
        }
    }

    void PrepareJump(Vector3 target)
    {
        float targetY = target.y;

        target.y = transform.position.y;

        CalculateHeading(target);

        //if (transform.position.y > targetY)
        if (targetY > transform.position.y)
        {
            _fallingDown = false;
            _jumpingUp = true;
            _movingEdge = false;

            _velocity = _heading * _unitStatus.moveSpeed / 1.75f;
            float difference = targetY - transform.position.y;
            _velocity.y = _unitStatus.jumpVelocity * (0.75f + difference / 2.0f);
        }
        else 
        {
            _fallingDown = false;
            _jumpingUp = false;
            _movingEdge = true;
            _jumpTarget = transform.position + (target - transform.position) / 2.0f;
        }
    }

     /*

    void PrepareJump(Vector3 target)
    {
        float targetY = target.y;

        target.y = transform.position.y;

        CalculateHeading(target);

        if (transform.position.y > targetY)
        {
            _fallingDown = false;
            _jumpingUp = false;
            _movingEdge = true;
            _jumpTarget = transform.position + (target - transform.position) / 2.0f;
        }
        else
        {
            _fallingDown = false;
            _jumpingUp = true;
            _movingEdge = false;

            _velocity = _heading * _unitStatus.moveSpeed / 3.0f;
            float difference = targetY - transform.position.y;
            _velocity.y = _unitStatus.jumpVelocity * (0.5f + difference / 2.0f);
        }
    }

    */

    void FallDownward(Vector3 target)
    {
        Vector3 directionToEdge = target - transform.position;
        directionToEdge.y = 0;
        
        if (directionToEdge.magnitude > 0.1f && _fallingDown && !_jumpingUp)
        {
            Vector3 moveDirection = directionToEdge.normalized; 
            transform.position += moveDirection * _unitStatus.moveSpeed * Time.deltaTime; 
        }
        else
        {
            _velocity += Physics.gravity * Time.deltaTime;

            if (transform.position.y <= target.y)
            {
                _fallingDown = false;
                _jumpingUp = false;
                _movingEdge = false;

                Vector3 p = transform.position;
                p.y = target.y;
                transform.position = p;

                _velocity = new Vector3();
            }
        }
    }

    /**

    void FallDownward(Vector3 target)
    {
        _velocity += Physics.gravity * Time.deltaTime;

        if (transform.position.y <= target.y)
        {            
            _fallingDown = false;
            _jumpingUp = false;
            _movingEdge = false;

            Vector3 p = transform.position;
            p.y = target.y;
            transform.position = p;
            _velocity = new Vector3();
        }
    }

    **/


    void JumpUpward(Vector3 target)
    {
        _velocity += Physics.gravity * Time.deltaTime;

        if (transform.position.y > target.y)
        {            
            //_jumpingUp = false;
            _fallingDown = true;            
        }
    }

    void MoveToEdge()
    {
        if (Vector3.Distance(transform.position, _jumpTarget) >= 0.05f)
        {
            SetHorizontalVelocity();
        }
        else
        {
            _movingEdge = false;
            _fallingDown = true;
            _velocity /= 5.0f;
            _velocity.y = 1.5f;
        }
    }

    protected Tile FindLowestF(List<Tile> list)
    {
        Tile lowest = list[0];
        foreach (Tile t in list)
        {
            if (t.f < lowest.f)
            {
                lowest = t;
            }
        }

        list.Remove(lowest);
        return lowest;
    }

    protected Tile FindEndTile(Tile t)
    {
        Stack<Tile> tempPath = new Stack<Tile>();
        Tile next = t.parent;

        while (next != null)
        {
            tempPath.Push(next);
            next = next.parent;
        }

        if (tempPath.Count <= _unitStatus.move)
        {
            return t.parent;
        }

        Tile endTile = null;
        for (int i = 0; i <= _unitStatus.move; i++)
        {
            endTile = tempPath.Pop();
        }

        return endTile;
    }

    protected void FindPath(Tile target, bool activeEndTile=true)
    {
        ComputeAdjacencyLists(_unitStatus.jumpHeight, target);
        GetCurrentTile();

        List<Tile> openList = new List<Tile>();
        List<Tile> closedList = new List<Tile>();

        openList.Add(currentTile);
        currentTile.h = Vector3.Distance(currentTile.transform.position, target.transform.position);
        currentTile.f = currentTile.h;

        while (openList.Count > 0)
        {
            Tile t = FindLowestF(openList);
            closedList.Add(t);

            if (t == target)
            {
                if (activeEndTile) 
                {
                    actualTargetTile = FindEndTile(t);
                    MoveToTile(actualTargetTile);
                }
                else 
                {
                    MoveToTile(target);
                }

                return;
            }

            foreach (Tile tile in t.adjacencyList)
            {
                if (!closedList.Contains(tile))
                {
                    float tempG = t.g + Vector3.Distance(tile.transform.position, t.transform.position);

                    if (tempG < tile.g)
                    {
                        tile.parent = t;
                        tile.g = tempG;
                        tile.f = tile.g + tile.h;
                    }
                    else if (!openList.Contains(tile))
                    {
                        tile.parent = t;
                        tile.g = t.g + Vector3.Distance(tile.transform.position, t.transform.position);
                        tile.h = Vector3.Distance(tile.transform.position, target.transform.position);
                        tile.f = tile.g + tile.h;
                        openList.Add(tile);
                    }
                }
            }
        }

        Debug.Log("Path not found");
    }

    protected void DetectBoost()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, .1f);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Boost") && !_boostActived)
            {
                _unitStatus.currentActPoint += _unitStatus.maxActPoint;
                _boostObject = hitCollider.gameObject;
                _boostActived = true;

                _boostObject.SetActive(false);

                Debug.Log("Boost détecté à proximité : " + hitCollider.gameObject.name);
                //InteractWithBoost(hitCollider.gameObject);
            }
        }
    }

    public void RemoveBoost()
    {
        if (_boostActived && _boostObject != null)
        {
            _boostObject.SetActive(true);
            _boostObject.GetComponent<BoostControl>().isActive = false;

            _boostActived = false;
            _boostObject = null;
        }
    }

    public void BeginTurn()
    {
        turn = true;
        actualTargetTile.current = true;

        TacticsCamera.Instance.SetTarget(this.gameObject.transform);
        TacticsCamera.Instance.ChangeOffset();
    }

    public void EndTurn()
    {
        turn = false;
        actualTargetTile.target = false;
        actualTargetTile.selectable = false;
        
        //actualTargetTile.current = true;

        //Debug.Break();

        TacticsCamera.Instance.ChangeOffset();
    }
}
