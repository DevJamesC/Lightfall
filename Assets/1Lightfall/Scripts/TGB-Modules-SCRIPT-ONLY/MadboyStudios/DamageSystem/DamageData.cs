using MBS.ForceSystem;
using MBS.StatsAndTags;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Items.Actions.Impact;
using Opsive.UltimateCharacterController.SurfaceSystem;
using Opsive.UltimateCharacterController.Traits.Damage;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.DamageSystem
{
    [Serializable]
    public class DamageData : Opsive.UltimateCharacterController.Traits.Damage.DamageData
    {
        public override void SetDamage(float amount, Vector3 position, Vector3 direction, float forceMagnitude, int frames, float radius, GameObject attacker, object attackerObject, Collider hitCollider)
        {
            base.SetDamage(amount, position, direction, forceMagnitude, frames, radius, attacker, attackerObject, hitCollider);
            UserData = new MBSExtraDamageData();
        }

        /// <summary>
        /// Initializes the DamageData to the parameters.
        /// </summary>
        public override void SetDamage(ImpactCallbackContext impactContext, float amount, Vector3 position, Vector3 direction, float forceMagnitude, int frames, float radius, Collider hitCollider)
        {
            base.SetDamage(impactContext, amount, position, direction, forceMagnitude, frames, radius, hitCollider);
            UserData = new MBSExtraDamageData();
        }

        /// <summary>
        /// Initializes the DamageData to the parameters.
        /// </summary>
        public override void SetDamage(IDamageSource damageSource, float amount, Vector3 position, Vector3 direction, float forceMagnitude, int frames, float radius, Collider hitCollider)
        {
            base.SetDamage(damageSource, amount, position, direction, forceMagnitude, frames, radius, hitCollider);
            UserData = new MBSExtraDamageData();
        }
    }

    [Serializable]
    public class ImpactDamageData : Opsive.UltimateCharacterController.Items.Actions.Impact.ImpactDamageData
    {
        [Tooltip("Object allowing custom user data.")]
        protected object m_UserData;

        public object UserData { get => m_UserData; set => m_UserData = value; }

        public ImpactDamageData InjectValues(MBSImpactDamageDataForInspector values)
        {
            LayerMask = values.LayerMask;
            DamageProcessor = values.DamageProcessor;
            DamageAmount = values.DamageAmount;
            ImpactForce = values.ImpactForce;
            ImpactForceFrames = values.ImpactForceFrames;
            ImpactRadius = values.ImpactRadius;
            ImpactStateName = values.ImpactStateName;
            ImpactStateDisableTimer = values.ImpactStateDisableTimer;
            SurfaceImpact = values.SurfaceImpact;
            UserData = values.UserData.Copy();
            return this;
        }
    }

    [Serializable]
    public class MBSExtraDamageData
    {
        [SerializeField] private float _weakpointMultiplier;
        [SerializeField] private float _sheildEffectiveness;
        [SerializeField] private float _armorEffectiveness;
        [SerializeField] private float _staggerForce;
        [SerializeField] private bool _applyExtraDamageFromStaggerForce;
        [SerializeField] private bool _ignoreShield;
        [SerializeField] private bool _ignoreArmor;
        [SerializeField] private bool _isSelfDamage;
        [SerializeField] private List<Tag> _sourceTags;

        public float WeakpointMultiplier { get => _weakpointMultiplier; set => _weakpointMultiplier = value; }
        public float ShieldEffectiveness { get => _sheildEffectiveness; set => _sheildEffectiveness = value; }
        public float ArmorEffectiveness { get => _armorEffectiveness; set => _armorEffectiveness = value; }
        public float StaggerForce { get => _staggerForce; set => _staggerForce = value; }

        public bool ApplyExtraDamageFromStaggerForce { get => _applyExtraDamageFromStaggerForce; set => _applyExtraDamageFromStaggerForce = value; }
        public bool IgnoreShield { get => _ignoreShield; set => _ignoreShield = value; }
        public bool IgnoreArmor { get => _ignoreArmor; set => _ignoreArmor = value; }
        public bool IsSelfDamage { get => _isSelfDamage; set => _isSelfDamage = value; }
        public List<Tag> SourceTags { get => _sourceTags; set => _sourceTags = value; }

        public MBSExtraDamageData()
        {
            Initalize(2, 1, 1, 10, false, false);
        }

        public void Initalize(float weakpointMult, float sheildEff, float armorEff, float staggerForce, bool ignoreShield, bool ignoreArmor, bool isSelfDamage = false, List<Tag> sourceTags = null)
        {
            WeakpointMultiplier = weakpointMult;
            ShieldEffectiveness = sheildEff;
            ArmorEffectiveness = armorEff;
            StaggerForce = staggerForce;
            IgnoreShield = ignoreShield;
            IgnoreArmor = ignoreArmor;
            IsSelfDamage = isSelfDamage;
            SourceTags = sourceTags == null ? new List<Tag>() : sourceTags;
        }

        public MBSExtraDamageData Copy()
        {
            MBSExtraDamageData returnVal = new MBSExtraDamageData();
            returnVal.WeakpointMultiplier = WeakpointMultiplier;
            returnVal.ShieldEffectiveness = ShieldEffectiveness;
            returnVal.ArmorEffectiveness = ArmorEffectiveness;
            returnVal.StaggerForce = StaggerForce;
            returnVal.ApplyExtraDamageFromStaggerForce = ApplyExtraDamageFromStaggerForce;
            returnVal.IgnoreShield = IgnoreShield;
            returnVal.IgnoreArmor = IgnoreArmor;
            returnVal.IsSelfDamage = IsSelfDamage;
            returnVal.SourceTags = SourceTags;
            return returnVal;
        }
    }

    /// <summary>
    /// Used to serialize damage data, options, and user data for designer modification in the inspector
    /// </summary>
    [Serializable]
    public class MBSDamageForInspector
    {
        [Tooltip("Use the impact damage data from the context if it is possible?")]
        [SerializeField] public bool UseContextData;
        [Tooltip("Use the impact damage data from the context if it is possible?")]
        [SerializeField] public bool SetDamageImpactData = true;
        [Tooltip("Invoke On Object Impact event")]
        [SerializeField] public bool InvokeOnObjectImpact;
        [Tooltip("Processes the damage dealt to a Damage Target.")]
        [SerializeField] public DamageProcessor DamageProcessor;
        [Tooltip("The amount of damage to apply to the hit object.")]
        [SerializeField] public float DamageAmount = 10;
        [Tooltip("The amount of force to apply to the hit object.")]
        [SerializeField] public float ImpactForce = 2;
        [Tooltip("The number of frames to add the impact force to.")]
        [SerializeField] public int ImpactForceFrames = 15;
        [Tooltip("The impact radius.")]
        [SerializeField] public float ImpactRadius;
        [Tooltip("The user specified fields.")]
        [SerializeField] public MBSExtraDamageData UserData = new MBSExtraDamageData();
    }
    [Serializable]
    public class MBSImpactDamageDataForInspector
    {
        [Tooltip("The Layer mask to which deal damage.")]
        [SerializeField]
        public LayerMask LayerMask =
           ~(1 << LayerManager.IgnoreRaycast
             | 1 << LayerManager.Water
             | 1 << LayerManager.SubCharacter
             | 1 << LayerManager.Overlay
             | 1 << LayerManager.VisualEffect);
        [Tooltip("Processes the damage dealt to a Damage Target.")]
        [SerializeField] public DamageProcessor DamageProcessor;
        [Tooltip("The amount of damage to apply to the hit object.")]
        [SerializeField] public float DamageAmount = 10;
        [Tooltip("The amount of force to apply to the hit object.")]
        [SerializeField] public float ImpactForce = 2;
        [Tooltip("The number of frames to add the impact force to.")]
        [SerializeField] public int ImpactForceFrames = 15;
        [Tooltip("The impact radius.")]
        [SerializeField] public float ImpactRadius;
        [Tooltip("The name of the state to activate upon impact.")]
        [SerializeField] public string ImpactStateName;
        [Tooltip("The number of seconds until the impact state is disabled. A value of -1 will require the state to be disabled manually.")]
        [SerializeField] public float ImpactStateDisableTimer = 10;
        [Tooltip("The Surface Impact defines what effects happen on impact.")]
        [SerializeField] public SurfaceImpact SurfaceImpact;
        [Tooltip("The user specified fields.")]
        [SerializeField] public MBSExtraDamageData UserData = new MBSExtraDamageData();

        public MBSImpactDamageDataForInspector InjectValues(ImpactDamageData values)
        {
            LayerMask = values.LayerMask;
            DamageProcessor = values.DamageProcessor;
            DamageAmount = values.DamageAmount;
            ImpactForce = values.ImpactForce;
            ImpactForceFrames = values.ImpactForceFrames;
            ImpactRadius = values.ImpactRadius;
            ImpactStateName = values.ImpactStateName;
            ImpactStateDisableTimer = values.ImpactStateDisableTimer;
            SurfaceImpact = values.SurfaceImpact;
            UserData = values.UserData as MBSExtraDamageData;
            return this;
        }

        public MBSImpactDamageDataForInspector Copy()
        {
            return new MBSImpactDamageDataForInspector()
            {
                LayerMask = LayerMask,
                DamageProcessor = DamageProcessor,
                DamageAmount = DamageAmount,
                ImpactForce = ImpactForce,
                ImpactForceFrames = ImpactForceFrames,
                ImpactRadius = ImpactRadius,
                ImpactStateName = ImpactStateName,
                ImpactStateDisableTimer = ImpactStateDisableTimer,
                SurfaceImpact = SurfaceImpact,
                UserData = UserData.Copy(),
            };
        }
    }

    //CURRENTLY USING OPSIVE DAMAGEDATA... uncomment all below if not using Opsive DamageData

    //[Serializable]
    //public class DamageData 
    //{
    //    [Tooltip("The object that caused the damage.")]
    //    protected IDamageSource m_DamageSource;
    //    [Tooltip("The object that is the target.")]
    //    protected IDamageTarget m_DamageTarget;
    //    [Tooltip("The amount of damage that should be dealt.")]
    //    protected float m_Amount;
    //    [Tooltip("The hit position.")]
    //    protected Vector3 m_Position;
    //    [Tooltip("The hit direction.")]
    //    protected Vector3 m_Direction;
    //    [Tooltip("The magnitude of the damage force.")]
    //    protected float m_ForceMagnitude;
    //    [Tooltip("The number of frames that the force should be applied over.")]
    //    protected int m_Frames;
    //    [Tooltip("The radius of the force.")]
    //    protected float m_Radius;
    //    [Tooltip("The collider that was hit.")]
    //    protected Collider m_HitCollider;
    //    [Tooltip("The collider that was hit.")]
    //    protected RaycastHit m_ImpactContext;
    //    [Tooltip("Object allowing custom user data.")]
    //    protected object m_UserData;

    //    public IDamageSource DamageSource { get => m_DamageSource; set => m_DamageSource = value; }
    //    public IDamageTarget DamageTarget { get => m_DamageTarget; set => m_DamageTarget = value; }
    //    public float Amount { get => m_Amount; set => m_Amount = value; }
    //    public Vector3 Position { get => m_Position; set => m_Position = value; }
    //    public Vector3 Direction { get => m_Direction; set => m_Direction = value; }
    //    public float ForceMagnitude { get => m_ForceMagnitude; set => m_ForceMagnitude = value; }
    //    public int Frames { get => m_Frames; set => m_Frames = value; }
    //    public float Radius { get => m_Radius; set => m_Radius = value; }
    //    public Collider HitCollider { get => m_HitCollider; set => m_HitCollider = value; }
    //    public RaycastHit ImpactContext { get => m_ImpactContext; set => m_ImpactContext = value; }
    //    public object UserData { get => m_UserData; set => m_UserData = value; }

    //    private DefaultDamageSource m_CachedDamageSource = new DefaultDamageSource();

    //    /// <summary>
    //    /// Initializes the DamageData to the spciefied parameters.
    //    /// </summary>
    //    public virtual void SetDamage(float amount, Vector3 position, Vector3 direction, float forceMagnitude, int frames, float radius, GameObject attacker, object attackerObject, Collider hitCollider)
    //    {
    //        //m_ImpactContext = null;
    //        {
    //            m_CachedDamageSource.OwnerDamageSource = null;
    //            m_CachedDamageSource.SourceOwner = attacker;
    //            if (attackerObject is Component component)
    //            {
    //                m_CachedDamageSource.SourceGameObject = component.gameObject;
    //                m_CachedDamageSource.SourceComponent = component;
    //            }
    //            else if (attackerObject is GameObject attackerGO)
    //            {
    //                m_CachedDamageSource.SourceGameObject = attackerGO;
    //                m_CachedDamageSource.SourceComponent = null;
    //            }
    //            else
    //            {
    //                m_CachedDamageSource.SourceGameObject = null;
    //                m_CachedDamageSource.SourceComponent = null;
    //            }
    //        }
    //        m_DamageSource = m_CachedDamageSource;
    //        m_Amount = amount;
    //        m_Position = position;
    //        m_Direction = direction;
    //        m_ForceMagnitude = forceMagnitude;
    //        m_Frames = frames;
    //        m_Radius = radius;
    //        m_HitCollider = hitCollider;
    //    }

    //    /// <summary>
    //    /// Initializes the DamageData to the parameters.
    //    /// </summary>
    //    //public virtual void SetDamage(RaycastHit impactContext, float amount, Vector3 position, Vector3 direction, float forceMagnitude, int frames, float radius, Collider hitCollider)
    //    //{
    //    //    m_ImpactContext = impactContext;
    //    //    var impactData = impactContext.ImpactCollisionData;
    //    //    m_DamageSource = impactData.DamageSource;
    //    //    m_Amount = amount;
    //    //    m_Position = position;
    //    //    m_Direction = direction;
    //    //    m_ForceMagnitude = forceMagnitude;
    //    //    m_Frames = frames;
    //    //    m_Radius = radius;
    //    //    m_HitCollider = hitCollider;
    //    //}

    //    /// <summary>
    //    /// Initializes the DamageData to the parameters.
    //    /// </summary>
    //    public virtual void SetDamage(IDamageSource damageSource, float amount, Vector3 position, Vector3 direction, float forceMagnitude, int frames, float radius, Collider hitCollider)
    //    {
    //        //m_ImpactContext = null;
    //        m_DamageSource = damageSource;
    //        m_Amount = amount;
    //        m_Position = position;
    //        m_Direction = direction;
    //        m_ForceMagnitude = forceMagnitude;
    //        m_Frames = frames;
    //        m_Radius = radius;
    //        m_HitCollider = hitCollider;
    //    }

    //    /// <summary>
    //    /// Copies the specified DamageData to the current object.
    //    /// </summary>
    //    /// <param name="damageData">The DamageData that should be copied.</param>
    //    public virtual void Copy(DamageData damageData)
    //    {
    //        m_ImpactContext = damageData.ImpactContext;
    //        m_DamageSource = damageData.DamageSource;
    //        m_Amount = damageData.Amount;
    //        m_Position = damageData.Position;
    //        m_Direction = damageData.Direction;
    //        m_ForceMagnitude = damageData.ForceMagnitude;
    //        m_Frames = damageData.Frames;
    //        m_Radius = damageData.Radius;
    //        m_HitCollider = damageData.HitCollider;
    //        m_UserData = damageData.UserData;
    //    }
    //}

    ///// <summary>
    ///// Specifies an object that can cause damage.
    ///// </summary>
    //public interface IDamageSource
    //{
    //    // The Damage Source of the owner, when it is nested. For example, Source Character -> ItemAction -> Projectile -> Explosion.
    //    IDamageSource OwnerDamageSource { get; }
    //    // The owner of the damage source. For example, the turret for projectiles OR ItemAction for hitbox.
    //    GameObject SourceOwner { get; }
    //    // The Source GameObject of the damage. For example, the projectile or explosion GameObject.
    //    GameObject SourceGameObject { get; }
    //    // The Source Component of the damage. For example, the Projectile or Explosion Component.
    //    Component SourceComponent { get; }
    //}

    ///// <summary>
    ///// Default implementation of IDamageSource.
    ///// </summary>
    //public class DefaultDamageSource : IDamageSource
    //{
    //    // The Damage Source of the owner, when it is nested. Example Character -> ItemAction -> Projectile -> Explosion.
    //    public IDamageSource OwnerDamageSource { get; set; }
    //    // The owner of the damage source. Example Turret for Projectiles OR ItemAction for HitBox
    //    public GameObject SourceOwner { get; set; }
    //    // The Source GameObject of the damage. Example Projectile or Explosion GameObject.
    //    public GameObject SourceGameObject { get; set; }
    //    // The Source Component of the damage. Example Projectile or Explosion Component.
    //    public Component SourceComponent { get; set; }

    //    /// <summary>
    //    /// Reset the values to default.
    //    /// </summary>
    //    public void Reset()
    //    {
    //        OwnerDamageSource = null;
    //        SourceOwner = null;
    //        SourceGameObject = null;
    //        SourceComponent = null;
    //    }
    //}

    ///// <summary>
    ///// Specifies an object that can receive damage.
    ///// </summary>
    //public interface IDamageTarget
    //{
    //    // The GameObject that receives damage.
    //    GameObject Owner { get; }
    //    // The GameObject that was hit. This can be a child of the Owner.
    //    GameObject HitGameObject { get; }

    //    /// <summary>
    //    /// Damages the object.
    //    /// </summary>
    //    /// <param name="damageData">The damage received.</param>
    //    void Damage(DamageData damageData);

    //    /// <summary>
    //    /// Is the object alive?
    //    /// </summary>
    //    /// <returns>True if the object is alive.</returns>
    //    bool IsAlive();
    //}
}