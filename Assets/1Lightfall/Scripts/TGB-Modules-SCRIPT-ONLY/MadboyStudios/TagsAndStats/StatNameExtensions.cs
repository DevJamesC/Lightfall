using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.StatsAndTags
{
    public static class StatNameExtensions
    {
        public static float Default(this StatName name)
        {
            float returnVal = 1;

            //special cases, commented below is for example. Enable if a default 
            //switch (name)
            //{
            //    case StatName.WeaponDamage: returnVal = 1; break;
            //    case StatName.MaxHealth: returnVal = 1; break;
            //    case StatName.FireRate: returnVal = 1; break;
            //    default: Debug.Log($"{name} Has no default value set, so it defaults x1 multiplier"); break;
            //}

            return returnVal;
        }
    }
}
