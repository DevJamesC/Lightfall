using MBS.Lightfall;
using Opsive.Shared.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.AbilitySystem
{
    public class SetScreenOverlayFX : AbilityFXBase
    {
        [SerializeField]
        private Overlay overlay;
        public override void Activate(AbilityWrapperBase wrapper)
        {
            EventHandler.ExecuteEvent(wrapper.gameObject, "StartOverlay", overlay);
        }

        public override void Deactivate(AbilityWrapperBase wrapper)
        {
            EventHandler.ExecuteEvent(wrapper.gameObject, "StopOverlay", overlay, false);

        }
    }
}