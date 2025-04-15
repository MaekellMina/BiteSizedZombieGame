using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFill : MonoBehaviour
{
    [SerializeField]
    private Image fill;

    public void SetFillAmount(float fillAmount)
    {
        fill.fillAmount = fillAmount;
    }
}
