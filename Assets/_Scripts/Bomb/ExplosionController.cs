using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ExplosionController : NetworkBehaviour
{
    private NetworkVariable<int> explosionRange = new NetworkVariable<int>();

    private NetworkVariable<bool> upBlocked = new NetworkVariable<bool>();
    private NetworkVariable<bool> downBlocked = new NetworkVariable<bool>();
    private NetworkVariable<bool> leftBlocked = new NetworkVariable<bool>();
    private NetworkVariable<bool> rightBlocked = new NetworkVariable<bool>();

    [SerializeField] private GameObject explosion;
    private NetworkVariable<TeamColor> team = new NetworkVariable<TeamColor>();

    [SerializeField] private LayerMask layermask;
    [SerializeField] private LayerMask layermaskWithBreakable;

    [SerializeField] private Material red, blue;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite fourWay, verticalWay, horizontalWay, upEnd, downEnd, leftEnd, rightEnd;

    public void SpawnExplosion(Vector2 positionToSpawn, int _explosionRange, TeamColor _team)
    {
        if (IsServer)
        {
            transform.position = positionToSpawn;
            explosionRange.Value = _explosionRange;
            team.Value = _team;
            DetermineGameStateChangesServerRpc();
        }
    }

    [ServerRpc]
    private void DetermineGameStateChangesServerRpc()
    {
        upBlocked.Value = IsDirectionBlocked(Vector2.up);
        downBlocked.Value = IsDirectionBlocked(Vector2.down);
        leftBlocked.Value = IsDirectionBlocked(Vector2.left);
        rightBlocked.Value = IsDirectionBlocked(Vector2.right);

        UpdateExplosionVisualsClientRpc();
    }

    [ClientRpc]
    private void UpdateExplosionVisualsClientRpc()
    {
        ChangeColor();
        CreateExplosionEffects();
    }

    private bool IsDirectionBlocked(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, explosionRange.Value, layermask);
        return hit.collider != null;
    }

    private void ChangeColor()
    {
        switch (team.Value)
        {
            case TeamColor.RED:
                spriteRenderer.material = red;
                break;

            case TeamColor.BLUE:
                spriteRenderer.material = blue;
                break;

            default:
                Debug.Log("Explosion cannot be assigned to an unknown team");
                break;
        }
    }

    private void CreateExplosionEffects()
    {
        if (!upBlocked.Value)
        {
            CreateExplosionLine(verticalWay, upEnd, Vector2.up);
        }
        if (!downBlocked.Value)
        {
            CreateExplosionLine(verticalWay, downEnd, Vector2.down);
        }
        if (!leftBlocked.Value)
        {
            CreateExplosionLine(horizontalWay, leftEnd, Vector2.left);
        }
        if (!rightBlocked.Value)
        {
            CreateExplosionLine(horizontalWay, rightEnd, Vector2.right);
        }
    }

    private void CreateExplosionLine(Sprite line, Sprite end, Vector2 direction)
    {
        ProcessExplosionImpactServerRpc(direction);
        CreateExplosionVisuals(direction, line, end);
    }

    [ServerRpc]
    private void ProcessExplosionImpactServerRpc(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, explosionRange.Value, layermaskWithBreakable);
        if (hit)
        {
            if (hit.transform.CompareTag("Breakable"))
            {
                hit.transform.GetComponent<IExplodable>().ExplosionHit();
            }
            else if (hit.transform.CompareTag("Player") || hit.transform.CompareTag("Bomb"))
            {
                hit.transform.GetComponent<IExplodable>().ExplosionHit();
            }
        }
    }

    private void CreateExplosionVisuals(Vector2 direction, Sprite line, Sprite end)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, explosionRange.Value, layermaskWithBreakable);
        int explosionLength = CalculateExplosionLength(hit, direction);

        for (int i = 0; i < explosionLength; i++)
        {
            GameObject _explosion = Instantiate(explosion);
            SpriteRenderer renderer = _explosion.GetComponent<SpriteRenderer>();
            renderer.sprite = (i == explosionLength - 1) ? end : line;
            renderer.material = spriteRenderer.material;
            _explosion.transform.position = transform.position + new Vector3(direction.x * (i + 1), direction.y * (i + 1), 0);
            Destroy(_explosion, 1f);
        }
        Destroy(gameObject, 1f);
    }

    private int CalculateExplosionLength(RaycastHit2D hit, Vector2 direction)
    {
        if (hit)
        {
            return (int)Mathf.Round(hit.distance - 0.5f) + (hit.transform.CompareTag("Breakable") ? 1 : 0);
        }
        return explosionRange.Value;
    }
}
