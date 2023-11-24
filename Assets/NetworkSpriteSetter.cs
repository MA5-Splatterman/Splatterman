using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class NetworkSpriteSetter : NetworkBehaviour
{
    [SerializeField] Sprite[] sprites;
    [SerializeField] Material redMat;
    [SerializeField] Material blueMat;
    [ClientRpc]
    public void SetSpriteClientRpc(string name, TeamColor color)

    {

        var sprite = sprites.Where(s => s.name == name).First();
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.material = color == TeamColor.RED ? redMat : blueMat;
    }
}
