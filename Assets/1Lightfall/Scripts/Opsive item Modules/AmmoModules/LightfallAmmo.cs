using MBS.ModifierSystem;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Items.Actions.Modules.Shootable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MBS.StatsAndTags;
using System.Linq;
using Opsive.UltimateCharacterController.Items.Actions.Modules;

namespace MBS.Lightfall
{
    public class LightfallAmmo : ShootableAmmoModule
    {
        [Range(0f, 1f)]
        public float StartingAmmoPercent;
        public int MaxAmmo;
        protected int m_AmmoCount;
        protected int m_MaxAmmo;
        protected DynamicStatModifier weaponCapacityModifier;
        protected LightfallClip lightfallClip;

        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);

            weaponCapacityModifier = Character.gameObject.GetComponent<ModifierHandler>().GetStatModifier(StatName.WeaponCapacity);
            weaponCapacityModifier.OnValueChanged += RecalculateCurrentAndMaxAmmo;
            lightfallClip = null;
            List<ActionModuleGroupBase> modules = new List<ActionModuleGroupBase>();
            itemAction.GetAllModuleGroups(modules);

            foreach (var mod in modules)
            {
                List<LightfallClip> castTest = new List<LightfallClip>();
                mod.GetModulesWithType(castTest);
                if (castTest.Count > 0)
                {
                    lightfallClip = castTest[0];
                    break;
                }
            }


            m_MaxAmmo = Mathf.FloorToInt(MaxAmmo * weaponCapacityModifier.Value);
            AdjustAmmoAmountByPercent(StartingAmmoPercent);
        }

        private void RecalculateCurrentAndMaxAmmo(float newValue)
        {
            float currentAmmoFloat = m_AmmoCount;
            float maxAmmoFloat = m_MaxAmmo;

            float percentMaxAmmo = currentAmmoFloat / maxAmmoFloat;

            //Adjust max ammo 
            m_MaxAmmo = Mathf.FloorToInt(MaxAmmo * newValue);
            maxAmmoFloat = m_MaxAmmo;

            //Adjust ammo equal to the capacity gained or lost. (ex: previous ammo was 20/100. New ammo would be 20/150, so we make it 30/150, or vice versa)
            //May nix these lines if reserve ammo changing ends up being confusing, unbalanced or unfun.
            float newPercentMaxAmmo = currentAmmoFloat / maxAmmoFloat;
            AdjustAmmoAmountByPercent(percentMaxAmmo - newPercentMaxAmmo);
        }

        public override void OnDestroy()
        {
            weaponCapacityModifier.OnValueChanged -= RecalculateCurrentAndMaxAmmo;
            base.OnDestroy();
        }

        public override void AdjustAmmoAmount(int amount)
        {

            m_AmmoCount = Mathf.Clamp(m_AmmoCount + amount, 0, m_MaxAmmo + (lightfallClip.ClipSize - lightfallClip.ClipRemainingCount));
            NotifyAmmoChange();
        }

        /// <summary>
        /// Give or take a percent of the weapons max ammo. On a 0 to 1 scale
        /// </summary>
        /// <param name="percent"></param>
        /// <returns>return percent used</returns>
        public float AdjustAmmoAmountByPercent(float percent)
        {
            int ammoToGive = Mathf.RoundToInt(m_MaxAmmo * percent);
            float returnVal = 1 - ((GetAmmoRemainingCount() + ammoToGive) / MaxAmmo);
            AdjustAmmoAmount(ammoToGive);

            return returnVal;
        }

        /// <summary>
        /// Give or take a percent of the weapons max ammo. On a 0 to 1 scale
        /// </summary>
        /// <param name="percent"></param>
        /// <returns>return percent used</returns>
        public float AdjustAmmoAmountByClipIncriment(float percent)
        {
            //give ammo, floored by clip amount.
            if (lightfallClip == null)
                lightfallClip = CharacterItemAction.GetFirstActiveModule<LightfallClip>();

            int ammoToGive = Mathf.RoundToInt(m_MaxAmmo * percent);
            int numberOfClips = Mathf.FloorToInt((float)ammoToGive / lightfallClip.ClipSize);
            ammoToGive = numberOfClips * lightfallClip.ClipSize;

            //return percent of percent actually consumed
            int ammoOverflow = (GetAmmoRemainingCount() + ammoToGive) - MaxAmmo;
            float returnVal = ammoToGive > 0 ? 1 : 0;

            if (ammoOverflow > 0)
            {
                //first, if we have overflow, but our clip is not full, give extra ammo for the missing percent of clip.
                int extraAmmoNeeded = (lightfallClip.ClipSize - lightfallClip.ClipRemainingCount);
                int extraAmmoToGive = ammoOverflow > extraAmmoNeeded ? extraAmmoNeeded : ammoOverflow;
                ammoOverflow -= extraAmmoToGive;
                ammoToGive += extraAmmoToGive;

                //then, determin what percent of the ammoToGive we actually used
                float clipOverflow = (float)ammoOverflow / lightfallClip.ClipSize;
                returnVal = 1 - (clipOverflow / numberOfClips);
            }

            AdjustAmmoAmount(ammoToGive);

            return returnVal;
        }

        public override bool DoesAmmoSharedMatch(ShootableAmmoModule otherAmmoModule)
        {
            throw new System.NotImplementedException();
        }

        public override int GetAmmoRemainingCount()
        {
            return m_AmmoCount;
        }

        public override bool HasAmmoRemaining()
        {
            return m_AmmoCount > 0;
        }

        public override bool IsAmmoShared()
        {
            return false;
        }

        public override void LoadAmmoIntoList(List<ShootableAmmoData> clipRemaining, int reloadAmount, bool removeAmmoWhenLoaded)
        {
            if (reloadAmount <= 0) { return; }

            for (int i = 0; i < reloadAmount; i++)
            {
                var ammoData = CreateAmmoData();
                clipRemaining.Add(ammoData);
            }

            if (removeAmmoWhenLoaded)
            {
                AdjustAmmoAmount(-reloadAmount);
                NotifyAmmoChange();
            }
        }

        public override ShootableAmmoData CreateAmmoData()
        {
            return new ShootableAmmoData(this, -1, -1, null, null);
        }
    }
}
