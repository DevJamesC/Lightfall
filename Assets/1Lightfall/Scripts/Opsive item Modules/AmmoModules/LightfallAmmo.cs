using MBS.ModifierSystem;
using Opsive.UltimateCharacterController.Items.Actions;
using Opsive.UltimateCharacterController.Items.Actions.Modules.Shootable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MBS.StatsAndTags;

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
            lightfallClip = itemAction.GetFirstActiveModule<LightfallClip>();

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
            m_AmmoCount = Mathf.Clamp(m_AmmoCount + amount, 0, m_MaxAmmo);
            NotifyAmmoChange();
        }

        /// <summary>
        /// Give or take a percent of the weapons max ammo. On a 0 to 1 scale
        /// </summary>
        /// <param name="percent"></param>
        /// <returns>return percent overflow</returns>
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
        /// <returns>return percent overflow</returns>
        public float AdjustAmmoAmountByPercentOfClips(float percent)
        {
            int numOfClipsInReserve = Mathf.RoundToInt(MaxAmmo / lightfallClip.ClipSize);
            int clipsToGive = Mathf.RoundToInt(numOfClipsInReserve * percent);
            int ammoToGive = clipsToGive * lightfallClip.ClipSize;

            float percentAmmoOverflow = 1 - ((GetAmmoRemainingCount() + ammoToGive) / MaxAmmo);
            float percentClipOverflow = numOfClipsInReserve / (percentAmmoOverflow / (lightfallClip.ClipSize / MaxAmmo));

            AdjustAmmoAmount(ammoToGive);

            return percentClipOverflow;
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
