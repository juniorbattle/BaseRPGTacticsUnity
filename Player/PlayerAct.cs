using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAct : TacticsAct
{
    private List<GameObject> _targets = new List<GameObject>();

    void Start()
    {
        Init();
    }

    void Update() 
    {
        if (!CheckMyTurn())
        {
            return;
        }

        if (!acting)
        {
            if (canAct) // Seulement autoriser la sélection si act est activé
            {
                if(selectableTiles.Count == 0) Invoke("FindSelectableTilesToAttack", 0.1f);
                CheckMouse();
                HoverTile(); 
            }
        }
    }

    void CheckMouse()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            _targets.Clear();

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("NPC"))
                {
                    NPCMove npc = hit.collider.GetComponent<NPCMove>();

                    if (npc.actualTargetTile.selectable)
                    {
                        //OnTileSelected(npc.actualTargetTile);
                        acting = true;

                        _targets.Add(hit.collider.gameObject);

                        if(actSelected.zoneTarget == Act.RangeTarget.MULTIPLE)
                        {
                            foreach (Tile t in selectableTiles)
                            {
                                GameObject tempTarget = t.GetHitFromTile();
                                if (tempTarget != null)
                                {
                                    _targets.Add(tempTarget);
                                }
                            }
                        }
                        
                        OnTargetSelected(_targets);

                        if (hoveredTile != null)
                        {
                            hoveredTile.hover = false;
                        }
                    }
                }
            }
        }
    }

    // Gère le survol de la souris
    void HoverTile()
    {
        if(selectableTiles.Count > 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Tile t = null;

                if (hit.collider.CompareTag("NPC"))
                {
                    NPCMove npc = hit.collider.GetComponent<NPCMove>();
                    t = npc.actualTargetTile;
                }
                else if (hit.collider.CompareTag("Tile"))
                {
                    t = hit.collider.GetComponent<Tile>();
                }

                if (t != null && t.selectable)
                {
                    if (hoveredTile != null && t != hoveredTile)
                    {
                        hoveredTile.hover = false;
                    }
                    
                    t.hover = true;
                    hoveredTile = t;
                }
            }
        }
    }
}
