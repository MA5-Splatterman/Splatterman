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


    public void HasBeenHit()
    {
        StartCoroutine(OnBreak());
    }

    IEnumerator OnBreak()
    {
        particles.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.enabled = false;
        DropPowerup();
        yield return new WaitForSeconds(0.5f);
        var smokeEmission = smokeParticle.emission;
        var brickEmission = brickParticle.emission;
        smokeEmission.enabled = false;
        brickEmission.enabled = false;
    }

    private void DropPowerup()
    {

    }

    public void ExplosionHit()
    {
        HasBeenHit();
    }
}
