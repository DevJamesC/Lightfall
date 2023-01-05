using System;


namespace MBS.AbilitySystem
{
    [Serializable]
    public class AbilityFXBase : IShallowCloneable<AbilityFXBase>
    {
        public virtual void Activate(AbilityWrapperBase wrapper)
        {

        }

        public virtual void Deactivate(AbilityWrapperBase wrapper)
        {

        }

        public virtual void OnUpdate(AbilityWrapperBase wrapper)
        {

        }

        internal AbilityFXBase GetShallowCopy()
        {
            return (AbilityFXBase)MemberwiseClone();
        }
    }
}

