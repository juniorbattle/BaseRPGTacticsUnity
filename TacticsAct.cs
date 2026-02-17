using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsAct : MonoBehaviour
{
    public bool acting = false;
    public bool canAct = false;
    public bool hasActed = false;
    public List<Tile> selectableTiles = new List<Tile>();

    protected Tile hoveredTile;
    protected Act actSelected = null;

    private GameObject[] tiles;
    private List<GameObject> _curTargets = new List<GameObject>();
    private bool _actMissed = false;
    private int _actRange = 1;
    private float _adjacencyRange = 0;
    private TacticsMove _unitMove;
    private TacticStatus _unitStatus;

    protected void Init()
    {
        _unitMove = GetComponent<TacticsMove>();
        _unitStatus = GetComponent<TacticStatus>();
        tiles = GameObject.FindGameObjectsWithTag("Tile");
    }

    public bool CheckMyTurn()
    {
        return _unitMove.turn;
    }

    public void SetCanAct(bool canAct)
    {
        this.canAct = canAct;
    }

    public void SetActRange(int range) 
    {
        this._actRange = range;
    }

    public void SetAdjacencyRange(float range) 
    {
        this._adjacencyRange = range;
    }

    public void SetActSelected(Act act) 
    {
        this.actSelected = act;
    }

    public void FindSelectableTilesToAttack()
    {
        if(canAct)
        {
            Tile currentTile = _unitMove.actualTargetTile;

            RemoveSelectableTiles();
            ComputeAdjacencyLists(2, currentTile, _adjacencyRange);

            Queue<Tile> process = new Queue<Tile>();
            process.Enqueue(currentTile);

            currentTile.visited = true;
                        
            while (process.Count > 0)
            {
                Tile t = process.Dequeue();
                
                selectableTiles.Add(t);
                t.selectable = true;

                if (t.distance < _actRange)
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

            selectableTiles.Remove(currentTile);

            if (_actRange > 1 && _adjacencyRange > 0) 
            {
                foreach (Tile tempTile in _unitMove.actualTargetTile.adjacencyList)
                {
                    tempTile.selectable = false;
                    selectableTiles.Remove(tempTile);
                }
            }
        }
    }

    protected bool CheckTilesToAttack()
    {
        Tile currentTile = _unitMove.actualTargetTile;
        List<Tile> tempTiles = new List<Tile>();

        ComputeAdjacencyLists(2, currentTile, _adjacencyRange);

        Queue<Tile> process = new Queue<Tile>();
        process.Enqueue(currentTile);

        while (process.Count > 0)
        {
            Tile t = process.Dequeue();

            tempTiles.Add(t);

            if (t.distance < _actRange)
            {
                foreach (Tile tile in t.adjacencyList)
                {
                    if (!tile.check)
                    {
                        tile.parent = t;
                        tile.check = true;
                        tile.distance = 1 + t.distance;
                        process.Enqueue(tile);
                    }
                }
            }
        }

        bool isTargetSelectable = false;
        foreach (Tile tile in tempTiles)
        {
            if (tile.CheckTileUsed() && currentTile != tile) 
            {
                isTargetSelectable = true;
                break; 
            }
        }

        return isTargetSelectable;
    }

    public Tile GetTargetTile(GameObject target)
    {
        RaycastHit hit;
        Tile tile = null;
        float objectHeight = 0f;

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

    public void ComputeAdjacencyLists(float jumpHeight, Tile target, float adjacencyRange)
    {
        foreach (GameObject tile in tiles)
        {
            Tile t = tile.GetComponent<Tile>();
            t.FindNeighbors(jumpHeight, target, true, adjacencyRange);
        }
    }

    public void RemoveSelectableTiles()
    {
        if (_unitMove.actualTargetTile != null)
        {
            //_unitMove.actualTargetTile.current = false;
            _unitMove.actualTargetTile.selectable = false;
        }

        foreach (Tile tile in selectableTiles)
        {
            tile.Reset();
        }

        selectableTiles.Clear();
    }

    public void OnTileSelected(Tile selectedTile)
    {
        // Si la tuile est bien sélectionnable, lancer l'attaque
        if (selectedTile.selectable)
        {
            //StartCoroutine(PerformAttack(selectedTile));

            //selectedTile.target = true; // Marquer la tuile comme cible
            //acting = true;
        }
    }

    public void OnTargetSelected(List<GameObject> selectedTargets)
    {
        _curTargets = new List<GameObject>(selectedTargets);

        RemoveSelectableTiles();
        StartCoroutine(PerformAttack());

        foreach (GameObject tempTarget in _curTargets)
        {
            tempTarget.GetComponent<TacticsMove>().actualTargetTile.target = true;
        }

        RotateTowardsTarget(_curTargets[0].transform.position);
    }

    // Coroutine pour effectuer l'attaque avec une animation de mouvement
    IEnumerator PerformAttack()
    {
        Vector3 startPos = transform.position;
        float duration = 0.2f; // Durée du mouvement (animation)
        float elapsedTime = 0f;

        yield return new WaitForSeconds(0.1f);

        if(actSelected != null && actSelected.projectileObj != null)
        {
            GameObject tempProjectile = Instantiate(actSelected.projectileObj);
            ProjectileController projectileController = tempProjectile.GetComponent<ProjectileController>();
            tempProjectile.transform.position = transform.position; // Example of setting position

            TacticsCamera.Instance.SetTarget(tempProjectile.transform);

            projectileController.LaunchProjectile(_curTargets[0].transform.position);

            while (projectileController.isReached == false && projectileController.isMissed == false)
            {
                yield return null;
            }

            if(projectileController.isMissed) _actMissed = true;

            TacticsCamera.Instance.SetTarget(this.gameObject.transform);

            Destroy(tempProjectile);
        }
        else
        {
            Vector3 targetPos = new Vector3(_curTargets[0].transform.position.x, transform.position.y, _curTargets[0].transform.position.z);
            
            // Animation simple de mouvement vers la cible (Contact Attack)
            while (elapsedTime < duration)
            {
                transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        transform.position = startPos;
        
        yield return new WaitForSeconds(0.1f);

        ExecuteAttack();
    }

    void ExecuteAttack()
    {
        int tempAttackDamage = GetAttackDamage();

        string typeDamage = (actSelected.typeAbility == Act.TypeAbility.MAGIC)? "magic" : "physic";

        foreach (GameObject tempTarget in _curTargets)
        {
            tempTarget.GetComponent<TacticStatus>().TakeDamage(tempAttackDamage, typeDamage);
            // Effect Status...
        }

        Debug.Log("Attaque avec : " + actSelected.nameAct);

        acting = false;
        canAct = false;

        _actMissed = false;

        hoveredTile = null;
        actSelected = null;

        _curTargets.Clear();

        hasActed = true;
        
        if(_unitMove.hasMoved) 
        {
            _unitMove.canCancelMove = false;
        }

    }

    int GetAttackDamage()
    {
        int tempAttackDamage = 0;

        tempAttackDamage = (actSelected.typeAbility == Act.TypeAbility.MAGIC)? actSelected.amount + _unitStatus.magic : actSelected.amount + _unitStatus.power;

        _unitStatus.currentActPoint -= actSelected.point;

        if (actSelected.typeAct == Act.TypeAct.TEMPORARY)
        {
            _unitStatus.tempMenu.Remove(actSelected);
        }

        if(_actMissed) tempAttackDamage = -1;


        // Success Rate... 
        // Critic Rate...

        return tempAttackDamage;
    }

    private void RotateTowardsTarget(Vector3 targetPosition)
    {
        float currentRotationX = transform.rotation.eulerAngles.x;
        float currentRotationZ = transform.rotation.eulerAngles.z;

        Vector3 direction = targetPosition - transform.position;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        targetRotation = Quaternion.Euler(currentRotationX, targetRotation.eulerAngles.y, currentRotationZ);

        transform.rotation = targetRotation;
    }
}
