using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Leaderboards;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;

public class PlayerController : NetworkBehaviour, IExplodable
{
    [Header("Stats")]
    [SerializeField] NetworkVariable<int> moveSpeed = new NetworkVariable<int>();
    [SerializeField] NetworkVariable<int> bombAmount = new NetworkVariable<int>();
    [SerializeField] NetworkVariable<int> bombRange = new NetworkVariable<int>();
    [SerializeField] public NetworkVariable<bool> canKickBombs = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] NetworkVariable<bool> bombPierce = new NetworkVariable<bool>();

    public NetworkVariable<TeamColor> team = new NetworkVariable<TeamColor>();

    [SerializeField] private NetworkVariable<double> playerScore = new NetworkVariable<double>();

    [Header("Project Variables")]
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private Animator anim;
    [SerializeField] private Rigidbody2D rb2d;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Material red, blue;
    [SerializeField] private SpriteRenderer suit;

    [SerializeField] private Behaviour[] scriptsToEnable;

    // Other variables
    private PlayerControls input;
    public NetworkVariable<Vector2> movementVector = new NetworkVariable<Vector2>(default(Vector2), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> vertical = new NetworkVariable<float>(default(float), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> horizontal = new NetworkVariable<float>(default(float), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    //private Vector2 movementVector = Vector2.zero;
    public static int PlayerCount = 0;
    public static HashSet<PlayerController> players = new HashSet<PlayerController>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        input = new PlayerControls();
        if (IsOwner)
        {
            playerCamera.SetActive(true);
            playerCamera.GetComponent<Camera>().depth = 1;
            //Debug.Log("Controls Enabled");
            input.Enable();
            input.Player.Movement.performed += OnMovementPerformed;
            input.Player.Movement.canceled += OnMovementCancelled;
            input.Player.DropBomb.performed += OnDropBombPerformed;
            isDead.OnValueChanged += (previousValue, newValue) =>
            {
                playerCamera.SetActive(!newValue);
                if (newValue)
                {
                    // if dead
                    input.Disable();
                }
                else
                {
                    // if alive
                    input.Enable();
                }
            };
            team.OnValueChanged += (previousValue, newValue) =>
            {
                UpdateTeamColor();
            };
        }



        if (IsServer)
        {
            PlayerCount++;
            players.Add(this);
            team.Value = PlayerCount % 2 == 0 ? TeamColor.BLUE : TeamColor.RED;
            transform.position = SpawnController.GetSpawnLocation(PlayerCount);
            isDead.OnValueChanged += (previousValue, newValue) =>
            {
                foreach (var script in scriptsToEnable)
                {
                    script.enabled = !newValue;
                }
                GameManager.instance.RecalculateGameState();
            };
            GameManager.instance?.RecalculateGameState();
            AssignTeam(team.Value);
        }
        UpdateTeamColor();
    }

    private void Start()
    {
        GameManager.instance.OnGameEnd += UploadScore;

    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            Debug.Log("Despawning player : " + OwnerClientId);
            PlayerCount--;
            players.Remove(this);

            if (GameManager.instance != default)
            {
                GameManager.instance.RecalculateGameState();
            }
        }
    }

    private void OnDisable()
    {
        if (IsOwner)
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
        if (IsServer)
        {
            anim.SetFloat("vertical", movementVector.Value.y);
            anim.SetFloat("horizontal", movementVector.Value.x);
        }

        anim.SetBool("isDead", isDead.Value);
        if (IsOwner && IsClient)
        {
            anim.SetFloat("vertical", movementVector.Value.y);
            anim.SetFloat("horizontal", movementVector.Value.x);
        }
    }

    private void OnMovementPerformed(InputAction.CallbackContext context)
    {
        movementVector.Value = context.ReadValue<Vector2>();
    }
    private void OnMovementCancelled(InputAction.CallbackContext context)
    {
        movementVector.Value = Vector2.zero;
    }

    private void MovePlayer(Vector2 direction)
    {
        rb2d.velocity = direction * (moveSpeed.Value / 2);
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
        if(GameManager.instance.gameIsActive.Value == false)
        {
            return;
        }
        // Posibly check if there is already a bomb in this location
        if(Physics2D.OverlapCircle(transform.position, 0.5f, LayerMask.GetMask("Bomb")) != null)
        {
            return;
        }
        Vector3 position = transform.position;
        GameObject bomb = Instantiate(bombPrefab, position, Quaternion.identity);
        bomb.GetComponent<NetworkObject>().Spawn(true);
        BombController bombController = bomb.GetComponent<BombController>();
        bombController.BombPlaced(this, team.Value, position, bombRange.Value);

    }
    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public void ExplosionHit(TeamColor color, PlayerController explosionCreatedBy)
    {
        isDead.Value = true;
        if (color == team.Value)
        {
            explosionCreatedBy.UpdateScore(-25);
        }
        else
        {
            explosionCreatedBy.UpdateScore(50);
        }
    }

    public void UpdateStats(powerUp stat, int amountToChangeBy)
    {
        switch (stat)
        {
            case powerUp.Speed:
                moveSpeed.Value += amountToChangeBy;
                break;

            case powerUp.BombPower:
                if (bombRange.Value == 1 && amountToChangeBy == -1) return;
                bombRange.Value += amountToChangeBy;
                break;

            case powerUp.BombAmount:
                if (bombAmount.Value == 1 && amountToChangeBy == -1)
                {
                    return;
                }
                bombAmount.Value += amountToChangeBy;
                break;

            case powerUp.Kick:
                canKickBombs.Value = true;
                break;

            case powerUp.Pierce:
                bombPierce.Value = true;
                break;
        }
    }

    public void UpdateScore(double ValueToUpdateBy)
    {
        playerScore.Value += ValueToUpdateBy;
    }

    private async void UploadScore(TeamColor color)
    {
        if (IsLocalPlayer)
        {
            var playerEntry = await LeaderboardsService.Instance.AddPlayerScoreAsync("Splatterman_Leaderboard", playerScore.Value);
        }
    }
}