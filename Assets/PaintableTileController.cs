using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PaintableTileController : NetworkBehaviour, IExplodable
{
    [SerializeField] private PaintableTileManager tileManager;
    [SerializeField] private TeamColor color;
    public TeamColor Color { get { return color; } }
    [SerializeField] private Material none, red, blue;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public override void OnNetworkSpawn()
    {
        if (tileManager == null)
        {
            tileManager = FindObjectOfType<PaintableTileManager>();
        }
    }

    public void ExplosionHit(TeamColor color)
    {
        Debug.Log("Hit By Explosion!");
        SetColorServerRpc(color);
    }

    [ServerRpc]
    private void SetColorServerRpc(TeamColor team)
    {
        color = team;
        switch (color)
        {
            case TeamColor.RED:
                color = team;
                spriteRenderer.material = red;
                break;
            
            case TeamColor.BLUE:
                spriteRenderer.material = blue;
                break;
        }
    }
}
