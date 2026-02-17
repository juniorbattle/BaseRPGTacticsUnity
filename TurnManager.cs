using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour 
{
    public bool start = false;
    private static TurnManager _instance;

    public static TurnManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TurnManager>();

                if (_instance == null)
                {
                    GameObject go = new GameObject("TurnManager");
                    _instance = go.AddComponent<TurnManager>();
                }
            }
            return _instance;
        }
    }
    
    static Dictionary<string, List<TacticsMove>> units = new Dictionary<string, List<TacticsMove>>();
    static Queue<string> turnKey = new Queue<string>();
    static Queue<TacticsMove> turnTeam = new Queue<TacticsMove>();
	
	void Update () 
	{
        if (start && turnTeam.Count == 0)
        {
            InitTeamTurnQueue();
        }
	}

    static void InitTeamTurnQueue()
    {
        List<TacticsMove> teamList = units[turnKey.Peek()];

        foreach (TacticsMove unit in teamList)
        {
            turnTeam.Enqueue(unit);
        }

        StartTurn();
    }

    public static void StartTurn()
    {   
        if (turnTeam.Count > 0)
        {
            turnTeam.Peek().BeginTurn();
        }
    }

    public static void EndTurn()
    {
        TacticsMove unit = turnTeam.Dequeue();
        unit.EndTurn();

        if (turnTeam.Count > 0)
        {
            StartTurn();
        }
        else
        {
            string team = turnKey.Dequeue();
            turnKey.Enqueue(team);
            InitTeamTurnQueue();
        }
    }

    public static void AddUnit(GameObject tempUnit)
    {
        List<TacticsMove> list;
        TacticsMove unit = tempUnit.GetComponent<TacticsMove>();

        if (!units.ContainsKey(unit.tag))
        {
            list = new List<TacticsMove>();
            units[unit.tag] = list;

            if (!turnKey.Contains(unit.tag))
            {
                turnKey.Enqueue(unit.tag);
            }
        }
        else
        {
            list = units[unit.tag];
        }

        list.Add(unit);
    }

    public static void RemoveUnit(GameObject tempUnit)
    {
        TacticsMove unit = tempUnit.GetComponent<TacticsMove>();

        if (units.ContainsKey(unit.tag))
        {
            List<TacticsMove> unitList = units[unit.tag];
            if (unitList.Contains(unit))
            {
                unitList.Remove(unit);
                
                // Si la liste est vide après la suppression, retirer la clé du dictionnaire
                if (unitList.Count == 0)
                {
                    units.Remove(unit.tag);
                    // Reconstruction de turnKey sans la clé de l'unité supprimée
                    Queue<string> newTurnKey = new Queue<string>();
                    foreach (var team in turnKey)
                    {
                        if (team != unit.tag)
                        {
                            newTurnKey.Enqueue(team);
                        }
                    }
                    turnKey = newTurnKey; // Mettre à jour turnKey
                }
            }
        }

        Queue<TacticsMove> newTurnTeam = new Queue<TacticsMove>();
        while (turnTeam.Count > 0)
        {
            TacticsMove currentUnit = turnTeam.Dequeue();
            if (currentUnit != unit) 
            {
                newTurnTeam.Enqueue(currentUnit);
            }
        }

        Destroy(tempUnit);

        turnTeam = newTurnTeam;
    }
}
