/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace MBS.Lightfall
{
    using MBS.DamageSystem;
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Items.Actions.Impact;
    using Opsive.UltimateCharacterController.Objects;
    using Opsive.UltimateCharacterController.Objects.ItemAssist;
    using Opsive.UltimateCharacterController.Traits.Damage;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using UnityEngine;

    /// <summary>
    /// An impact module used to deal damage and forces to the target.
    /// </summary>
    [Serializable]
    public class LightfallDamage : ImpactAction
    {
        [SerializeField] protected MBSDamageForInspector damageFields;

        public bool UseContextData { get => damageFields.UseContextData; set => damageFields.UseContextData = value; }
        public DamageProcessor DamageProcessor { get { return damageFields.DamageProcessor; } set { damageFields.DamageProcessor = value; } }
        public float DamageAmount { get { return damageFields.DamageAmount; } set { damageFields.DamageAmount = value; } }
        public float ImpactForce { get { return damageFields.ImpactForce; } set { damageFields.ImpactForce = value; } }
        public int ImpactForceFrames { get { return damageFields.ImpactForceFrames; } set { damageFields.ImpactForceFrames = value; } }

        protected DamageSystem.ImpactDamageData m_CachedImpactDamageData;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LightfallDamage() { }

        /// <summary>
        /// Overloaded constructor with use context data.
        /// </summary>
        /// <param name="useContextData">Use the context data rather than the local data?</param>
        public LightfallDamage(bool useContextData) { damageFields.UseContextData = useContextData; }

        /// <summary>
        /// Internal method which performs the impact action.
        /// </summary>
        /// <param name="ctx">Context about the hit.</param>
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            var impactData = ctx.ImpactCollisionData;

            var damageAmount = damageFields.DamageAmount;
            var damageProcessor = damageFields.DamageProcessor;
            var impactForce = damageFields.ImpactForce;
            var impactforceframes = damageFields.ImpactForceFrames;
            var radius = damageFields.ImpactRadius;
            if (damageFields.UseContextData && ctx.ImpactDamageData != null)
            {
                damageAmount = ctx.ImpactDamageData.DamageAmount;
                damageProcessor = ctx.ImpactDamageData.DamageProcessor;
                impactForce = ctx.ImpactDamageData.ImpactForce;
                impactforceframes = ctx.ImpactDamageData.ImpactForceFrames;
                radius = ctx.ImpactDamageData.ImpactRadius;
                MBS.DamageSystem.ImpactDamageData mbsImpactDamageData = ctx.ImpactDamageData as MBS.DamageSystem.ImpactDamageData;
                if (mbsImpactDamageData != null)
                    damageFields.UserData = mbsImpactDamageData.UserData as MBSExtraDamageData;
            }

            // The shield can absorb some (or none) of the damage from the hitscan. (I beleive this is a handheld shield, not sci-fy force shield)
            var shieldCollider = impactData.ImpactCollider.gameObject.GetCachedComponent<ShieldCollider>();
            if (shieldCollider != null)
            {
                damageAmount = shieldCollider.ShieldAction.Damage(ctx, damageAmount);
            }

            var impactForceMagnitude = impactForce * impactData.ImpactStrength;
            var impactDirectionalForce = impactForceMagnitude * impactData.ImpactDirection;

            var target = impactData.ImpactGameObject;

            var damageTarget = DamageUtility.GetDamageTarget(impactData.ImpactGameObject);
            if (damageTarget != null)
            {
                target = damageTarget.HitGameObject;

                // If the shield didn't absorb all of the damage then it should be applied to the character.
                if (damageAmount > 0)
                {
                    // First get the damage data and initialize it.
                    var pooledDamageData = GenericObjectPool.Get<MBS.DamageSystem.DamageData>();
                    pooledDamageData.SetDamage(ctx, damageAmount, impactData.ImpactPosition,
                        impactData.ImpactDirection, impactForceMagnitude, impactforceframes,
                        radius, impactData.ImpactCollider);
                    //Initalize UserData
                    pooledDamageData.UserData = damageFields.UserData.Copy();

                    // Then find how to apply this damage data, through a damage processor or processor module.
                    var damageProcessorModule = impactData.SourceRootOwner?.GetCachedComponent<DamageProcessorModule>();
                    if (damageProcessorModule != null)
                    {
                        damageProcessorModule.ProcessDamage(damageProcessor, damageTarget, pooledDamageData);
                    }
                    else
                    {
                        if (damageProcessor == null) { damageProcessor = DamageProcessor.Default; }
                        damageProcessor.Process(damageTarget, pooledDamageData);
                    }

                    GenericObjectPool.Return(pooledDamageData);
                }

            }
            else
            {
                var forceObject = impactData.TargetGameObject.GetCachedParentComponent<IForceObject>();
                if (forceObject != null)
                {
                    forceObject.AddForce(impactDirectionalForce);
                }
                else if (impactForceMagnitude > 0 && impactData.ImpactRigidbody != null && !impactData.ImpactRigidbody.isKinematic)
                {
                    // If the damage target exists it will apply a force to the rigidbody in addition to procesing the damage.
                    // Otherwise just apply the force to the rigidbody. If the radius is bigger than 0 than it must be explosive.
                    if (radius > 0)
                    {
                        impactData.ImpactRigidbody.AddExplosionForce(impactForceMagnitude * MathUtility.RigidbodyForceMultiplier, impactData.ImpactPosition, radius);

                    }
                    else
                    {
                        impactData.ImpactRigidbody.AddForceAtPosition(impactDirectionalForce * MathUtility.RigidbodyForceMultiplier,
                            impactData.ImpactPosition);
                    }
                }
            }

            // Set the Damage Impact data to the context.
            if (damageFields.SetDamageImpactData)
            {
                var ctxImpactData = ctx.ImpactDamageData;
                if (ctxImpactData == null)
                {
                    if (m_CachedImpactDamageData == null)
                    {
                        m_CachedImpactDamageData = new DamageSystem.ImpactDamageData();
                    }
                    ctxImpactData = m_CachedImpactDamageData;
                }

                ctx.ImpactCollisionData.ImpactGameObject = target;

                ctxImpactData.DamageAmount = damageAmount;
                ctxImpactData.DamageProcessor = damageProcessor;
                ctxImpactData.ImpactForce = impactForce;
                ctxImpactData.ImpactForceFrames = impactforceframes;
                ctxImpactData.ImpactRadius = radius;

                ctx.ImpactDamageData = ctxImpactData;
            }

            //Send the event to the , the collider and its rigidbody, target.
            if (damageFields.InvokeOnObjectImpact)
            {
                target = ctx.ImpactCollisionData.ImpactGameObject;

                var collider = ctx.ImpactCollisionData.ImpactCollider;
                if (collider != null)
                {
                    ctx.ImpactCollisionData.ImpactGameObject = collider.gameObject;
                    ctx.InvokeImpactTargetCallback();
                    if (collider.attachedRigidbody != null &&
                        collider.attachedRigidbody.gameObject != collider.gameObject)
                    {
                        ctx.ImpactCollisionData.ImpactGameObject = collider.attachedRigidbody.gameObject;
                        ctx.InvokeImpactTargetCallback();
                    }

                    if (target != collider.gameObject)
                    {
                        ctx.ImpactCollisionData.ImpactGameObject = target;
                        ctx.InvokeImpactTargetCallback();
                    }
                }
                else
                {
                    ctx.InvokeImpactTargetCallback();
                }

                ctx.ImpactCollisionData.ImpactGameObject = target;

            }
        }
    }
}