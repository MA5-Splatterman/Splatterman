using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ExplosionController : NetworkBehaviour
{
    private NetworkVariable<int> explosionRange = new NetworkVariable<int>(2);

    private NetworkVariable<bool> upBlocked = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> downBlocked = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> leftBlocked = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> rightBlocked = new NetworkVariable<bool>(false);

    [SerializeField] private GameObject explosion;
    private NetworkVariable<TeamColor> team = new NetworkVariable<TeamColor>(TeamColor.NONE);
    private NetworkVariable<Vector2> originPosition = new NetworkVariable<Vector2>(Vector2.zero);

    [SerializeField] private LayerMask layermask;
    [SerializeField] private LayerMask layermaskWithBreakable;

    [SerializeField] private Material red, blue;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite fourWay, verticalWay, horizontalWay, upEnd, downEnd, leftEnd, rightEnd;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        team.OnValueChanged += HandleTeamChanged;
        originPosition.OnValueChanged += HandleOriginPositionChanged;
    }

    public void SpawnExplosion(Vector2 positionToSpawn, int _explosionRange, TeamColor _team)
    {
        originPosition.Value = positionToSpawn;
        if (IsServer)
        {
            explosionRange.Value = _explosionRange;
            team.Value = _team;
            DetermineGameStateChangesServerRpc();
        }
    }

    [ServerRpc]
    private void DetermineGameStateChangesServerRpc()
    {
        Debug.Log("Determining Game State Changes");
        upBlocked.Value = IsDirectionBlocked(Vector2.up);
        downBlocked.Value = IsDirectionBlocked(Vector2.down);
        leftBlocked.Value = IsDirectionBlocked(Vector2.left);
        rightBlocked.Value = IsDirectionBlocked(Vector2.right);

        UpdateExplosionVisualsClientRpc();
    }

    private void HandleOriginPositionChanged(Vector2 oldValue, Vector2 newValue)
    {
        transform.position = originPosition.Value;
    }

    private void HandleTeamChanged(TeamColor oldValue, TeamColor newValue)
    {
        ChangeColor();
    }

    [ClientRpc]
    private void UpdateExplosionVisualsClientRpc()
    {
        Debug.Log("Updating Visuals for Client");
        CreateExplosionEffects();
    }

    private bool IsDirectionBlocked(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1, layermask);
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
        Debug.Log("Creating Line");
        if (!IsServer || IsHost)
        {
            CreateExplosionVisuals(direction, line, end);
        }
        if (IsServer)
        {
            ProcessExplosionImpactServerRpc(direction);
        }
    }

    [ServerRpc]
    private void ProcessExplosionImpactServerRpc(Vector2 direction)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, explosionRange.Value);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                if (hit.transform.CompareTag("Wall")) break;
                if (hit.transform.CompareTag("Breakable"))
                {
                    hit.transform.GetComponent<IExplodable>().ExplosionHit(team.Value);
                    break;
                }
                if (hit.transform.CompareTag("Player") || hit.transform.CompareTag("Bomb") || hit.transform.CompareTag("Paintable"))
                {
                    Debug.Log("Explosion hit: " + hit.transform.tag);
                    hit.transform.GetComponent<IExplodable>().ExplosionHit(team.Value);
                }
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
            if (_explosion.TryGetComponent<NetworkObject>(out NetworkObject netObj))
            {
                netObj.Spawn();
            }

            SpriteRenderer renderer = _explosion.GetComponent<SpriteRenderer>();
            renderer.sprite = (i == explosionLength - 1) ? end : line;
            renderer.material = spriteRenderer.material;
            _explosion.transform.position = transform.position + new Vector3(direction.x * (i + 1), direction.y * (i + 1), 0);
            Destroy(_explosion, 2f);
        }
        Destroy(gameObject, 2f);
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
