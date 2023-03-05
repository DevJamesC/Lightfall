using MBS.ModifierSystem;
using MBS.StatsAndTags;
using Opsive.UltimateCharacterController.Items.Actions.Modules.Shootable;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MBS.Lightfall
{
    public class MBSReloader : GenericReloader
    {
        private Animator characterAnimator;
        private Animator weaponAnimator;
        private ModifierHandler modifierHandler;
        private int reloadMultiplierID;

        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            characterAnimator = Character.GetComponentInChildren<Animator>();
            //weaponAnimator =
            modifierHandler = characterAnimator.GetComponentInParent<ModifierHandler>();
            reloadMultiplierID = Animator.StringToHash("ReloadSpeedMult");
        }

        public override void StartItemReload()
        {
            characterAnimator.SetFloat(reloadMultiplierID, modifierHandler.GetStatModifierValue(StatName.WeaponReloadSpeed));
            base.StartItemReload();
        }
    }
}