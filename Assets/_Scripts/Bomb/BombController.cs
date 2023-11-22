using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Loading;

public enum TeamColor
{
    NONE,
    RED,
    BLUE
}

public class BombController : NetworkBehaviour, IExplodable
{
    [SerializeField] Grid grid;

    [SerializeField] private GameObject explosion;

    private NetworkVariable<TeamColor> team = new NetworkVariable<TeamColor>();

    [SerializeField] private Material red, blue;

    [SerializeField] private ParticleSystem bombParticle;
    [SerializeField] private ParticleSystemRenderer bombParticleRenderer;
    [SerializeField] private CircleCollider2D collider2D;

    [SerializeField] private int bombMovementSpeed = 10;
    [SerializeField] private int bombExplosionRange = 2;
    [SerializeField] private float bombFuse = 5;

    public void BombPlaced(TeamColor _team, Vector2 position)
    {
        if (IsServer)
        {
            team.Value = _team;
            UpdateColorServerRpc();
            transform.position = position;
            var particleMain = bombParticle.main;
            particleMain.startLifetime = bombFuse;
            particleMain.stopAction = ParticleSystemStopAction.Callback;
            bombParticle.Emit(1);
            bombParticle.Stop();
            var particleEmission = bombParticle.emission;
            particleEmission.enabled = false;
            SnapToCell();
        }
    }

    private void SnapToCell()
    {
        var x = Mathf.Round(transform.position.x) + 0.5f;
        var y = Mathf.Round(transform.position.y) + 0.5f;
        if (x - transform.position.x > 0.5f)
        {
            x -= 1;
        }
        if (y - transform.position.y > 0.5f)
        {
            y -= 1;
        }
        transform.position = new Vector2(x, y);
    }

    public void BombKicked(TeamColor _team, Vector2 kickOrigin)
    {
        if (IsServer)
        {
            if (team.Value != _team)
            {
                team.Value = _team;
                UpdateColorServerRpc();
            }
            StartCoroutine(BombMoving(CalculateDirection(kickOrigin, transform.position)));
        }
    }

    private Vector2 CalculateDirection(Vector2 origin, Vector2 self)
    {
        return self - origin;
    }

    [ServerRpc]
    private void UpdateColorServerRpc()
    {
        Debug.Log("Setting color");
        switch (team.Value)
        {
            case TeamColor.RED:
                bombParticleRenderer.material = red;
                Debug.Log("Color is Red");
                break;

            case TeamColor.BLUE:
                bombParticleRenderer.material = blue;
                Debug.Log("Color is Blue");
                break;

            default:
                Debug.Log("Team Color not recognized or does not exist");
                break;
        }
    }

    IEnumerator BombMoving(Vector2 direction)
    {
        bool isBlocked = false;

        do
        {
            RaycastHit2D hit2D = Physics2D.Raycast(direction, transform.position, 1);
            if (hit2D)
            {
                isBlocked = true;
            }
            else
            {
                transform.position += (Vector3)direction*Time.deltaTime*bombMovementSpeed;
                yield return new WaitForSeconds(0.01f);
            }
        } while (isBlocked == false);
        SnapToCell();
    }

    private void OnParticleSystemStopped()
    {
        if (IsServer)
        {
            SnapToCell();
            Explode();
        }
    }

    private void Explode()
    {
        if (IsServer)
        {
            collider2D.enabled = false;
			GameObject GO = Instantiate(explosion);
			GO.GetComponent<NetworkObject>().Spawn(  );
			GO.GetComponent<ExplosionController>().SpawnExplosion(transform.position, bombExplosionRange, team.Value);
            Destroy(gameObject);
        }
    }

    public void ExplosionHit(TeamColor color)
    {
        StopAllCoroutines();
        team.Value = color;
        SnapToCell();
        Explode();
    }
}
