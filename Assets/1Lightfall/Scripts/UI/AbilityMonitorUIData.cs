using MBS.Lightfall;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityMonitorUIData : MonoBehaviour
{
    public Slider Slider;
    public TextMeshProUGUI Text;
    public Image Image;
    protected LightfallAbilityBase ability;

    private bool isSetup;

    private void Start()
    {
        isSetup = true;
        if (Slider == null || Text == null || Image == null)
        {
            Debug.Log($"Not all values where set for AbilityMonitorUIData on {gameObject.name}");
            isSetup = false;
        }
    }

    internal void OnAttachAbility(LightfallAbilityBase ability)
    {
        this.ability = ability;
    }

    private void Update()
    {
        if (ability == null || !isSetup)
            return;

        if (ability.AbilityWrapper == null)
            return;

        Slider.value = ability.sharedRechargePercentRemaining;
        //Debug.Log(ability.sharedRechargePercentRemaining);
        Text.text = ability.AbilityWrapper.ChargesRemaining.ToString();

    }

}
