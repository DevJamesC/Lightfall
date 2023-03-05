using Opsive.UltimateCharacterController.Traits.Damage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.DamageSystem
{
    public static class OpsiveDamageDataExtensions
    {
        /// <summary>
        /// Casts UserData as type T. If UserData is null, or if cast fails, initalize UserData as new object of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="damageData"></param>
        /// <returns></returns>
        public static T GetUserData<T>(this MBS.DamageSystem.DamageData damageData) where T : class, new()
        {
            T returnVal = damageData.UserData as T;

            if (returnVal == null)
            {
                returnVal = new T();
                damageData.UserData = returnVal;
            }

            return returnVal;
        }

        /// <summary>
        /// Casts UserData as type T. If UserData is null, or if cast fails, initalize UserData as new object of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="damageData"></param>
        /// <returns></returns>
        public static T GetUserData<T>(this Opsive.UltimateCharacterController.Traits.Damage.DamageData damageData) where T : class, new()
        {
            T returnVal = damageData.UserData as T;

            if (returnVal == null)
            {
                returnVal = new T();
                damageData.UserData = returnVal;
            }

            return returnVal;
        }

        /// <summary>
        /// Casts UserData as type T. If UserData is null, or if cast fails, initalize UserData as new object of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="damageData"></param>
        /// <returns></returns>
        public static T GetUserData<T>(this DamageSystem.ImpactDamageData damageData) where T : class, new()
        {
            T returnVal = damageData.UserData as T;

            if (returnVal == null)
            {
                returnVal = new T();
                damageData.UserData = returnVal;
            }

            return returnVal;
        }

        public static DamageData Copy(this MBS.DamageSystem.DamageData damageData)
        {
            DamageData newDamageData = new DamageData();
            damageData.Copy(newDamageData);
            return newDamageData;
        }      

        public static Opsive.UltimateCharacterController.Traits.Damage.DamageData Copy(this Opsive.UltimateCharacterController.Traits.Damage.DamageData damageData)
        {
            Opsive.UltimateCharacterController.Traits.Damage.DamageData newDamageData = new Opsive.UltimateCharacterController.Traits.Damage.DamageData();
            damageData.Copy(newDamageData);
            return newDamageData;
        }
    }
}
