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
        private Color ProspectiveNegativeColor = Color.red;
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
        private Slider ProspectiveNegativeValueSlider;
        [SerializeField]
        private Image ProspectiveNegativeValueSliderImage;
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
                InitalizePospectiveValue(stat);
            }
            else
            {
                ProspectiveValueSlider.gameObject.SetActive(false);
                ProspectiveNegativeValueSlider.gameObject.SetActive(false);
                ProspectiveNegativeValueSliderImage.gameObject.SetActive(false);
            }
        }

        private void InitalizePospectiveValue(AbilityUIStat stat)
        {
            bool isPositive = stat.ProspectiveValue >= stat.CurrentValue;
            if (stat.StatName == StatName.AbilityRecharge) isPositive = !isPositive;

            Slider slider = isPositive ? ProspectiveValueSlider : ProspectiveNegativeValueSlider;

            slider.value = stat.ProspectiveValue / stat.MaxValue;
            if (stat.StatName == StatName.AbilityRecharge)
                slider.value = stat.MaxValue / stat.ProspectiveValue;

            if (!isPositive)
            {
                ProspectiveValueSlider.gameObject.SetActive(false);
                //Debug.Log("currently takes PERCENT CHANGE off of the blue line, rather than off the total line. (It give the illusion of working if the blue line has not been changed via upgrade...)");
                slider.value = 1 - slider.value;
            }
            else
                ProspectiveNegativeValueSliderImage.gameObject.SetActive(false);

            DisplayValueText.text = (Mathf.Round(stat.ProspectiveValue * 100) / 100).ToString() + stat.statValueDisplaySuffix;
            DisplayValueText.outlineColor = ProspectiveColor;
            DisplayValueText.outlineWidth = .15f;
            slider.gameObject.SetActive(true);

        }

        private void OnValidate()
        {
            BackgroundImage.color = EmptyColor;
            CurrentValueSliderImage.color = FillColor;
            ProspectiveValueSliderImage.color = ProspectiveColor;
            ProspectiveNegativeValueSliderImage.color = ProspectiveNegativeColor;
        }
    }
}
