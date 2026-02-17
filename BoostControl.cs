using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostControl : MonoBehaviour
{
    public bool isActive;

    private GameObject[] tiles; 
    private System.Random random = new System.Random();

    private GameObject prevTile;

    void Start()
    {
        tiles = GameObject.FindGameObjectsWithTag("Tile");
        
    }

    void Update()
    {
        if (!isActive)
        {
            GameObject randomTile;
            int randomIndex;

            do
            {
                randomIndex = random.Next(tiles.Length);
                randomTile = tiles[randomIndex];
            } while (randomTile == prevTile || randomTile.GetComponent<Tile>().CheckTileUsed());  // Si randomTile est identique à prevTile, recommence


            this.transform.position = new Vector3(randomTile.transform.position.x, randomTile.transform.position.y + 1.9f, randomTile.transform.position.z);

            prevTile = randomTile;
            isActive = true;            
        }        
    }
}
