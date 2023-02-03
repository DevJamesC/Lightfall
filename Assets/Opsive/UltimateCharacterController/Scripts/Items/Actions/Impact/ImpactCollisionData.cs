/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Impact
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Traits.Damage;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.SurfaceSystem;
    using System;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;
    
    /// <summary>
    /// The impact callback data contains impact data and a character item action which will listen to nay callbacks.
    /// </summary>
    public class ImpactCallbackContext
    {
        protected ImpactCollisionData m_ImpactCollisionData;
        protected CharacterItemAction m_CharacterItemAction;
        protected IImpactDamageData m_ImpactDamageData;

        public CharacterItemAction CharacterItemAction => m_CharacterItemAction;
        public ImpactCollisionData ImpactCollisionData { get => m_ImpactCollisionData; set => m_ImpactCollisionData = value; }
        public IImpactDamageData ImpactDamageData { get => m_ImpactDamageData; set => m_ImpactDamageData = value; }
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ImpactCallbackContext() { }

        /// <summary>
        /// Construcor with the character item action that will receive callbacks.
        /// </summary>
        /// <param name="characterItemAction">The character item action that will receive callbacks.</param>
        public ImpactCallbackContext(CharacterItemAction characterItemAction)
        {
            SetCharacterItemAction(characterItemAction);
        }

        /// <summary>
        /// Set the character item action that will receive callbacks.
        /// </summary>
        /// <param name="characterItemAction">The character item action that will receive callbacks.</param>
        public virtual void SetCharacterItemAction(CharacterItemAction characterItemAction)
        {
            m_CharacterItemAction = characterItemAction;
        }

        /// <summary>
        /// Reset the data.
        /// </summary>
        public virtual void Reset()
        {
            m_ImpactCollisionData = null;
        }

        /// <summary>
        /// Invoke an interrupt callback on the character initiator.
        /// </summary>
        /// <param name="impactAction">A reference to the Impact Actiuon.</param>
        public virtual void InvokeInterruptCallback(ImpactAction impactAction)
        {
            var originator = m_ImpactCollisionData.SourceGameObject;
            if (originator == null) {
                Debug.LogWarning("Cannot call Impact event on a null originator.");
                return;
            }
            EventHandler.ExecuteEvent<ImpactCallbackContext, ImpactAction>(originator, "ImpactInteruptedCallback", this, impactAction);
        }

        /// <summary>
        /// Invoke an impact callback on the originator.
        /// </summary>
        public virtual void InvokeImpactOriginatorCallback()
        {
            var originator = m_ImpactCollisionData.SourceGameObject;
            if (originator == null) {
                Debug.LogWarning("Cannot call Impact event on a null originator.");
                return;
            }
            EventHandler.ExecuteEvent<ImpactCallbackContext>(originator, "OnObjectImpactSourceCallback", this);
        }
        
        /// <summary>
        /// Invoked an impact callback on the target.
        /// </summary>
        public virtual void InvokeImpactTargetCallback()
        {
            var target = m_ImpactCollisionData.TargetGameObject;
            if (target == null) {
                Debug.LogWarning("Cannot call Impact event on a null target.");
                return;
            }
            EventHandler.ExecuteEvent<ImpactCallbackContext>(target, "OnObjectImpact", this);
        }
        
        /// <summary>
        /// String format to visualize the data.
        /// </summary>
        /// <returns>string format.</returns>
        public override string ToString()
        {
            return "Impact Callback Context:\n  " +
                   "Character Item Action: " + (m_CharacterItemAction == null? "(null)" : m_CharacterItemAction) + "\n  " +
                   m_ImpactCollisionData + "\n  " +
                   "Damage Data: "+(ImpactDamageData == null? "(null)" : ImpactDamageData);
        }
    }

    /// <summary>
    /// The Impact data contains all relevant data about an impact.
    /// </summary>
    [Serializable]
    public class ImpactCollisionData
    {
        protected bool m_Initialized;
        public bool Initialized => m_Initialized;
        
        protected uint m_SourceID;
        protected LayerMask m_LayerMask;
        protected RaycastHit m_RaycastHit;
        protected Vector3 m_ImpactPosition;
        protected GameObject m_ImpactGameObject;
        protected Rigidbody m_ImpactRigidbody;
        protected Collider m_ImpactCollider;
        protected Vector3 m_ImpactDirection;
        protected float m_ImpactStrength;
        protected IDamageTarget m_DamageTarget;
        protected IDamageSource m_DamageSource;
        protected Component m_SourceComponent;
        protected GameObject m_SourceGameObject;
        protected GameObject m_SourceOwner;
        protected GameObject m_SourceRootOwner;
        protected CharacterLocomotion m_SourceCharacterLocomotion;
        protected int m_HitCount;
        protected ListSlice<Collider> m_HitColliders;
        protected UsableAction m_SourceItemAction;
        protected SurfaceImpact m_SurfaceImpact;

        private DefaultDamageSource m_CachedDamageSource = new DefaultDamageSource();

        /// <summary>
        /// The source id is used to identify the cause of the impact. For example it can be set to the hitbox index, magic cast index, etc.
        /// </summary>
        public uint SourceID { get => m_SourceID; set => m_SourceID = value; }
        /// <summary>
        /// Most impact have raycast hit.
        /// </summary>
        public RaycastHit RaycastHit { get => m_RaycastHit; set => m_RaycastHit = value; }
        /// <summary>
        /// The position the impact happened.
        /// </summary>
        public Vector3 ImpactPosition { get => m_ImpactPosition; set => m_ImpactPosition = value; }
        /// <summary>
        /// The gameobject that was impacted by the collision.
        /// </summary>
        public GameObject ImpactGameObject { get => m_ImpactGameObject; set => m_ImpactGameObject = value; }
        /// <summary>
        /// The rigidbody of the gameobject that was impacted.
        /// </summary>
        public Rigidbody ImpactRigidbody { get => m_ImpactRigidbody; set => m_ImpactRigidbody = value; }
        /// <summary>
        /// The collider that was impacted.
        /// </summary>
        public Collider ImpactCollider { get => m_ImpactCollider; set => m_ImpactCollider = value; }
        /// <summary>
        /// The direction of the impact.
        /// </summary>
        public Vector3 ImpactDirection { get => m_ImpactDirection; set => m_ImpactDirection = value; }
        /// <summary>
        /// The strength of the impact, this can be used as a multiplier for force or damage.
        /// </summary>
        public float ImpactStrength { get => m_ImpactStrength; set => m_ImpactStrength = value; }
        /// <summary>
        /// The damage target.
        /// </summary>
        public IDamageTarget DamageTarget { get => m_DamageTarget; set => m_DamageTarget = value; }
        /// <summary>
        /// The damage originator.
        /// </summary>
        public IDamageSource DamageSource { get => m_DamageSource; set => m_DamageSource = value; }
        /// <summary>
        /// The Component that caused the impact.
        /// </summary>
        public Component SourceComponent { get=> m_SourceComponent; set=> m_SourceComponent = value; }
        /// <summary>
        /// The gameobject that caused the impact.
        /// </summary>
        public GameObject SourceGameObject { get => m_SourceGameObject; set => m_SourceGameObject = value; }
        /// <summary>
        /// The gameobject that caused the impact.
        /// </summary>
        public GameObject SourceOwner { get => m_SourceOwner; set => m_SourceOwner = value; }
        /// <summary>
        /// The character that caused the impact.
        /// </summary>
        public GameObject SourceRootOwner { get => m_SourceRootOwner; set => m_SourceRootOwner = value; }
        /// <summary>
        /// The character locomotion of the character that caused the impact.
        /// </summary>
        public CharacterLocomotion SourceCharacterLocomotion { get => m_SourceCharacterLocomotion; set => m_SourceCharacterLocomotion = value; }
        /// <summary>
        /// The Item Action that caused the impact
        /// </summary>
        public UsableAction SourceItemAction { get=> m_SourceItemAction; set=> m_SourceItemAction = value; }
        /// <summary>
        /// The surface impact.
        /// </summary>
        public SurfaceImpact SurfaceImpact { get=> m_SurfaceImpact; set => m_SurfaceImpact = value; }
        /// <summary>
        /// The layers that were used as layer mask to detect that collision.
        /// </summary>
        public LayerMask DetectLayers { get=> m_LayerMask; set=> m_LayerMask = value; }
        /// <summary>
        /// Count of the colliders that were detected during the collision
        /// </summary>
        public int HitCount { get=> m_HitCount; set=> m_HitCount = value; }
        /// <summary>
        /// All the colliders that were detected during the collision
        /// </summary>
        public ListSlice<Collider> HitColliders { get=> m_HitColliders; set=> m_HitColliders = value; }
        public GameObject TargetGameObject => m_ImpactGameObject;


        /// <summary>
        /// Reset the data such that the object can be reused.
        /// </summary>
        public virtual void Reset()
        {
            m_Initialized = false;
            
            SourceID = 0;
            ImpactPosition = Vector3.zero;
            ImpactGameObject = null;
            ImpactRigidbody = null;
            ImpactCollider = null;
            ImpactDirection = Vector3.zero;
            ImpactStrength = 0;
            SourceComponent = null;
            SourceGameObject = null;
            SourceOwner = null;
            SourceRootOwner = null;
            SourceCharacterLocomotion = null;
            SourceItemAction = null;
            RaycastHit = default;
            DetectLayers = 0;
            HitCount = -1;
            HitColliders = default;
            DamageTarget = null;
            DamageSource = null;
            m_SurfaceImpact = null;
        }

        /// <summary>
        /// Copy the Impact collision dat from another impact collision data.
        /// </summary>
        /// <param name="other">The other impact collision data to copy.</param>
        public virtual void Copy(ImpactCollisionData other)
        {
            SourceID = other.SourceID;
            ImpactPosition = other.ImpactPosition;
            ImpactGameObject = other.ImpactGameObject;
            ImpactRigidbody = other.ImpactRigidbody;
            ImpactCollider = other.ImpactCollider;
            ImpactDirection = other.ImpactDirection;
            ImpactStrength = other.ImpactStrength;
            SourceComponent = other.SourceComponent;
            SourceGameObject = other.SourceGameObject;
            SourceOwner = other.SourceOwner;
            SourceRootOwner = other.SourceRootOwner;
            SourceCharacterLocomotion = other.SourceCharacterLocomotion;
            SourceItemAction = other.SourceItemAction;
            RaycastHit = other.RaycastHit;
            DetectLayers = other.DetectLayers;
            HitCount = other.HitCount;
            HitColliders = other.HitColliders;
            DamageTarget = other.DamageTarget;
            DamageSource = other.DamageSource;
            m_SurfaceImpact = other.SurfaceImpact;
        }

        /// <summary>
        /// Initialize the data before it can be used again.
        /// </summary>
        public virtual void Initialize()
        {
            m_Initialized = true;
        }

        /// <summary>
        /// Set the raycast that defines the impact data.
        /// </summary>
        /// <param name="hit">The raycast hit.</param>
        public virtual void SetRaycast(RaycastHit hit)
        {
            RaycastHit = hit;
            ImpactPosition = hit.point;
            ImpactDirection = -hit.normal;
            
            SetImpactTarget(hit.collider);
        }
        
        /// <summary>
        /// Set the impact origin by specifying the item action that caused it.
        /// </summary>
        /// <param name="sourceItemAction">The usable action that caused the impact.</param>
        public void SetImpactSource(UsableAction sourceItemAction)
        {
            DamageSource = sourceItemAction;
            SourceComponent = sourceItemAction;
            SourceGameObject = sourceItemAction.gameObject;
            SourceOwner = DamageSource.SourceOwner;
            SourceItemAction = sourceItemAction;
            SourceRootOwner = sourceItemAction.Character;
            SourceCharacterLocomotion = sourceItemAction.CharacterLocomotion;
        }
        
        /// <summary>
        /// Set the impact origin.
        /// </summary>
        /// <param name="sourceComponent">The component that caused the impact.</param>
        /// <param name="sourceCharacter">The character that owns the component that caused the impact.</param>
        public void SetImpactSource(Component sourceComponent, GameObject sourceCharacter)
        {
            SourceComponent = sourceComponent;
            SourceGameObject = sourceComponent.gameObject;
            DamageSource = sourceComponent as IDamageSource;
            if (DamageSource == null) {
                DamageSource = sourceComponent.gameObject.GetCachedComponent<IDamageSource>();
            }
            SourceItemAction = sourceComponent as UsableAction;
            SourceRootOwner = sourceCharacter;
            SourceCharacterLocomotion = sourceCharacter?.GetCachedComponent<CharacterLocomotion>();
            
            if (DamageSource == null) {
                m_CachedDamageSource.Reset();
                m_CachedDamageSource.SourceComponent = sourceComponent;
                m_CachedDamageSource.SourceGameObject = SourceGameObject;
                if (sourceCharacter == null) {
                    m_CachedDamageSource.SourceOwner = SourceGameObject;
                } else {
                    m_CachedDamageSource.SourceOwner = sourceCharacter;
                }
                DamageSource = m_CachedDamageSource;
            }
            SourceOwner = DamageSource.SourceOwner;
        }
        
        /// <summary>
        /// Set the impact origin.
        /// </summary>
        /// <param name="sourceGameObject">The originator gameobject.</param>
        /// <param name="sourceCharacter">The character originator gameobject.</param>
        public void SetImpactSource(GameObject sourceGameObject, GameObject sourceCharacter)
        {
            SourceGameObject = sourceGameObject;
            SourceItemAction = sourceGameObject.GetCachedComponent<UsableAction>();

            if (SourceItemAction != null) {
                DamageSource = SourceItemAction;
            } else {
                DamageSource = sourceGameObject.GetCachedComponent<IDamageSource>();
            }
            if (DamageSource is Component component) {
                SourceComponent = component;
            } else {
                SourceComponent = SourceItemAction;
            }
            SourceRootOwner = sourceCharacter;
            SourceCharacterLocomotion = sourceCharacter?.GetCachedComponent<CharacterLocomotion>();
            
            if (DamageSource == null) {
                m_CachedDamageSource.Reset();
                m_CachedDamageSource.SourceComponent = SourceComponent;
                m_CachedDamageSource.SourceGameObject = SourceGameObject;
                if (sourceCharacter == null) {
                    m_CachedDamageSource.SourceOwner = SourceGameObject;
                } else {
                    m_CachedDamageSource.SourceOwner = sourceCharacter;
                }
                DamageSource = m_CachedDamageSource;
            }
            SourceOwner = DamageSource.SourceOwner;
        }
        
        /// <summary>
        /// Set the impact origin by specifying the damage originator and optionally the character originator.
        /// </summary>
        /// <param name="damageSource">The damage originator that caused the impact.</param>
        public void SetImpactSource(IDamageSource damageSource)
        {
            DamageSource = damageSource;
            if (DamageSource == null) {
                m_CachedDamageSource.Reset();
                DamageSource = m_CachedDamageSource;
                SourceComponent = null;
                SourceGameObject = null;
                SourceOwner = null;
                SourceItemAction = null;
                SourceRootOwner = null;
                SourceCharacterLocomotion = null;
                return;
            }
            
            SourceGameObject = damageSource.SourceGameObject;
            SourceComponent = damageSource.SourceComponent;
            SourceOwner = damageSource.SourceOwner;
            
            if (DamageSource is UsableAction itemAction) {
                SourceItemAction = itemAction;
            } else {
                SourceItemAction = damageSource.SourceOwner.GetCachedComponent<UsableAction>();
            }
            if (SourceComponent == null) {
                SourceComponent = SourceItemAction;
            }

            damageSource.TryGetCharacterLocomotion(out m_SourceCharacterLocomotion);
            SourceRootOwner = damageSource.GetRootOwner();
        }

        /// <summary>
        /// Set the Impact target by specifying the impact collider and the impact gameobject.
        /// </summary>
        /// <param name="impactCollider">The collider that was impacted.</param>
        /// <param name="impactGameObject">The gameobject that was impacted (if null the attached rigid body is used).</param>
        public void SetImpactTarget(Collider impactCollider, GameObject impactGameObject = null)
        {
            ImpactCollider = impactCollider;
            ImpactRigidbody = impactCollider.attachedRigidbody;
            ImpactGameObject = impactGameObject == null ? impactCollider.gameObject : impactGameObject;
            DamageTarget = DamageUtility.GetDamageTarget(ImpactGameObject);
        }

        /// <summary>
        /// To string in an easy to read format
        /// </summary>
        /// <returns>Easy to read format.</returns>
        public override string ToString()
        {
            return "Impact Data: \n\t" +
                   "Source ID: " + m_SourceID + "\n\t" +
                   "\n\tIMPACT \n\t" +
                   "RaycastHit: " + RaycastHit + "\n\t"+
                   "Impact Position: " + m_ImpactPosition + "\n\t" +
                   "Impact GameObject: " + m_ImpactGameObject + "\n\t" +
                   "Impact Rigidbody: " + ImpactRigidbody + "\n\t"+
                   "Impact Collider: " + m_ImpactCollider + "\n\t"+
                   "Impact Direction: " + ImpactDirection + "\n\t"+
                   "Impact Strength: " + m_ImpactStrength + "\n\t"+
                   "DetectLayers: " + DetectLayers + "\n\t"+
                   "HitCount: " + HitCount + "\n\t"+
                   "HitColliders: " + HitColliders.ToStringDeep()+ "\n\t"+
                   "SurfaceImpact: " + SurfaceImpact + "\n\t"+
                   "DamageTarget: " + DamageTarget + "\n\t"+
                   "\n\tSOURCE \n\t" +
                   "Source Component: " + SourceComponent + "\n\t"+
                   "Originator: " + SourceGameObject + "\n\t"+
                   "Character Initiator: " + SourceRootOwner + "\n\t"+
                   "Character Locomotion Initiator: " + SourceCharacterLocomotion + "\n\t"+
                   "Item Action Initiator: " + SourceItemAction + "\n\t"+
                   "DamageOriginator: " + DamageSource + "\n\t";
        }
    }

    /// <summary>
    /// An interface that contains information about the damage caused by an impact.
    /// </summary>
    public interface IImpactDamageData
    {
        LayerMask LayerMask { get; set; }
        DamageProcessor DamageProcessor { get; set; }
        float DamageAmount { get; set; }
        float ImpactForce { get; set; }
        int ImpactForceFrames { get; set; }
        float ImpactRadius { get; set; }
        string ImpactStateName { get; set; }
        float ImpactStateDisableTimer { get; set; }
        SurfaceImpact SurfaceImpact { get; set; }
    }
    
    /// <summary>
    /// The default impact damage data class.
    /// </summary>
    [Serializable]
    public class ImpactDamageData : IImpactDamageData
    {
        [Tooltip("The Layer mask to which deal damage.")]
        [SerializeField] protected LayerMask m_LayerMask =
            ~( 1 << LayerManager.IgnoreRaycast
              | 1 << LayerManager.Water 
              | 1 << LayerManager.SubCharacter 
              | 1 << LayerManager.Overlay 
              | 1 << LayerManager.VisualEffect);
        [Tooltip("Processes the damage dealt to a Damage Target.")]
        [SerializeField] protected DamageProcessor m_DamageProcessor;
        [Tooltip("The amount of damage to apply to the hit object.")]
        [SerializeField] protected float m_DamageAmount = 10;
        [Tooltip("The amount of force to apply to the hit object.")]
        [SerializeField] protected float m_ImpactForce = 2;
        [Tooltip("The number of frames to add the impact force to.")]
        [SerializeField] protected int m_ImpactForceFrames = 15;
        [Tooltip("The impact radius.")]
        [SerializeField] protected float m_ImpactRadius;
        [Tooltip("The name of the state to activate upon impact.")]
        [SerializeField] protected string m_ImpactStateName;
        [Tooltip("The number of seconds until the impact state is disabled. A value of -1 will require the state to be disabled manually.")]
        [SerializeField] protected float m_ImpactStateDisableTimer = 10;
        [Tooltip("The Surface Impact defines what effects happen on impact.")]
        [SerializeField] protected SurfaceImpact m_SurfaceImpact;


        public LayerMask LayerMask { get => m_LayerMask; set => m_LayerMask = value; }
        public DamageProcessor DamageProcessor { get => m_DamageProcessor; set => m_DamageProcessor = value; }
        public float DamageAmount { get => m_DamageAmount; set => m_DamageAmount = value; }
        public float ImpactForce { get => m_ImpactForce; set => m_ImpactForce = value; }
        public int ImpactForceFrames { get => m_ImpactForceFrames; set => m_ImpactForceFrames = value; }
        public float ImpactRadius { get => m_ImpactRadius; set => m_ImpactRadius = value; }
        public string ImpactStateName { get => m_ImpactStateName; set => m_ImpactStateName = value; }
        public float ImpactStateDisableTimer { get => m_ImpactStateDisableTimer; set => m_ImpactStateDisableTimer = value; }
        public SurfaceImpact SurfaceImpact { get => m_SurfaceImpact; set => m_SurfaceImpact = value; }

        /// <summary>
        /// Copy all the values of the another impact damage data.
        /// </summary>
        /// <param name="other">The other impact data data.</param>
        public void Copy(ImpactDamageData other)
        {
            LayerMask = other.LayerMask;
            DamageProcessor = other.DamageProcessor;
            DamageAmount = other.DamageAmount;
            ImpactForce = other.ImpactForce;
            ImpactForceFrames = other.ImpactForceFrames;
            ImpactRadius = other.ImpactRadius;
            ImpactStateName = other.ImpactStateName;
            ImpactStateDisableTimer = other.ImpactStateDisableTimer;
            SurfaceImpact = other.SurfaceImpact;
        }

        /// <summary>
        /// Returns a new string value.
        /// </summary>
        /// <returns>The new string value.</returns>
        public override string ToString()
        {
            return "Impact Damage Data: \n\t" +
                   "Layer Mask: " + m_LayerMask + "\n\t" +
                   "Damage Processor: " + DamageProcessor + "\n\t" +
                   "Damage Amount: " + DamageAmount + "\n\t" +
                   "Impact Force: " + ImpactForce + "\n\t" +
                   "Impact Force Frames: " + ImpactForceFrames + "\n\t" +
                   "Impact Radius: " + ImpactRadius + "\n\t" +
                   "Impact State Name: " + ImpactStateName + "\n\t" +
                   "Impact State Disable Timer: " + ImpactStateDisableTimer + "\n\t" +
                   "Surface Impact: " + SurfaceImpact + "\n\t";
        }
    }
}