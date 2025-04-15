using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Stamina : MonoBehaviour
{
    public float MaxStamina = 5f;
    public float StaminaThresholdToRun = 0.5f;
    public float RegenRate = 0.5f;
    public float RegenDelay = 1f;

    public float CurrentStamina { get; private set; }
    public bool CanStartRun => CurrentStamina >= StaminaThresholdToRun;

    private float timeSinceLastDrain = 0f;
    private bool isDraining = false;

    private void Awake()
    {
        CurrentStamina = MaxStamina;
    }

    public void DrainStamina(float amount)
    {
        CurrentStamina = Mathf.Max(CurrentStamina - amount, 0f);
        timeSinceLastDrain = 0f;
        isDraining = true;
    }

    public void RegenerateStamina(float amount)
    {
        CurrentStamina = Mathf.Min(CurrentStamina + amount, MaxStamina);
    }

    private void Update()
    {
        if (!isDraining)
            timeSinceLastDrain += Time.deltaTime;

        isDraining = false;

        if (timeSinceLastDrain >= RegenDelay && CurrentStamina < MaxStamina)
        {
            RegenerateStamina(Time.deltaTime * RegenRate);
        }
    }
}