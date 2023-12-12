using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public enum powerUp
{
    Speed,
    BombPower,
    BombAmount,
    Kick,
    Pierce
}

public class PowerUpController : NetworkBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite positive;
    [SerializeField] private Sprite negative;
    [SerializeField] private SpriteRenderer iconSR, modifierSR, background;
    [SerializeField] private Material red, blue, neutral;

    [Header("Misc")]
    NetworkVariable<TeamColor> team = new NetworkVariable<TeamColor>();
    [SerializeField] NetworkVariable<powerUp> powerUpAbility = new NetworkVariable<powerUp>();
    [SerializeField] NetworkVariable<int> modifier = new NetworkVariable<int>();
    [SerializeField] NetworkVariable<bool> overridePowerup = new NetworkVariable<bool>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!overridePowerup.Value)
        {

            SetPowerup((powerUp)Random.Range(0, 5), Random.Range(0, 2) == 1 ? 1 : -1);
        }
        else
        {
            SetPowerup(powerUpAbility.Value, modifier.Value);
        }
    }

    public void SetTeam(TeamColor color)
    {
        team.Value = color;
        UpdateBackgroundClientRPC();
    }

    [ClientRpc]
    private void UpdateBackgroundClientRPC()
    {
        switch (team.Value)
        {
            case TeamColor.NONE:
                background.material = neutral;
                break;

            case TeamColor.BLUE:
                background.material = blue;
                break;

            case TeamColor.RED:
                background.material = red;
                break;
        }
    }

    private void SetPowerup(powerUp _powerUp, int _modifier)
    {
        if (IsServer)
        {
            powerUpAbility.Value = _powerUp;
            modifier.Value = _modifier;
        }
        if (modifier.Value == -1 )
        {
            modifierSR.sprite = negative;
        }
        switch (powerUpAbility.Value)
        {
            default:
                Debug.Log("Powerup not found");
                iconSR.sprite = Resources.Load<Sprite>("Powerups/Questionmark");
                break;

            case powerUp.Speed:
                iconSR.sprite = Resources.Load<Sprite>("Powerups/SpeedPowerup");
                break;

            case powerUp.BombPower:
                iconSR.sprite = Resources.Load<Sprite>("Powerups/BombPowerPowerup");
                break;

            case powerUp.BombAmount:
                iconSR.sprite = Resources.Load<Sprite>("Powerups/BombAmountPowerup");
                break;

            case powerUp.Kick:
                iconSR.sprite = Resources.Load<Sprite>("Powerups/KickPowerup");
                modifierSR.gameObject.SetActive(false);
                break;

            case powerUp.Pierce:
                iconSR.sprite = Resources.Load<Sprite>("Powerups/PiercePowerup");
                modifierSR.gameObject.SetActive(false);
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && (collision.GetComponent<PlayerController>().team.Value == team.Value || team.Value == TeamColor.NONE))
        {
            collision.GetComponent<PlayerController>().UpdateStats(powerUpAbility.Value, modifier.Value);
            Destroy(gameObject);
        }
    }
}
