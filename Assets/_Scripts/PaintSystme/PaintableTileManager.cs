using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class PaintableTileManager : NetworkBehaviour
{
    [SerializeField] private List<PaintableTileController> paintableTiles;
    [SerializeField] private NetworkVariable<int> numberOfRedTiles; 
    [SerializeField] private NetworkVariable<int> numberOfBlueTiles; 

    

    public TeamColor CheckWinningTeam()
    {
        foreach (PaintableTileController tile in paintableTiles)
        {
            switch (tile.PaintColor.Value)
            {
                case TeamColor.RED:
                    numberOfRedTiles = new NetworkVariable<int>(numberOfRedTiles.Value++);
                    break;

                case TeamColor.BLUE:
                    numberOfBlueTiles = new NetworkVariable<int>(numberOfBlueTiles.Value++);
                    break;

                case TeamColor.NONE:
                    break;
            }
        }
        if (numberOfRedTiles.Value > numberOfBlueTiles.Value)
        {
            return TeamColor.RED;
        }
        else if (numberOfRedTiles.Value < numberOfBlueTiles.Value)
        {
            return TeamColor.BLUE;
        }
        return TeamColor.NONE;
    }
}
