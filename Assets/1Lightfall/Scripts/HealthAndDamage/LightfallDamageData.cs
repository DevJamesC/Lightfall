using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    /// <summary>
    /// This class is used by Opsive.DamageData (in DamageProcessor.cs) in the UserData property to add game specific fields to the damage data
    /// </summary>
    public class LightfallDamageData
    {
        //fields
        private bool useColliderDamageMultiplier;

        //properties
        public bool UseColliderDamageMultiplier { get => useColliderDamageMultiplier; set => useColliderDamageMultiplier = value; }

        public LightfallDamageData()
        {
            useColliderDamageMultiplier = true;
        }
    }
}
