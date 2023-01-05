using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    [Serializable]
    public class DetonationFXBase
    {
        public virtual void Activate(GameObject source, List<GameObject> targets, EffectWithIntensityData data, MajorEffects primerType, MajorEffects detonatorType)
        {
        }

        public virtual void Deactivate(GameObject source, List<GameObject> targets, EffectWithIntensityData data, MajorEffects primerType, MajorEffects detonatorType)
        {

        }
    }
}
