using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MBS.ModifierSystem
{
    [CreateAssetMenu(fileName = "New Detonation", menuName = "MBS/Detonations/ new Detonation")]
    public class DetonationSO : ScriptableObject
    {
        [SerializeField]
        private List<DetonationFXContainer> FX;

        [SerializeField]
        private DetonationFXContainer genericDetonationFX;

        public void Detonate(EffectWithIntensityBase primingEffect, EffectWithIntensityData primerData, MajorEffects detonatorType, int detonationLevel, ModifierHandler targetModHandler, GameObject detonatorSource, float damageChangePercent = 1)
        {

            DetonationEffect detonationEffect = null;
            switch (primingEffect.EffectCategory)
            {
                case MajorEffects.Burning:
                    detonationEffect = new DetonationEffectBurning();
                    break;
                case MajorEffects.Freezing:
                    detonationEffect = new DetonationEffectFreezing();
                    break;
                case MajorEffects.Shocking:
                    detonationEffect = new DetonationEffectShocking();
                    break;
                case MajorEffects.Draining:
                    detonationEffect = new DetonationEffectDraining();
                    break;
                case MajorEffects.Warping:
                    detonationEffect = new DetonationEffectWarping();
                    break;
                case MajorEffects.Balefiring:
                    detonationEffect = new DetonationEffectBalefiring();
                    break;

            }

            if (detonationEffect == null)
                return;


            detonationEffect.Detonate(detonationLevel, detonatorType, primerData);
            TriggerFX(GetFXByDetonator(detonatorType, detonationLevel), detonatorSource, new List<GameObject> { targetModHandler.gameObject }, primerData, primingEffect.EffectCategory, detonatorType);

        }

        private void TriggerFX(List<DetonationFXBase> fxList, GameObject source, List<GameObject> targets, EffectWithIntensityData primerData, MajorEffects primerType, MajorEffects detonatorType)
        {
            foreach (var fx in fxList)
            {
                fx.Activate(source, targets, primerData, primerType, detonatorType);
            }
        }

        internal List<MajorEffects> GetReactors(MajorEffects majorEffect)
        {
            switch (majorEffect)
            {
                case MajorEffects.Burning:
                    return new DetonationEffectBurning().GetDetonators();
                case MajorEffects.Freezing:
                    return new DetonationEffectFreezing().GetDetonators();
                case MajorEffects.Shocking:
                    return new DetonationEffectShocking().GetDetonators();
                case MajorEffects.Draining:
                    return new DetonationEffectDraining().GetDetonators();
                case MajorEffects.Warping:
                    return new DetonationEffectWarping().GetDetonators();
                case MajorEffects.Balefiring:
                    return new DetonationEffectBalefiring().GetDetonators();
            }

            return new List<MajorEffects>();
        }

        private List<DetonationFXBase> GetFXByDetonator(MajorEffects detonatorType, int level)
        {
            DetonationFXContainer container = FX.Where(fxList => fxList.detonatorType == detonatorType).FirstOrDefault();
            if (container == null)
                container = genericDetonationFX;


            switch (level)
            {
                case 1: return container.intenstiy1FX;
                case 2: return container.intenstiy2FX;
                case 3: return container.intenstiy3FX;
                case 4: return container.intenstiy4FX;
                case 5: return container.intenstiy5FX;
                case 6: return container.intenstiy6FX;
                case 7: return container.intenstiy7FX;
                default: return container.intenstiy1FX;
            }
        }

        [Serializable]
        private class DetonationFXContainer
        {
            public MajorEffects detonatorType;

            [SerializeReference]
            public List<DetonationFXBase> intenstiy1FX = new List<DetonationFXBase>();
            [SerializeReference]
            public List<DetonationFXBase> intenstiy2FX = new List<DetonationFXBase>();
            [SerializeReference]
            public List<DetonationFXBase> intenstiy3FX = new List<DetonationFXBase>();
            [SerializeReference]
            public List<DetonationFXBase> intenstiy4FX = new List<DetonationFXBase>();
            [SerializeReference]
            public List<DetonationFXBase> intenstiy5FX = new List<DetonationFXBase>();
            [SerializeReference]
            public List<DetonationFXBase> intenstiy6FX = new List<DetonationFXBase>();
            [SerializeReference]
            public List<DetonationFXBase> intenstiy7FX = new List<DetonationFXBase>();
        }
    }

    public class DetonationEffect
    {
        public virtual void Detonate(int detonationLevel, MajorEffects detonatorType, EffectWithIntensityData primerData)
        {
            Debug.Log($"{GetType()} detonation is not handled");
        }

        public virtual List<MajorEffects> GetDetonators()
        {
            return new List<MajorEffects>();
        }

    }

    public class DetonationEffectBurning : DetonationEffect
    {

        public override void Detonate(int detonationLevel, MajorEffects detonatorType, EffectWithIntensityData primerData)
        {
            switch (detonatorType)
            {
                case MajorEffects.Freezing:
                    Debug.Log($"level {detonationLevel} Force detonation with a size of {primerData.DetonationSizeModifier * 100}%");
                    break;
                case MajorEffects.Balefiring:
                    Debug.Log($"level {detonationLevel} Soulfire detonation with a size of {primerData.DetonationSizeModifier * 100}%");
                    break;
                default:
                    Debug.Log($"level {detonationLevel} Explosive detonation with a size of {primerData.DetonationSizeModifier * 100}%");
                    break;

            }
        }

        public override List<MajorEffects> GetDetonators()
        {
            return new List<MajorEffects>() { MajorEffects.Freezing, MajorEffects.Balefiring };
        }
    }
    public class DetonationEffectFreezing : DetonationEffect
    {
        public override void Detonate(int detonationLevel, MajorEffects detonatorType, EffectWithIntensityData primerData)
        {
            switch (detonatorType)
            {
                case MajorEffects.Burning:
                    Debug.Log($"level {detonationLevel} Steam detonation with a size of {primerData.DetonationSizeModifier * 100}%");
                    break;
                case MajorEffects.Shocking:
                    Debug.Log($"level {detonationLevel} Snap Freeze detonation with a size of {primerData.DetonationSizeModifier * 100}%");
                    break;
                default:
                    Debug.Log($"level {detonationLevel} Frost Nova detonation with a size of {primerData.DetonationSizeModifier * 100}%");
                    break;

            }
        }
        public override List<MajorEffects> GetDetonators()
        {
            return new List<MajorEffects>() { MajorEffects.Burning, MajorEffects.Shocking };
        }
    }

    public class DetonationEffectShocking : DetonationEffect
    {
        public override void Detonate(int detonationLevel, MajorEffects detonatorType, EffectWithIntensityData primerData)
        {
            switch (detonatorType)
            {
                case MajorEffects.Draining:
                    Debug.Log($"level {detonationLevel} Overload detonation with a size of {primerData.DetonationSizeModifier * 100}%");
                    break;
                case MajorEffects.Freezing:
                    Debug.Log($"level {detonationLevel} Lockdown detonation with a size of {primerData.DetonationSizeModifier * 100}%");
                    break;
                default:
                    Debug.Log($"level {detonationLevel} Arc detonation with a size of {primerData.DetonationSizeModifier * 100}%");
                    break;

            }
        }

        public override List<MajorEffects> GetDetonators()
        {
            return new List<MajorEffects>() { MajorEffects.Draining, MajorEffects.Freezing };
        }
    }

    public class DetonationEffectDraining : DetonationEffect
    {

        public override void Detonate(int detonationLevel, MajorEffects detonatorType, EffectWithIntensityData primerData)
        {
            switch (detonatorType)
            {
                case MajorEffects.Shocking:
                    Debug.Log($"level {detonationLevel} Paralyzing detonation with a size of {primerData.DetonationSizeModifier * 100}%");
                    break;
                case MajorEffects.Warping:
                    Debug.Log($"level {detonationLevel} Destabilizing detonation with a size of {primerData.DetonationSizeModifier * 100}%");
                    break;
                default:
                    Debug.Log($"level {detonationLevel} Depleting detonation with a size of {primerData.DetonationSizeModifier * 100}%");
                    break;

            }
        }
        public override List<MajorEffects> GetDetonators()
        {
            return new List<MajorEffects>() { MajorEffects.Shocking, MajorEffects.Warping };
        }
    }

    public class DetonationEffectWarping : DetonationEffect
    {
        public override void Detonate(int detonationLevel, MajorEffects detonatorType, EffectWithIntensityData primerData)
        {
            switch (detonatorType)
            {
                case MajorEffects.Balefiring:
                    Debug.Log($"level {detonationLevel} Warprift detonation with a size of {primerData.DetonationSizeModifier * 100}%");
                    break;
                case MajorEffects.Draining:
                    Debug.Log($"level {detonationLevel} Cataclysmic detonation with a size of {primerData.DetonationSizeModifier * 100}%");
                    break;
                    //Warp has no default detonator

            }
        }

        public override List<MajorEffects> GetDetonators()
        {
            return new List<MajorEffects>() { MajorEffects.Balefiring, MajorEffects.Draining };
        }
    }

    public class DetonationEffectBalefiring : DetonationEffect
    {
        public override void Detonate(int detonationLevel, MajorEffects detonatorType, EffectWithIntensityData primerData)
        {
            switch (detonatorType)
            {
                case MajorEffects.Warping:
                    Debug.Log($"level {detonationLevel} Balerift detonation with a size of {primerData.DetonationSizeModifier * 100}%");
                    break;
                case MajorEffects.Burning:
                    Debug.Log($"level {detonationLevel} Abyssal detonation with a size of {primerData.DetonationSizeModifier * 100}%");
                    break;
                    //Balefire has no default detonator

            }
        }
        public override List<MajorEffects> GetDetonators()
        {
            return new List<MajorEffects>() { MajorEffects.Warping, MajorEffects.Burning };
        }
    }
}
