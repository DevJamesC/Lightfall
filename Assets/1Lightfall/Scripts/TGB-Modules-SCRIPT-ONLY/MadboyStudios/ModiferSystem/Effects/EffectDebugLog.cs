using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    public class EffectDebugLog : ModifierEffectBase
    {
        [SerializeField]
        private string effectActivatedText;
        [SerializeField]
        private string effectUpdateText;
        [SerializeField]
        private string effectRemovedText;
        public override void EffectActivated(ModifierEntry target)
        {
            base.EffectActivated(target);
            if (effectActivatedText != "")
                Debug.Log(effectActivatedText);
        }

        public override void EffectRemoved(ModifierEntry target)
        {
            base.EffectRemoved(target);
            if (effectRemovedText != "")
                Debug.Log(effectRemovedText);
        }

        public override void EffectUpdate(ModifierEntry target)
        {
            base.EffectUpdate(target);
            if (effectUpdateText != "")
                Debug.Log(effectUpdateText);
        }
    }
}
