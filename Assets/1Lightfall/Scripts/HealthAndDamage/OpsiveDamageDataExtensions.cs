using Opsive.UltimateCharacterController.Traits.Damage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    public static class OpsiveDamageDataExtensions
    {
        /// <summary>
        /// Casts UserData as type T. If UserData is null, or if cast fails, initalize UserData as new object of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="damageData"></param>
        /// <returns></returns>
        public static T GetUserData<T>(this DamageData damageData) where T : class, new()
        {
            T returnVal = damageData.UserData as T;

            if (returnVal == null)
            {
                returnVal = new T();
                damageData.UserData = returnVal;
            }

            return returnVal;
        }
    }
}
