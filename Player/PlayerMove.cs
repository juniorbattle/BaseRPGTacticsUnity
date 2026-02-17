using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : TacticsMove 
{
    void Start()
    {
        Init();
    }

    void Update() 
    {
        if (!turn)
        {
            return;
        }

        if (!moving)
        {
            if (canMove) // Seulement autoriser la sélection si le mouvement est activé
            {
                if(selectableTiles.Count == 0) Invoke("FindSelectableTiles", 0.1f);
                CheckMouse();
                HoverTile();
            }
        }
        else
        {
            Move();
            DetectBoost();
        }
    }

    void CheckMouse()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Tile"))
                {
                    Tile t = hit.collider.GetComponent<Tile>();
                    if (t.selectable)
                    {
                        MoveToTile(t);
                        actualTargetTile = t;
                        moving = true;
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
                if (hit.collider.CompareTag("Tile"))
                {
                    Tile t = hit.collider.GetComponent<Tile>();

                    if (hoveredTile != null &&  t != hoveredTile)
                    {
                        hoveredTile.hover = false;
                    }

                    if (t.selectable)
                    {
                        t.hover = true;
                        hoveredTile = t;
                    }
                }
            }
        }
    }
}
