using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoClipDisplay : MonoBehaviour
{
    [SerializeField]
    Renderer Renderer;
    public float FillAmountValue { get { return progressShaderMaterial.GetFloat(fillAmountID); } }
    private Material progressShaderMaterial;

    private int fillAmountID;

    [Button("IncreaseValue")]
    [HorizontalGroup("Split", 0.5f)]
    private void IncrimentValue()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("AmmoClipDisplay: Please enter play mode to preview shader change");
            return;
        }
        SetPercentCeilToTenth(FillAmountValue + .1f);
    }

    [Button("DecreaseValue")]
    [HorizontalGroup("Split", 0.5f)]
    private void DecrimentValue()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("AmmoClipDisplay: Please enter play mode to preview shader change");
            return;
        }

        SetPercentCeilToTenth(FillAmountValue - .1f);
    }

    private void Awake()
    {
        fillAmountID = Shader.PropertyToID("_FillAmount");
        progressShaderMaterial = Renderer.material;
    }

    public void SetPercent(float fillAmount)
    {
        fillAmount = Mathf.Clamp01(fillAmount);
        SetFillShaderValue(fillAmount);
    }

    public void SetPercentCeilToTenth(float fillAmount)
    {
        float roundedValue = CeilToNearestTenth(fillAmount);
        SetPercent(roundedValue);
    }

    private float CeilToNearestTenth(float value)
    {
        value = Mathf.Round(value * 100) / 100;
        return Mathf.Ceil(value * 10) / 10;
    }

    private void SetFillShaderValue(float fillAmount)
    {
        progressShaderMaterial.SetFloat(fillAmountID, fillAmount);
    }
}
