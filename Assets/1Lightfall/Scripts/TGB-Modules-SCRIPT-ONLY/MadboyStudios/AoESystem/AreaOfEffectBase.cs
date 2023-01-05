using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.AoeSystem
{
    public class AreaOfEffectBase : MonoBehaviour
    {
        public event Action<Collider> OnInsidePrimaryRadius = delegate { };
        public event Action<Collider> OnInsideSecondaryRadius = delegate { };

        protected void InvokeInsidePrimaryRadius(Collider target)
        {
            OnInsidePrimaryRadius.Invoke(target);
        }
        protected void InvokeInsideSecondaryRadius(Collider target)
        {
            OnInsideSecondaryRadius.Invoke(target);
        }

        public virtual void Setup(float radius)
        {
            Debug.Log($"setup for {GetType()} has not been implimented...");

        }
    }
}