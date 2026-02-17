using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Act", menuName = "Element/Act")]
public class Act : ScriptableObject
{
    public int ID = 0;

    public string nameAct;

    public enum TypeAct
    {
        PERMANENT,
        TEMPORARY
    }
    public TypeAct typeAct;

    [TextArea(3, 20)]
    public string description;

    public int point;

    public int range;

    public float adjacency;
    
    public int amount;
    
    public int criticRate;
    
    public int successRate;

    public enum TypeAbility
    {
        PHYSIC,
        MAGIC
    }
    public TypeAbility typeAbility;

    public enum TypeAction 
    {
        DAMAGE,
        HEAL,
        EFFECT
    }
    public TypeAction typeAction;

    public enum RangeTarget
    {
        UNIQUE,
        MULTIPLE
    }
    public RangeTarget zoneTarget;

    public float duration;

    public GameObject weaponObj;

    public GameObject projectileObj;

    public GameObject impactObj;

    public enum EffectsAct
    {
        NONE,
        SLEEP,
        POISON,
        PARALYZE,
        BLIND,
        MUTE
    }

    [System.Serializable]
    public class Effects
    {
        public EffectsAct effect;
        public int successRate;
    }
    public List<Effects> effectsList = new List<Effects>();


    public virtual void UseAct()
    {

    }
}
