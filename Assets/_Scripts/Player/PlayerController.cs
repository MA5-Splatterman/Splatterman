using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [Header("Stats")]
    [SerializeField] private int moveSpeed;
    [SerializeField] private int bombAmount;
    [SerializeField] private int bombRange;
    [SerializeField] private bool canKickBombs;
    [SerializeField] private bool bombsPierce;
    [SerializeField] private TeamColor team;

    [Header("Project Variables")]
    [SerializeField] private Animator anim;
    [SerializeField] private Rigidbody2D rb2d;
    [SerializeField] private GameObject bomb;
    [SerializeField] private Material red, blue;
    [SerializeField] private SpriteRenderer suit;


    // Other variables
    private PlayerControls input;
    private Vector2 movementVector = Vector2.zero;

    private void Awake()
    {
        input = new PlayerControls();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Movement.performed += OnMovementPerformed;
        input.Player.Movement.canceled += OnMovementCancelled;
        input.Player.DropBomb.performed += DropBomb;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Player.Movement.performed -= OnMovementPerformed;
        input.Player.Movement.canceled -= OnMovementCancelled;
        input.Player.DropBomb.performed -= DropBomb;
    }

    public void AssignTeam(TeamColor color)
    {
        switch (color)
        {
            case TeamColor.RED:
                suit.color = Color.red;
                team = TeamColor.RED;
                break;

            case TeamColor.BLUE:
                suit.color = Color.blue;
                team = TeamColor.BLUE;
                break;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer(movementVector);
        
    }

    private void OnMovementPerformed(InputAction.CallbackContext context)
    {
        movementVector = context.ReadValue<Vector2>();
        anim.SetFloat("vertical", movementVector.y);
        anim.SetFloat("horizontal", movementVector.x);
    }

    private void OnMovementCancelled(InputAction.CallbackContext context)
    {
        movementVector = Vector2.zero;
    }

    private void MovePlayer(Vector2 direction)
    {
        rb2d.velocity = (Vector3)direction * moveSpeed;
    }

    private void DropBomb(InputAction.CallbackContext context)
    {
        GameObject GO = Instantiate(bomb);
        BombController bombController =  GO.GetComponent<BombController>();
        bombController.BombPlaced(team, (Vector2)transform.position);
    }
}
