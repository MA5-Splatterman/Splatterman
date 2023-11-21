using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class ExplosionController : NetworkBehaviour
{

    private int explosionRange = 1;

    private bool upBlocked = false;
    private bool downBlocked = false;
    private bool leftBlocked = false;
    private bool rightBlocked = false;

    [SerializeField] private GameObject explosion;
    private TeamColor team;

    [SerializeField] private LayerMask layermask;
    [SerializeField] private LayerMask layermaskWithBreakable;

    [SerializeField] private Material red, blue;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite fourWay, verticalWay, horizontalWay, upEnd, downEnd, leftEnd, rightEnd;


    public void SpawnExplosion(Vector2 positionToSpawn, int _explosionRange, TeamColor _team)
    {
        transform.position = positionToSpawn;
        explosionRange = _explosionRange;
        team = _team;
        ChangeColor();
        CheckCardinalDirections();
        if (!upBlocked)
        {
            CreateExplosionLine(verticalWay, upEnd, Vector2.up);
        }
        if (!downBlocked)
        {
            CreateExplosionLine(verticalWay, downEnd, Vector2.down);
        }
        if (!leftBlocked)
        {
            CreateExplosionLine(horizontalWay, leftEnd, Vector2.left);
        }
        if(!rightBlocked)
        {
            CreateExplosionLine(horizontalWay, rightEnd, Vector2.right);
        }
    }

    private void CheckCardinalDirections()
    {
        upBlocked = IsDirectionBlocked(Vector2.up, 1);
        downBlocked = IsDirectionBlocked(Vector2.down, 1);
        leftBlocked = IsDirectionBlocked(Vector2.left, 1);
        rightBlocked = IsDirectionBlocked(Vector2.right, 1);
    }

    private bool IsDirectionBlocked(Vector2 direction, int range)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, range, layermask);
        if (hit)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void ChangeColor()
    {
        switch (team)
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

    private void CreateExplosionLine(Sprite line, Sprite end, Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, explosionRange, layermaskWithBreakable);
        Sprite[] explosionSprites; 
        if (hit)
        {
            float distance = hit.distance;
            explosionSprites = new Sprite[(int)Mathf.Round(distance - 0.5f)];
            if (hit.transform.CompareTag("Breakable"))
            {
                hit.transform.GetComponent<IExplodable>().ExplosionHit();
                explosionSprites = new Sprite[(int)Mathf.Round(distance - 0.5f) + 1];
            }
        }
        else
        {
            explosionSprites = new Sprite[explosionRange];
        }
        hit = Physics2D.Raycast(transform.position, direction, explosionRange);
        if (hit)
        {
            if (hit.transform.CompareTag("Player") || hit.transform.CompareTag("Bomb"))
            {
                hit.transform.GetComponent<IExplodable>().ExplosionHit();
            }
        }
        for (int i = 0; i < explosionSprites.Length; i++)
        {
            explosionSprites[i] = line;
            if (i == explosionSprites.Length-1)
            {
                explosionSprites[i] = end;
            }
            GameObject _explosion = Instantiate(explosion);
            SpriteRenderer renderer = _explosion.GetComponent<SpriteRenderer>();
            renderer.sprite = explosionSprites[i];
            renderer.material = spriteRenderer.material;
            _explosion.transform.position = transform.position + new Vector3(direction.x * (i+1), direction.y * (i+1), 0);
        }
    }
}
