using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticStatus : MonoBehaviour
{
    public enum Affinity
    {
        RED,
        YELLOW,
        BLUE,
        GREEN,
        WHITE,
        BLACK
    }
    public Affinity currentAffinity = Affinity.RED; 
    public int maxHealth = 100;      // Santé maximale de l'unité
    public int currentHealth = 100;  // Santé actuelle de l'unité
    public int maxActPoint = 2; // Point d'action maximal par tour de l'unité
    public int currentActPoint = 2; // Point d'action par tour de l'unité
    public int power = 10;          // Puissance physique de l'unité
    public int magic = 10;          // Magie de l'unité
    public int defense = 5;          // Défense physique de l'unit
    public int resistance = 5;       // Résistence magique de l'unité
    public int agility = 5;         // Vitesse d'agissement par tour
    public int chance = 1;          // Chance de reussir ou esquiver une attaque 
    public int moveDistance = 3;  
    public float moveSpeed = 2;
    public int move = 5;
    public float jumpHeight = 2;
    public float jumpVelocity = 4.5f;
    public List<Act> basicMenu = new List<Act>();
    public List<Act> tempMenu = new List<Act>();
    public List<Act> tempLibrary = new List<Act>();

    private List<Act> _tempCurrentLibrary = new List<Act>();
    private System.Random _random = new System.Random();

    [SerializeField] private Healthbar _healthbar;

    void Start()
    {
        currentHealth = maxHealth;

        currentActPoint = maxActPoint;

        _tempCurrentLibrary = new List<Act>(tempLibrary);

        _healthbar.UpdateHealthBar(maxHealth, currentHealth);

        AddRandomTempActToMenu();
    }


    public void TakeDamage(int damage, string typeDamage)
    {
        int damageAfterDefense = 0;

        if (damage > -1)
        {
            damageAfterDefense = (typeDamage == "magic")? Mathf.Max(damage - resistance, 0) : Mathf.Max(damage - defense, 0);
            currentHealth -= damageAfterDefense;
            currentHealth = Mathf.Max(currentHealth, 0);

            _healthbar.UpdateHealthBar(maxHealth, currentHealth);

            if (currentHealth == 0)
            {            
                TurnManager.RemoveUnit(this.gameObject);
                Debug.Log(gameObject.name + " a été retiré du tour.");
            }
        }
        else 
        {
            Debug.Log("Attaque manquée. Aucun Dégats.");
        }
    }

    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        _healthbar.UpdateHealthBar(maxHealth, currentHealth);
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    public void AddRandomTempActToMenu()
    {
        int maxTempActs = 4;
        int actsToAdd = Math.Min(maxTempActs - tempMenu.Count, _tempCurrentLibrary.Count);

        for (int i = 0; i < actsToAdd; i++)
        {
            int randomIndex = _random.Next(_tempCurrentLibrary.Count);

            Act actToAdd = _tempCurrentLibrary[randomIndex];

            if (!tempMenu.Contains(actToAdd))
            {
                tempMenu.Add(actToAdd);
                _tempCurrentLibrary.Remove(actToAdd);
            }
        }
    }

    public void DisplayStats()
    {
        //Debug.Log("Attaque: " + power + ", Défense: " + defense + ", Santé actuelle: " + currentHealth + "/" + maxHealth);
    }
}
