using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAct : TacticsAct
{
    public bool skip;
    private List<GameObject> _targets = new List<GameObject>();

    private NPCAI _npcAI;

    private void Start()
    {
        _npcAI = GetComponent<NPCAI>();
        
        Init();
    }

    private void Update()
    {
        if (!CheckMyTurn())
        {
            return;
        }

        if (canAct && !acting)
        {
            if (CheckTilesToAttack())
            {
                //ACTIVE SELECTABLE TILES ATTACK
                FindSelectableTilesToAttack();

                if(selectableTiles.Count > 0)
                {
                    //FIND A TARGET ON SELECTABLE TILES
                    CheckForTargets();

                    //ATTACK IF TARGET FOUND
                    PrepareAttack();
                }
            }
            else
            {
                SkipAttack();
            }
        }
    }

    private void CheckForTargets()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");
        List<GameObject> targetsSelectable = new List<GameObject>();
        float lowestHealth = Mathf.Infinity;

        _targets.Clear();

        foreach (GameObject obj in targets)
        {
            if (selectableTiles.Contains(obj.GetComponent<PlayerMove>().actualTargetTile))
            {
                targetsSelectable.Add(obj);
                _targets.Add(obj);
            }
        }

        if (actSelected.zoneTarget == Act.RangeTarget.UNIQUE && targetsSelectable.Count > 0)
        {
            GameObject tempTarget = null;

            foreach (GameObject obj in targetsSelectable)
            {
                PlayerStatus playerStatus = obj.GetComponent<PlayerStatus>();
                if (playerStatus != null)
                {
                    if (playerStatus.currentHealth < lowestHealth)
                    {
                        lowestHealth = playerStatus.currentHealth;
                        tempTarget = obj;
                    }
                }
            }

            _targets.Clear();
            _targets.Add(tempTarget);
        }
    }

    private void PrepareAttack()
    {
        if (_targets.Count > 0)
        {
            Invoke("ProcessAttack", 0.5f);
            acting = true;
        }
        else 
        {
            skip = true;
            canAct = false;
        }
    }


    private void ProcessAttack()
    {
        OnTargetSelected(_targets);
        
        if (actSelected.typeAct == Act.TypeAct.TEMPORARY)
        {
            _npcAI.SetTempActUsed(true);
        }
    }

    private void SkipAttack()
    {
        skip = true;
        canAct = false;
    }
}
