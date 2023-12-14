using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableWallController : NetworkBehaviour, IExplodable
{
    [SerializeField] private ParticleSystem smokeParticle, brickParticle;
    [SerializeField] private GameObject particles;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject powerup;
    [SerializeField] private int PowerupDropProbability;
    private bool hasRolledPowerupDrop = false;
    private TeamColor color;

    [ClientRpc]
    public void HasBeenHitClientRpc()
    {
        StartCoroutine(OnBreak());
    }

    IEnumerator OnBreak()
    {
        particles.SetActive(true);
        particles.transform.parent = null;
        Destroy(particles, brickParticle.main.duration > smokeParticle.main.duration ? brickParticle.main.duration : smokeParticle.main.duration);
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.enabled = false;
        if (!hasRolledPowerupDrop)
        {
            if (IsServer)
            {
                DropPowerup();
            }
        }
        if (IsServer)
        {
            Destroy(gameObject,0.6f);
        }
        yield return new WaitForSeconds(0.5f);
        var smokeEmission = smokeParticle.emission;
        var brickEmission = brickParticle.emission;
        smokeEmission.enabled = false;
        brickEmission.enabled = false;
    }

    private void DropPowerup()
    {
        if (IsServer)
        {
            hasRolledPowerupDrop = true;
            if (Random.Range(0, 101) > PowerupDropProbability)
            {
                return;
            }
            GameObject GO = Instantiate(powerup, transform);
            GO.GetComponent<NetworkObject>().Spawn(true);
            GO.GetComponent<PowerUpController>().SetTeam(color);
            GO.transform.parent = null;
        }
    }

    public void ExplosionHit(TeamColor color, PlayerController explosionCreatedBy)
    {
        this.color = color;
        explosionCreatedBy.UpdateScore(3);
        HasBeenHitClientRpc();
    }
}
