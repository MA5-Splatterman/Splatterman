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
    [SerializeField] public NetworkVariable<bool> canKickBombs = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private bool bombsPierce;

    public NetworkVariable<TeamColor> team = new NetworkVariable<TeamColor>();

    [Header("Project Variables")]
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private Animator anim;
    [SerializeField] private Rigidbody2D rb2d;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Material red, blue;
    [SerializeField] private SpriteRenderer suit;

    // Other variables
    private PlayerControls input;
    public NetworkVariable<Vector2> movementVector = new NetworkVariable<Vector2>(default(Vector2), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> vertical = new NetworkVariable<float>(default(float), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> horizontal = new NetworkVariable<float>(default(float), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    //private Vector2 movementVector = Vector2.zero;
    public static int PlayerCount = 0;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        input = new PlayerControls();
        if (IsOwner)
        {
            playerCamera.SetActive(true);
            playerCamera.GetComponent<Camera>().depth = 1;
            Debug.Log("Controls Enabled");
            input.Enable();
            input.Player.Movement.performed += OnMovementPerformed;
            input.Player.Movement.canceled += OnMovementCancelled;
            input.Player.DropBomb.performed += OnDropBombPerformed;
            vertical.OnValueChanged += HandleVerticalChanged;
            horizontal.OnValueChanged += HandleHorizontalChanged;
            team.OnValueChanged += (previousValue, newValue) =>
            {
                UpdateTeamColor();
            };
        }
        if (IsServer)
        {
            PlayerCount++;
            team.Value = PlayerCount % 2 == 0 ? TeamColor.BLUE : TeamColor.RED;
            AssignTeam(team.Value);
        }
        UpdateTeamColor();
    }

    private void OnDisable()
    {
        if (IsOwner)
        {
            input.Disable();
            input.Player.Movement.performed -= OnMovementPerformed;
            input.Player.Movement.canceled -= OnMovementCancelled;
            input.Player.DropBomb.performed -= OnDropBombPerformed;
            vertical.OnValueChanged -= HandleVerticalChanged;
            horizontal.OnValueChanged -= HandleHorizontalChanged;
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
                suit.material = blue;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner && IsClient)
        {
            MovePlayer(movementVector.Value);
        }
    }

    private void OnMovementPerformed(InputAction.CallbackContext context)
    {
        movementVector.Value = context.ReadValue<Vector2>();
        if (IsOwner)
        {
            DisplayMoveAnimationServerRpc();
        }
    }

    [ServerRpc]
    private void DisplayMoveAnimationServerRpc()
    {
        vertical.Value = movementVector.Value.y;
        horizontal.Value = movementVector.Value.x;
        anim.SetFloat("vertical", vertical.Value);
        anim.SetFloat("horizontal", horizontal.Value);
    }

    private void HandleVerticalChanged(float oldValue, float newValue)
    {
        DisplayMoveAnimationServerRpc();
    }

    private void HandleHorizontalChanged(float oldValue, float newValue)
    {
        DisplayMoveAnimationServerRpc();
    }

    private void OnMovementCancelled(InputAction.CallbackContext context)
    {
        movementVector.Value = Vector2.zero;
    }

    private void MovePlayer(Vector2 direction)
    {
        rb2d.velocity = direction * moveSpeed;
    }


    private void OnDropBombPerformed(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            DropBombServerRpc();
        }
    }

    [ServerRpc]
    private void DropBombServerRpc()
    {
        Vector3 position = transform.position;
        GameObject bomb = Instantiate(bombPrefab, position, Quaternion.identity);
        bomb.GetComponent<NetworkObject>().Spawn();
        BombController bombController = bomb.GetComponent<BombController>();
        bombController.BombPlaced(team.Value, position);

    }

    public void ExplosionHit(TeamColor color)
    {
        Debug.Log("Ouch! Player was hit!");
    }


    public void SetSpawnLocation(Vector2 spawnLocation)
    {
        //GetComponent<ClientNetworkTransform>().SyncPositionX = true;
        Debug.Log($"SetSpawnLocationServerRpc: {spawnLocation}");
        transform.position = spawnLocation;
    }
}