using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class MultiConditionEvent : MonoBehaviour
{
    public List<bool> Switches = new List<bool>();
    [Space]
    public UnityEvent E_Initialize = new UnityEvent();
    [Space]
    public UnityEvent E_OnCompleteSwitch = new UnityEvent();
    [Space]
    public UnityEvent E_OnIncompleteSwitch = new UnityEvent();

    [Space]
    [SerializeField]
    bool IsCompleted = false;

    private void OnEnable()
    {
        for (int i = 0; i < Switches.Count; i++)
        {
            Switches[i] = false;
        }
        E_Initialize?.Invoke();
        IsCompleted = false;
    }
    public void ToggleSwitchOn(int index)
    {
        Switches[index] = true;
        CheckSwitches();
    }

    public void ToggleSwitchOFF(int index)
    {
        Switches[index] = false;
        CheckSwitches();
    }

    void CheckSwitches()
    {
        for (int i = 0; i < Switches.Count; i++)
        {
            if (Switches[i] == false)
            {
                if (IsCompleted)
                    E_OnIncompleteSwitch?.Invoke();
                return ;

            }
        }

        E_OnCompleteSwitch?.Invoke();
        if (!IsCompleted) IsCompleted = true;

    }
}
