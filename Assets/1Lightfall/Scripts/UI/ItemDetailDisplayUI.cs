using Michsky.MUIP;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MBS.Lightfall
{
    public class ItemDetailDisplayUI : MonoBehaviour
    {
        [SerializeField] protected GameObject statPrefab;
        [SerializeField] protected TextMeshProUGUI titleText;
        [SerializeField] protected Image image;
        [SerializeField] protected TextMeshProUGUI ItemCountText;
        [SerializeField] protected TextMeshProUGUI shortDescriptionText;
        [SerializeField] protected GameObject statsDisplayRoot;
        [SerializeField] protected ButtonManager NextBtn;
        [SerializeField] protected ButtonManager PrevBtn;
        [SerializeField] protected ButtonManager SelectBtn;
        [SerializeField] protected ButtonManager CancelBtn;

        public event Action OnNextPressed = delegate { };
        public event Action OnPreviousPressed = delegate { };
        public event Action OnSelectPressed = delegate { };
        public event Action OnCancelPressed = delegate { };

        private void Start()
        {
            NextBtn.onClick.AddListener(() => OnNextPressed.Invoke());
            PrevBtn.onClick.AddListener(() => OnPreviousPressed.Invoke());
            SelectBtn.onClick.AddListener(() => OnSelectPressed.Invoke());
            CancelBtn.onClick.AddListener(() => OnCancelPressed.Invoke());

        }

        public void InitalizeDetails(ItemDetailsScriptableObject itemDetails, bool nextButtonEnabled, bool previousButtonEnabled)
        {
            titleText.text = itemDetails.ItemName;
            image.sprite = itemDetails.Image;
            shortDescriptionText.text = itemDetails.ItemShortDescription;
            NextBtn.Interactable(nextButtonEnabled);
            PrevBtn.Interactable(previousButtonEnabled);
            foreach (Transform child in statsDisplayRoot.transform)
            {
                Destroy(child.gameObject);
            }

            if (itemDetails.Stats != null)
            {
                foreach (var stat in itemDetails.Stats)
                {
                    GameObject newStat = Instantiate(statPrefab, statsDisplayRoot.transform);
                    newStat.GetComponentInChildren<TextMeshProUGUI>().text = stat.Name;
                    newStat.GetComponentInChildren<SliderManager>().mainSlider.value = stat.StatPercent;
                }
            }
        }

        public void UpdateItemCountText(string text, bool enabled)
        {
            ItemCountText.gameObject.SetActive(enabled);
            ItemCountText.text = text;
        }

    }
}