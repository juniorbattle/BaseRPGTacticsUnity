using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAI : MonoBehaviour
{
    public enum NPCState
    {
        SelectingAction,
        ChoosingAct,
        Moving,          
        Acting,         
        EndTurn         
    }
    public NPCState currentState = NPCState.SelectingAction;

    private NPCMove _npcMove;
    private NPCAct _npcAct;
    private NPCStatus _npcStatus;

    private bool _tempActUsed = false;
    //private bool attackUsed = false;

    void Start()
    {
        _npcMove = GetComponent<NPCMove>();
        _npcAct = GetComponent<NPCAct>();
        _npcStatus = GetComponent<NPCStatus>();
    }

    void Update()
    {
        if (!_npcMove.turn) return;

        switch (currentState)
        {
            case NPCState.SelectingAction:
                HandleSelectingAction();
                break;
            case NPCState.ChoosingAct:
                HandleChoosingAct();
                break;
            case NPCState.Moving:
                HandleMovement();
                break;
            case NPCState.Acting:
                HandleActing();
                break;
            case NPCState.EndTurn:
                HandleEndTurn();
                break;
        }
    }

    public void SetTempActUsed(bool isUsed)
    {
        this._tempActUsed = isUsed;
    }

    void HandleSelectingAction()
    {
        //_npcAct.hasActed = _npcStatus.currentActPoint == 0;

        if (!_npcAct.hasActed && !_npcAct.skip) 
        {
            currentState = NPCState.ChoosingAct;
        }
        else if (!_npcMove.hasMoved) 
        {
            _npcMove.canMove = true;
            currentState = NPCState.Moving;
        }
        else 
        {
            currentState = NPCState.EndTurn;
        }
    }

    void HandleMovement()
    {
        if(_npcMove.hasMoved)
        {
            _npcAct.skip = false;
            currentState = NPCState.SelectingAction;
        }
    }

    void HandleActing()
    {
        if(_npcAct.hasActed || _npcAct.skip)
        {
            if(_npcStatus.currentActPoint > 0) 
            {
                _npcAct.hasActed = false;
            } 

            _npcAct.RemoveSelectableTiles();
            _npcAct.canAct = false;
            currentState = NPCState.SelectingAction;
        }
    }

    void HandleEndTurn()
    {        
        _npcMove.hasMoved = false;
        _npcMove.prevTargetTile = null;
        _npcMove.canMove = false;
        _npcAct.canAct = false;
        _npcAct.hasActed = false;
        _npcAct.skip = false;

        _npcStatus.currentActPoint = _npcStatus.maxActPoint;

        _tempActUsed = false;

        _npcMove.RemoveBoost();

        _npcAct.SetActSelected(null);

        _npcStatus.AddRandomTempActToMenu();

        TurnManager.EndTurn();
        currentState = NPCState.SelectingAction;
    }

    void HandleChoosingAct()
    {
        if(!_tempActUsed && _npcStatus.tempMenu.Count > 0)
        {
            foreach(Act tempAct in _npcStatus.tempMenu)
            {
                if(tempAct.point <= _npcStatus.currentActPoint)
                {
                    _npcAct.SetActRange(tempAct.range);
                    _npcAct.SetAdjacencyRange(tempAct.adjacency);
                    _npcAct.SetActSelected(tempAct);
                    break;
                }
            }
        }
        else 
        {
            if (_npcStatus.basicMenu.Count > 0)
            {
                int index = Random.Range(0, _npcStatus.basicMenu.Count);
                Act tempAct = _npcStatus.basicMenu[index]; 
                _npcAct.SetActRange(tempAct.range);
                _npcAct.SetAdjacencyRange(tempAct.adjacency);
                _npcAct.SetActSelected(tempAct);
            }
        }

        _npcAct.canAct = true;
        currentState = NPCState.Acting;
    }
}
