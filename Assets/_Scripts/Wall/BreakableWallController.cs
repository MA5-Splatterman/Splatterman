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
        DropPowerup();
        Destroy(gameObject, 0.6f);
        yield return new WaitForSeconds(0.5f);
        var smokeEmission = smokeParticle.emission;
        var brickEmission = brickParticle.emission;
        smokeEmission.enabled = false;
        brickEmission.enabled = false;
    }

    private void DropPowerup()
    {

    }

    public void ExplosionHit(TeamColor color)
    {
        HasBeenHitClientRpc();
    }
}
