using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour, IExplodable
{
    [Header("Stats")]
    [SerializeField] private int moveSpeed;
    [SerializeField] private int bombAmount;
    [SerializeField] private int bombRange;
    [SerializeField] private bool canKickBombs;
    [SerializeField] private bool bombsPierce;

    private NetworkVariable<TeamColor> team = new NetworkVariable<TeamColor>();

    [Header("Project Variables")]
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private Animator anim;
    [SerializeField] private Rigidbody2D rb2d;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Material red, blue;
    [SerializeField] private SpriteRenderer suit;

    // Other variables
    private PlayerControls input;
    private Vector2 movementVector = Vector2.zero;
    public Vector2 actualVelocity;
    //private Vector2 movementVector = Vector2.zero;

    public override void OnNetworkSpawn()
    {
        input = new PlayerControls();
        if (IsOwner && (IsClient || IsHost))
        {
            playerCamera.SetActive(true);
            Debug.Log("Controls Enabled");
            input.Enable();
            input.Player.Movement.performed += OnMovementPerformed;
            input.Player.Movement.canceled += OnMovementCancelled;
            input.Player.DropBomb.performed += OnDropBombPerformed;
        }
        AssignTeam(TeamColor.BLUE);
    }

    private void OnDisable()
    {
        if (IsOwner && IsClient)
        {
            input.Disable();
            input.Player.Movement.performed -= OnMovementPerformed;
            input.Player.Movement.canceled -= OnMovementCancelled;
            input.Player.DropBomb.performed -= OnDropBombPerformed;
        }
    }

    public void AssignTeam(TeamColor color)
    {
        team.Value = color;
        UpdateTeamColor();
    }

    private void UpdateTeamColor()
    {
        switch (team.Value)
        {
            case TeamColor.RED:
                suit.material = red;
                break;

            case TeamColor.BLUE:
                suit.material= blue;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner && IsClient)
        {
            MovePlayer(movementVector);
        }
    }

    private void OnMovementPerformed(InputAction.CallbackContext context)
    {
        movementVector = context.ReadValue<Vector2>();
        DisplayMoveAnimationServerRpc();
    }

    [ServerRpc]
    private void DisplayMoveAnimationServerRpc()
    {
        if (IsOwner)
        {
            anim.SetFloat("vertical", movementVector.y);
            anim.SetFloat("horizontal", movementVector.x);
        }
    }

    private void OnMovementCancelled(InputAction.CallbackContext context)
    {
        movementVector = Vector2.zero;
    }

    private void MovePlayer(Vector2 direction)
    {
        rb2d.velocity = direction * moveSpeed;
    }

    private void OnDropBombPerformed(InputAction.CallbackContext context)
    {
        if ((IsOwner & IsClient))
        {
            DropBombServerRpc((Vector2)transform.position);
        }
    }

    [ServerRpc]
    private void DropBombServerRpc(Vector2 position)
    {
        GameObject bomb = Instantiate(bombPrefab, position, Quaternion.identity);
        bomb.GetComponent<NetworkObject>().Spawn();
        BombController bombController = bomb.GetComponent<BombController>();
        bombController.BombPlaced(team.Value, position);
        
    }

    public void ExplosionHit(TeamColor color)
    {
        Debug.Log("Ouch! Player was hit!");
    }
}