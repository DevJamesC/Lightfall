using MBS.StatsAndTags;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MBS.AbilitySystem
{
    public class StatProgressBarUI : MonoBehaviour
    {
        [SerializeField]
        private Color EmptyColor = Color.gray;
        [SerializeField]
        private Color FillColor = Color.blue;
        [SerializeField]
        private Color ProspectiveColor = Color.green;
        [SerializeField]
        private Image BackgroundImage;
        [SerializeField]
        private Slider CurrentValueSlider;
        [SerializeField]
        private Image CurrentValueSliderImage;
        [SerializeField]
        private Slider ProspectiveValueSlider;
        [SerializeField]
        private Image ProspectiveValueSliderImage;
        [SerializeField]
        private TextMeshProUGUI TitleText;
        [SerializeField]
        private TextMeshProUGUI DisplayValueText;

        public void Initalize(AbilityUIStat stat)
        {

            TitleText.text = stat.StatNameDisplayName == "" ? stat.StatName.ToString() : stat.StatNameDisplayName;
            DisplayValueText.text = (Mathf.Round(stat.CurrentValue * 100) / 100).ToString() + stat.statValueDisplaySuffix;


            CurrentValueSlider.value = stat.CurrentValue / stat.MaxValue;
            if (stat.StatName == StatName.AbilityRecharge)
                CurrentValueSlider.value = stat.MaxValue / stat.CurrentValue;

            if (stat.ProspectiveValue != stat.CurrentValue)
            {
                ProspectiveValueSlider.value = stat.ProspectiveValue / stat.MaxValue;
                if (stat.StatName == StatName.AbilityRecharge)
                    ProspectiveValueSlider.value = stat.MaxValue / stat.ProspectiveValue;

                DisplayValueText.text = (Mathf.Round(stat.ProspectiveValue * 100) / 100).ToString() + stat.statValueDisplaySuffix;
                DisplayValueText.outlineColor = ProspectiveColor;
                DisplayValueText.outlineWidth = .15f;
                ProspectiveValueSlider.gameObject.SetActive(true);
            }
            else
            {
                ProspectiveValueSlider.gameObject.SetActive(false);
            }
        }

        private void OnValidate()
        {
            BackgroundImage.color = EmptyColor;
            CurrentValueSliderImage.color = FillColor;
            ProspectiveValueSliderImage.color = ProspectiveColor;
        }
    }
}
