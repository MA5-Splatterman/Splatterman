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
    [SerializeField] private Collider2D colliderr;

    [SerializeField] private int bombMovementSpeed = 10;
    [SerializeField] private int bombExplosionRange = 2;
    [SerializeField] private float bombFuse = 5;
    bool shouldExplodeOnEnemyPlayerImpact = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        team.OnValueChanged += HandleTeamColorChanged;
        var particleMain = bombParticle.main;
        spawntime = Time.time;
        particleMain.stopAction = ParticleSystemStopAction.Callback;

    }

    public void BombPlaced(TeamColor _team, Vector2 position)
    {
        if (IsServer)
        {
            team.Value = _team;
            transform.position = new Vector2(position.x, position.y - 0.3f);
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
    private Vector2 SnapPosToCell(Vector2 pos)
    {
        var x = Mathf.Round(pos.x) + 0.5f;
        var y = Mathf.Round(pos.y) + 0.5f;
        if (x - pos.x > 0.5f)
        {
            x -= 1;
        }
        if (y - pos.y > 0.5f)
        {
            y -= 1;
        }
        return new Vector2(x, y);
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
    private Vector2 CalculateDirection(Vector2 origin, Vector2 self)
    {
        return self - origin;
    }

    private void HandleTeamColorChanged(TeamColor oldValue, TeamColor newValue)
    {
        UpdateColor();
    }

    private void UpdateColor()
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
    Vector2 SnapDirTo90Deg(Vector2 angle)
    {
        if (Mathf.Abs(angle.x) > Mathf.Abs(angle.y))
        {
            return new Vector2(angle.x, 0).normalized;
        }
        else
        {
            return new Vector2(0, angle.y).normalized;
        }
    }

    IEnumerator BombMoving(Vector2 direction)
    {
        bool isBlocked = false;

        do
        {

            Vector2 Dir = SnapDirTo90Deg(direction);
            Vector2 Pos = SnapPosToCell(transform.position);
            RaycastHit2D hit2D = Physics2D.Raycast(Pos, Dir, 1, LayerMask.GetMask("Wall", "Breakable Wall", "Bomb", "Player"));
            Debug.DrawRay(Pos, Dir, Color.red, 1);

            if (hit2D)
            {
                isBlocked = true;
                Debug.Log("Bomb Blocked by" + hit2D.collider.gameObject.name + " PSO: " + transform.position);
            }
            else
            {
                Debug.Log("Bomb Moving" + direction + " PSO: " + transform.position);
                transform.position += (Vector3)Dir * Time.deltaTime * bombMovementSpeed;
                yield return null;
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
            colliderr.enabled = false;
            GameObject GO = Instantiate(explosion);
            GO.GetComponent<NetworkObject>().Spawn();
            GO.GetComponent<ExplosionController>().SpawnExplosion(transform.position, bombExplosionRange, team.Value);
            GetComponent<NetworkObject>().Despawn();
        }
    }

    public void ExplosionHit(TeamColor color)
    {
        if (IsServer)
        {
            StopAllCoroutines();
            team.Value = color;
            SnapToCell();
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        AttempKick(other);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsOwnedByServer)
        {
            if (colliderr)
                colliderr.isTrigger = false;
        }
    }

    float spawntime;
    private void AttempKick(Collider2D other)
    {
        if (spawntime + 0.2f > Time.time)
        {
            return;
        }
        if (IsOwnedByServer)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                var player = other.transform.GetComponent<PlayerController>();
                if (player is null) return;

                if (shouldExplodeOnEnemyPlayerImpact && player.team.Value != team.Value)
                {
                    SnapToCell();
                    Explode();
                    return;
                }

                if (player.team.Value != team.Value)
                {
                    team.Value = player.team.Value;
                    UpdateColor();
                }

                if (player.canKickBombs.Value)
                {
                    // prevent pushing bomb if player is not moving
                    if (Vector2.Distance(player.movementVector.Value, Vector2.zero) > 0.05f)
                    {
                        StartCoroutine(BombMoving(player.movementVector.Value));
                    }
                }
            }
        }
    }
}
