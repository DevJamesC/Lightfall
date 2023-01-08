using Opsive.UltimateCharacterController.Traits.Damage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    [CreateAssetMenu(fileName = "LightfallDamageProcessor", menuName = "MBS/Damage Processors/Lightfall Damage Processor")]
    public class LightfallDamageProcessor : DamageProcessor
    {
        public override void Process(IDamageTarget target, DamageData damageData)
        {
            target.Damage(damageData);
        }
    }
}
