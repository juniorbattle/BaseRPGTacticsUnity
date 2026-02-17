using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMenu : MonoBehaviour
{
    public enum PlayerState
    {
        SelectingAction, // Lorsqu'un joueur est dans le menu pour choisir une action
        SelectingAct,
        SelectingSkill,
        Moving,          // Lorsqu'un joueur est en train de se déplacer
        Acting,          // Lorsqu'un joueur effectue une action
        EndTurn          // Lorsqu'un joueur termine son tour
    }
    public PlayerState currentState = PlayerState.SelectingAction;

    public Text NameMenuText;
    public Text PointMenuText;
    public Button MoveButton;
    public Button UndoMoveButton;
    public Button ActButton;
    public Button TempActButton;
    public Button EndTurnButton;

    public GameObject ActionMenu;
    public GameObject ActMenu;
    public GameObject TempActMenu;
    public GameObject SelectedActMenu;

    public Text NameSelectedMenuText;
    public Text PointSelectedMenuText;
    public Text DescriptionSelectedMenuText;

    public List<Button> BasicButtons = new List<Button>();
    public List<Button> TempButtons = new List<Button>();

    private PlayerMove _playerMove;
    private PlayerAct _playerAct;
    private PlayerStatus _playerStatus;

    void Awake()
    {
        MoveButton.onClick.AddListener(HandleMoveSelection_onClick);
        UndoMoveButton.onClick.AddListener(HandleUndoMoveSelection_onClick);
        ActButton.onClick.AddListener(HandleBasicActSelection_onClick);
        TempActButton.onClick.AddListener(HandleTempActSelection_onClick);
        EndTurnButton.onClick.AddListener(HandleEndTurnSelection_onClick);
    }
    
    void Start()
    {
        _playerMove = GetComponent<PlayerMove>();
        _playerAct = GetComponent<PlayerAct>();
        _playerStatus = GetComponent<PlayerStatus>();

        NameMenuText.text = gameObject.name;
        PointMenuText.text = _playerStatus.currentActPoint.ToString();

        ActionMenu.SetActive(false);
        ActMenu.SetActive(false);
        TempActMenu.SetActive(false);
        SelectedActMenu.SetActive(false);
    }

    void Update()
    {
        if (!_playerMove.turn) return;

        switch (currentState)
        {
            case PlayerState.SelectingAction:
                HandleSelectingAction();
                break;
            case PlayerState.Moving:
                HandleMovement();
                break;
            case PlayerState.SelectingAct:
                HandleSelectingAct();
                break;
            case PlayerState.Acting:
                HandleActing();
                break;
            case PlayerState.SelectingSkill:
                HandleSelectingSkill();
                break;
            case PlayerState.EndTurn:
                HandleEndTurn();
                break;
        }
    }
    
    void HandleSelectingAction()
    {
        if (_playerMove.canCancelMove && _playerMove.hasMoved && !_playerMove.canMove) 
        {
            UndoMoveButton.gameObject.SetActive(true);
            MoveButton.gameObject.SetActive(false);
        } 
        else 
        {
            MoveButton.interactable = true;
            MoveButton.gameObject.SetActive(true);
            UndoMoveButton.gameObject.SetActive(false);

            if(!_playerMove.canCancelMove && _playerMove.hasMoved)
            {
                MoveButton.interactable = false;
            }
        }

        ActButton.interactable = _playerStatus.currentActPoint > 0;
        _playerAct.hasActed = _playerStatus.currentActPoint == 0;

        ShowActionMenu();
    }

    void ShowActionMenu()
    {
        if(ActionMenu.activeSelf) return;

        //Debug.Break();

        PointMenuText.text = _playerStatus.currentActPoint.ToString();

        ActionMenu.SetActive(true);
        ActMenu.SetActive(false);
        TempActMenu.SetActive(false);
        SelectedActMenu.SetActive(false);
    }

    void HandleMoveSelection_onClick()
    {
        if(!_playerMove.hasMoved)
        {
            TacticsCamera.Instance.ChangeOffset();
            currentState = PlayerState.Moving;
            _playerMove.SetCanMove(true);
            _playerAct.SetCanAct(false);
            ActionMenu.SetActive(false);
            ActMenu.SetActive(false);
            TempActMenu.SetActive(false);
            SelectedActMenu.SetActive(false);
        }
    }

    void HandleUndoMoveSelection_onClick()
    {
        if(_playerMove.canCancelMove && _playerMove.hasMoved && !_playerMove.canMove)
        {
            UndoMove();
        }
    }

    void HandleBasicActSelection_onClick()
    {
        DisplayBasicActAvailable();

        currentState = PlayerState.SelectingAct;
        ActionMenu.SetActive(true);
        ActMenu.SetActive(true);
    }

    void HandleTempActSelection_onClick()
    {
        DisplaytempActAvailable();
        
        currentState = PlayerState.SelectingSkill;
        ActionMenu.SetActive(true);
        ActMenu.SetActive(true);
        TempActMenu.SetActive(true);
    }

    public void HandleProcessActSelection_onClick(Button clickedButton)
    {
        if(!_playerAct.hasActed)
        {
            if (clickedButton.GetComponent<ActUI>() != null)
            {
                Act tempAct = clickedButton.GetComponent<ActUI>().act;
                _playerAct.SetActRange(tempAct.range);
                _playerAct.SetAdjacencyRange(tempAct.adjacency);
                _playerAct.SetActSelected(tempAct);

                NameSelectedMenuText.text = tempAct.nameAct;
                PointSelectedMenuText.text = tempAct.point.ToString();
                DescriptionSelectedMenuText.text = tempAct.description;
            }
            else
            {
                return;
            }
            
            currentState = PlayerState.Acting;
            _playerAct.SetCanAct(true);
            _playerMove.SetCanMove(false);
            ActionMenu.SetActive(false);
            ActMenu.SetActive(false);
            TempActMenu.SetActive(false);

            SelectedActMenu.SetActive(true);

        }
    }

    void HandleEndTurnSelection_onClick()
    {
        currentState = PlayerState.EndTurn;
    }

    void HandleMovement()
    {
        if(_playerMove.moving) return;

        if (_playerMove.hasMoved || Input.GetMouseButtonUp(1))
        {
            TacticsCamera.Instance.ChangeOffset();
            _playerMove.RemoveSelectableTiles();
            _playerMove.canMove = false;
            currentState = PlayerState.SelectingAction;
        }
    }

    void HandleSelectingAct()
    {
        if(Input.GetMouseButtonUp(1))
        {
            ActMenu.SetActive(false);
            currentState = PlayerState.SelectingAction;
        }
    }

    void HandleSelectingSkill()
    {
        if(Input.GetMouseButtonUp(1))
        {
            TempActMenu.SetActive(false);
            currentState = PlayerState.SelectingAct;
        }
    }

    void HandleActing()
    {
        if(_playerAct.acting) return;

        if(_playerAct.hasActed || Input.GetMouseButtonUp(1)) 
        {   
            if(_playerStatus.currentActPoint > 0) 
            {
                _playerAct.hasActed = false;
            } 

            _playerAct.SetActSelected(null);

            _playerAct.RemoveSelectableTiles();
            _playerAct.canAct = false;
            currentState = PlayerState.SelectingAction; 
        }
    }

    void HandleEndTurn()
    {
        _playerMove.hasMoved = false;
        _playerAct.hasActed = false;
        _playerMove.canCancelMove = true;
        _playerMove.prevTargetTile = null;
        _playerMove.canMove = false;
        _playerAct.canAct = false;

        _playerStatus.currentActPoint = _playerStatus.maxActPoint;

        ActionMenu.SetActive(false);
        ActMenu.SetActive(false);
        TempActMenu.SetActive(false);

        _playerMove.RemoveBoost();

        _playerAct.SetActSelected(null);

        _playerStatus.AddRandomTempActToMenu();

        currentState = PlayerState.SelectingAction;
        TurnManager.EndTurn();
    }

    void UndoMove()
    {
        _playerMove.canMove = false;
        _playerAct.canAct = false;
        _playerMove.hasMoved = false;
        _playerMove.RemoveSelectableTiles();
        _playerAct.RemoveSelectableTiles();
        _playerMove.UndoMove();  // Appel à la méthode UndoMove du script PlayerMove
        currentState = PlayerState.SelectingAction;  // Revenir à la sélection d'action
    }

    void DisplayBasicActAvailable()
    {
        foreach (Button curButton in BasicButtons)
        {
            curButton.gameObject.SetActive(false);
            curButton.interactable = true;
        }

        foreach(Act curAct in _playerStatus.basicMenu)
        {
            foreach (Button curButton in BasicButtons)
            {
                if(!curButton.gameObject.activeSelf)
                {
                    curButton.GetComponent<ActUI>().nameText.text = curAct.nameAct;
                    curButton.GetComponent<ActUI>().pointText.text = curAct.point.ToString();
                    curButton.GetComponent<ActUI>().act = curAct;
                    curButton.gameObject.SetActive(true);
                    if(_playerStatus.currentActPoint < curAct.point)
                    {
                        curButton.interactable = false;
                    }
                    break;
                }
            }
        }
    }

    void DisplaytempActAvailable()
    {
        foreach (Button curButton in TempButtons)
        {
            curButton.gameObject.SetActive(false);
            curButton.interactable = true;
        }

        foreach(Act curAct in _playerStatus.tempMenu)
        {
            foreach (Button curButton in TempButtons)
            {
                if(!curButton.gameObject.activeSelf)
                {
                    curButton.GetComponent<ActUI>().nameText.text = curAct.nameAct;
                    curButton.GetComponent<ActUI>().pointText.text = curAct.point.ToString();
                    curButton.GetComponent<ActUI>().act = curAct;
                    curButton.gameObject.SetActive(true);
                    if(_playerStatus.currentActPoint < curAct.point)
                    {
                        curButton.interactable = false;
                    }
                    break;
                }
            }
        }
    }
}
