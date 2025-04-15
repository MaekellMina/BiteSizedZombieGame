using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private UIFill playerStaminaUI;

    [Header("Player components")]
    [SerializeField]
    private Stamina playerStamina;


    private void Awake()
    {
        playerStamina.e_OnStaminaPrctUpdated.AddListener(UpdatePlayerStaminaUI);
    }

    private void OnDestroy()
    {
        playerStamina.e_OnStaminaPrctUpdated.RemoveListener(UpdatePlayerStaminaUI);
    }

    private void UpdatePlayerStaminaUI(float playerStaminaPrct)
    {
        playerStaminaUI.SetFillAmount(playerStaminaPrct);
    }
}
