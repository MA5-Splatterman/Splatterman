using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableWallController : NetworkBehaviour, IExplodable
{

    [SerializeField] private GameObject particles;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject powerup;
    [SerializeField] private int PowerupDropProbability;
    private bool hasRolledPowerupDrop = false;
    private TeamColor color;

    [ClientRpc]
    public void HasBeenHitClientRpc()
    {
        Debug.Log("ClientRpc");
        Instantiate(particles, transform.position, Quaternion.identity);
        spriteRenderer.enabled = false;
    }

    private void DropPowerup()
    {

        hasRolledPowerupDrop = true;
        if (Random.Range(0, 101) > PowerupDropProbability)
        {
            return;
        }
        GameObject GO = Instantiate(powerup, transform.position, Quaternion.identity);
        GO.GetComponent<NetworkObject>().Spawn(true);
        GO.GetComponent<PowerUpController>().SetTeam(color);

    }

    public void ExplosionHit(TeamColor color, PlayerController explosionCreatedBy)
    {
        this.color = color;
        explosionCreatedBy.UpdateScore(3);
        HasBeenHitClientRpc();
        if (!hasRolledPowerupDrop) DropPowerup();
        GetComponent<NetworkObject>().Despawn();

    }
}
