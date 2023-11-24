using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PaintableTileController : NetworkBehaviour, IExplodable
{
    [SerializeField] private PaintableTileManager tileManager;
    [SerializeField] public NetworkVariable<TeamColor> PaintColor = new NetworkVariable<TeamColor>(TeamColor.NONE, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private Material none, red, blue;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public override void OnNetworkSpawn()
    {
        if (tileManager == null)
        {
            tileManager = FindObjectOfType<PaintableTileManager>();
            PaintColor.OnValueChanged += (previousValue, newValue) =>
            {
                UpdateColor(newValue);
            };
        }
    }

    public void ExplosionHit(TeamColor color)
    {
        PaintColor.Value = color;
    }
    public void UpdateColor(TeamColor newcolor)
    {
        switch (newcolor)
        {
            case TeamColor.NONE:
                spriteRenderer.material = none;
                break;
            case TeamColor.RED:
                spriteRenderer.material = red;
                break;

            case TeamColor.BLUE:
                spriteRenderer.material = blue;
                break;
        }
    }
}
