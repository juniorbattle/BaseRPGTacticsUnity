using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMove : TacticsMove 
{
    public bool blocked = false;

    private GameObject _target;

    private void Start()
    {
        Init();

        if(transform.position.y > 3) blocked = true;
    }

    private void Update()
    {
        if (!turn)
        {
            return;
        }

        if (canMove)
        {
            if(blocked)
            {
                moving = false;
                canMove = false;
                hasMoved = true;

                return;
            }

            if (!moving)
            {
                FindWeakestPlayer();
                if(_target == null) FindNearestTarget();
                
                if (_target != null)
                {
                    CalculatePath();
                    actualTargetTile.target = true;
                }
            }
            else
            {
                Move();
                DetectBoost();
            }
        }
    }

    void CalculatePath()
    {
        Tile targetTile = GetTargetTile(_target);
        FindPath(targetTile, false);

        AdjustPathToMaintainDistance(targetTile);
    }

    public void FindNearestTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");

        GameObject nearest = null;
        float distance = Mathf.Infinity;

        foreach (GameObject obj in targets)
        {
            float d = Vector3.Distance(transform.position, obj.transform.position);

            if (d < distance)
            {
                distance = d;
                nearest = obj;
            }
        }

        _target = nearest;
    }

    public void FindWeakestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject weakestPlayer = null;
        float lowestHealth = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            PlayerStatus playerStatus = player.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                if (playerStatus.currentHealth < lowestHealth)
                {
                    lowestHealth = playerStatus.currentHealth;
                    weakestPlayer = player;
                }
            }
        }

        _target = weakestPlayer;
    }

    void AdjustPathToMaintainDistance(Tile targetTile)
    {
        NPCStatus unitStatus = GetComponent<NPCStatus>();
        NPCAct unitAct = GetComponent<NPCAct>();

        int pathCount = path.Count-1;

        // Si l'unité est trop éloignée de la cible, on se rapproche
        if (pathCount > unitStatus.moveDistance && !unitAct.hasActed)
        {
            FindPath(targetTile);
            FindSelectableTiles();
            moving = true;
        }
        else
        {
            GameObject[] points = GameObject.FindGameObjectsWithTag("Point");

            GameObject farthestPoint = null;
            float maxDistance = -Mathf.Infinity; 

            foreach (GameObject point in points)
            {
                float distance = Vector3.Distance(_target.transform.position, point.transform.position);

                if (distance > maxDistance)
                {
                    maxDistance = distance;  // Mise à jour de la distance maximale
                    farthestPoint = point;   // Mise à jour du point le plus éloigné
                }
            }

            Tile newTargetTile = GetTargetTile(farthestPoint);

            if(newTargetTile != null)
            {
                FindPath(newTargetTile);  // Trouver le chemin vers cette nouvelle position éloignée
                FindSelectableTiles();
                moving = true;
            }
        }
    }
}
