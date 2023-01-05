
namespace MBS.AbilitySystem
{
    public class AbilityModifierFormulas
    {
        /// <summary>
        /// Used for most things, with the exception of cooldowns/recharge speed, and percent/ hard values.
        /// </summary>
        /// <param name="baseValue"></param>
        /// <param name="totalPercent"></param>
        /// <returns></returns>
        public static float NormalModifier(float baseValue, float totalPercent)
        {
            return baseValue * totalPercent;
        }

        /// <summary>
        /// Used for calculating recharge speed, which is in "uses per second".
        /// </summary>
        /// <param name="baseValue"></param>
        /// <param name="totalPercent"></param>
        /// <returns></returns>
        public static float RechargeSpeedModifier(float baseValue, float totalPercent)
        {
            totalPercent = totalPercent - 1;//Used for adjusting for modifiers. (modifiers have a base of 1, since normal values use it as a multiplier)
            return baseValue * (1 / (1 + totalPercent));
        }

        /// <summary>
        /// Used for calculating percent upgrades, such as "Weapon Damage". for example, we want (1.15 +1.05=1.2) or +20% weapon damage, not (1.15*1.05=1.2075) or +20.75% weapon damage.
        /// </summary>
        /// <param name="baseValue"></param>
        /// <param name="totalPercent"></param>
        /// <returns></returns>
        public static float HardValueModifier(float baseValue, float totalPercent)
        {
            //totalPercent = totalPercent - 1;//Used for adjusting for modifiers. (modifiers have a base of 1, since normal values use it as a multiplier)//Adjustment not needed here
            return baseValue + totalPercent;
        }
    }
}







