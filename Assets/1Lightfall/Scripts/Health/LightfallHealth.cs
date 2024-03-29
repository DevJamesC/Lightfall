namespace MBS.Lightfall
{
    using MBS.DamageSystem;
    using MBS.ForceSystem;
    using MBS.ModifierSystem;
    using MBS.StatsAndTags;
    using Opsive.Shared.Audio;
    using Opsive.Shared.Game;
    using Opsive.Shared.StateSystem;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Events;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Objects;
    using Opsive.UltimateCharacterController.Traits;
#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_MULTIPLAYER
    using Opsive.UltimateCharacterController.Networking;
    using Opsive.UltimateCharacterController.Networking.Traits;
#endif
    using Opsive.UltimateCharacterController.Traits.Damage;
    using Opsive.UltimateCharacterController.Utility;
    using System.Collections.Generic;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Adds health and a shield to the object.
    /// </summary>
    public class LightfallHealth : StateBehavior, Opsive.UltimateCharacterController.Traits.Damage.IDamageTarget
    {
        [Tooltip("Is the object invincible?")]
        [SerializeField] protected bool m_Invincible;
        [Tooltip("The amount of time that the object is invincible after respawning.")]
        [SerializeField] protected float m_TimeInvincibleAfterSpawn;
        [Tooltip("The name of the health attribute.")]
        [SerializeField] protected string m_HealthAttributeName = "Health";
        [Tooltip("The name of the shield attribute.")]
        [SerializeField] protected string m_ShieldAttributeName;
        [Tooltip("The name of armor attribute.")]
        [SerializeField] protected string m_ArmorAttributeName;
        [Tooltip("The type of CC the target resists, if any.")]
        [SerializeField] protected CrowdControlType resistantToCrowdControl;
        [Tooltip("The amount of force to render the target weightless, AKA the stagger threshold.")]
        [SerializeField] protected float WeightlessStaggerThreshold;
        [Tooltip("The list of Colliders that should apply a multiplier when damaged.")]
        [SerializeField] protected Hitbox[] m_Hitboxes;
        [Tooltip("The maximum number of colliders that can be detected when determining if a hitbox was damaged.")]
        [SerializeField] protected int m_MaxHitboxCollisionCount = 10;
        [Tooltip("Any object that should spawn when the object dies.")]
        [SerializeField] protected GameObject[] m_SpawnedObjectsOnDeath;
        [Tooltip("Any object that should be destroyed when the object dies.")]
        [SerializeField] protected GameObject[] m_DestroyedObjectsOnDeath;
        [Tooltip("Should the object be deactivated on death?")]
        [SerializeField] protected bool m_DeactivateOnDeath;
        [Tooltip("If DeactivateOnDeath is enabled, specify a delay for the object to be deactivated.")]
        [SerializeField] protected float m_DeactivateOnDeathDelay;
        [Tooltip("The layer that the GameObject should switch to upon death.")]
        [SerializeField] protected LayerMask m_DeathLayer;
        [Tooltip("A set of AudioClips that can be played when the object takes damage.")]
        [HideInInspector] [SerializeField] protected AudioClipSet m_TakeDamageAudioClipSet = new AudioClipSet();
        [Tooltip("A set of AudioClips that can be played when the object is healed.")]
        [HideInInspector] [SerializeField] protected AudioClipSet m_HealAudioClipSet = new AudioClipSet();
        [Tooltip("A set of AudioClips that can be played when the object dies.")]
        [HideInInspector] [SerializeField] protected AudioClipSet m_DeathAudioClipSet = new AudioClipSet();
        [Tooltip("The ID of the Damage Pop up Manager.")]
        [SerializeField] protected int m_DamagePopupManagerID = -1;
        [Tooltip("Unity event invoked when taking damage.")]
        [SerializeField] protected UnityFloatVector3Vector3GameObjectEvent m_OnDamageEvent;
        [Tooltip("Unity event invoked when healing.")]
        [SerializeField] protected UnityFloatEvent m_OnHealEvent;
        [Tooltip("Unity event invoked when the object dies.")]
        [SerializeField] protected UnityVector3Vector3GameObjectEvent m_OnDeathEvent;

        public GameObject Owner { get { return gameObject; } }
        public GameObject HitGameObject { get { return gameObject; } }
        public DamagePopupMonitor DamagePopupMonitor { get { return m_DamagePopupMonitor; } set { m_DamagePopupMonitor = value; } }
        public bool Invincible { get { return m_Invincible; } set { m_Invincible = value; } }
        public float TimeInvincibleAfterSpawn { get { return m_TimeInvincibleAfterSpawn; } set { m_TimeInvincibleAfterSpawn = value; } }
        public string HealthAttributeName
        {
            get { return m_HealthAttributeName; }
            set
            {
                m_HealthAttributeName = value;
                if (Application.isPlaying)
                {
                    if (!string.IsNullOrEmpty(m_HealthAttributeName))
                    {
                        m_HealthAttribute = m_AttributeManager.GetAttribute(m_HealthAttributeName);
                    }
                    else
                    {
                        m_HealthAttribute = null;
                    }
                }
            }
        }
        public string ShieldAttributeName
        {
            get { return m_ShieldAttributeName; }
            set
            {
                m_ShieldAttributeName = value;
                if (Application.isPlaying)
                {
                    if (!string.IsNullOrEmpty(m_ShieldAttributeName))
                    {
                        m_ShieldAttribute = m_AttributeManager.GetAttribute(m_ShieldAttributeName);
                    }
                    else
                    {
                        m_ShieldAttribute = null;
                    }
                }
            }
        }

        public string ArmorAttributeName
        {
            get { return m_ArmorAttributeName; }
            set
            {
                m_ArmorAttributeName = value;
                if (Application.isPlaying)
                {
                    if (!string.IsNullOrEmpty(m_ArmorAttributeName))
                    {
                        m_ArmorAttribute = m_AttributeManager.GetAttribute(m_ArmorAttributeName);
                    }
                    else
                    {
                        m_ArmorAttribute = null;
                    }
                }
            }
        }
        [Opsive.Shared.Utility.NonSerialized] public Hitbox[] Hitboxes { get { return m_Hitboxes; } set { m_Hitboxes = value; } }
        public int MaxHitboxCollisionCount { get { return m_MaxHitboxCollisionCount; } set { m_MaxHitboxCollisionCount = value; } }
        public GameObject[] SpawnedObjectsOnDeath { get { return m_SpawnedObjectsOnDeath; } set { m_SpawnedObjectsOnDeath = value; } }
        public GameObject[] DestroyedObjectsOnDeath { get { return m_DestroyedObjectsOnDeath; } set { m_DestroyedObjectsOnDeath = value; } }
        public bool DeactivateOnDeath { get { return m_DeactivateOnDeath; } set { m_DeactivateOnDeath = value; } }
        public float DeactivateOnDeathDelay { get { return m_DeactivateOnDeathDelay; } set { m_DeactivateOnDeathDelay = value; } }
        public LayerMask DeathLayer { get { return m_DeathLayer; } set { m_DeathLayer = value; } }
        public AudioClipSet TakeDamageAudioClipSet { get { return m_TakeDamageAudioClipSet; } set { m_TakeDamageAudioClipSet = value; } }
        public AudioClipSet HealAudioClipSet { get { return m_HealAudioClipSet; } set { m_HealAudioClipSet = value; } }
        public AudioClipSet DeathAudioClipSet { get { return m_DeathAudioClipSet; } set { m_DeathAudioClipSet = value; } }
        public int DamagePopupManagerID { get { return m_DamagePopupManagerID; } set { m_DamagePopupManagerID = value; } }
        public UnityFloatVector3Vector3GameObjectEvent OnDamageEvent { get { return m_OnDamageEvent; } set { m_OnDamageEvent = value; } }
        public UnityFloatEvent OnHealEvent { get { return m_OnHealEvent; } set { m_OnHealEvent = value; } }
        public UnityVector3Vector3GameObjectEvent OnDeathEvent { get { return m_OnDeathEvent; } set { m_OnDeathEvent = value; } }

        protected DamagePopupMonitor m_DamagePopupMonitor;
        protected GameObject m_GameObject;
        protected Transform m_Transform;
        protected ModifierHandler m_ModifierHandler;
        private IForceObject m_ForceObject;
        private Rigidbody m_Rigidbody;
        private AttributeManager m_AttributeManager;
        private Attribute m_HealthAttribute;
        private Attribute m_ShieldAttribute;
        private Attribute m_ArmorAttribute;
        private float m_ShieldBaseMaxValue;
        private float m_HealthBaseMaxValue;
#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_MULTIPLAYER
        private INetworkInfo m_NetworkInfo;
        private INetworkHealthMonitor m_NetworkHealthMonitor;
#endif

        private float m_SpawnTime;
        private int m_AliveLayer;
        private Dictionary<Collider, Hitbox> m_ColliderHitboxMap;
        private RaycastHit[] m_RaycastHits;
        private Opsive.UltimateCharacterController.Utility.UnityEngineUtility.RaycastHitComparer m_RaycastHitComparer;

        public float HealthValue { get { return (m_HealthAttribute != null ? m_HealthAttribute.Value : 0); } }
        public float ShieldValue { get { return (m_ShieldAttribute != null ? m_ShieldAttribute.Value : 0); } }
        public float ArmorValue { get { return (m_ArmorAttribute != null ? m_ArmorAttribute.Value : 0); } }
        public float Value { get { return HealthValue + ShieldValue; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_GameObject = gameObject;
            m_Transform = transform;
            m_ForceObject = m_GameObject.GetCachedComponent<IForceObject>();
            m_Rigidbody = m_GameObject.GetCachedComponent<Rigidbody>();
            m_AttributeManager = GetComponent<AttributeManager>();
            m_ModifierHandler = GetComponent<ModifierHandler>();
            if (!string.IsNullOrEmpty(m_HealthAttributeName))
            {
                m_HealthAttribute = m_AttributeManager.GetAttribute(m_HealthAttributeName);
                m_HealthBaseMaxValue = m_HealthAttribute.MaxValue;
                if (m_ModifierHandler != null)
                    m_ModifierHandler.GetStatModifier(StatName.MaxHealth).OnValueChanged += LightfallMaxHealth_OnValueChanged;
            }
            if (!string.IsNullOrEmpty(m_ShieldAttributeName))
            {
                m_ShieldAttribute = m_AttributeManager.GetAttribute(m_ShieldAttributeName);
                m_ShieldBaseMaxValue = m_ShieldAttribute.MaxValue;
                if (m_ModifierHandler != null)
                    m_ModifierHandler.GetStatModifier(StatName.MaxShield).OnValueChanged += LightfallMaxShield_OnValueChanged; ;
            }
            if (!string.IsNullOrEmpty(m_ArmorAttributeName))
            {
                m_ArmorAttribute = m_AttributeManager.GetAttribute(m_ArmorAttributeName);
            }
            m_AliveLayer = m_GameObject.layer;
#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_MULTIPLAYER
            m_NetworkInfo = m_GameObject.GetCachedComponent<INetworkInfo>();
            m_NetworkHealthMonitor = m_GameObject.GetCachedComponent<INetworkHealthMonitor>();
            if (m_NetworkInfo != null && m_NetworkHealthMonitor == null) {
                Debug.LogError("Error: The object " + m_GameObject.name + " must have a NetworkHealthMonitor component.");
            }
#endif

            if (m_Hitboxes != null && m_Hitboxes.Length > 0)
            {
                m_ColliderHitboxMap = new Dictionary<Collider, Hitbox>();
                for (int i = 0; i < m_Hitboxes.Length; ++i)
                {
                    m_ColliderHitboxMap.Add(m_Hitboxes[i].Collider, m_Hitboxes[i]);
                }
                m_RaycastHits = new RaycastHit[m_MaxHitboxCollisionCount];
                m_RaycastHitComparer = new Opsive.UltimateCharacterController.Utility.UnityEngineUtility.RaycastHitComparer();
            }

            EventHandler.RegisterEvent(m_GameObject, "OnRespawn", OnRespawn);
        }

        private void LightfallMaxShield_OnValueChanged(float maxShieldMultiplier)
        {
            ChangeAttributeMaxValue(m_ShieldAttribute, maxShieldMultiplier, m_ShieldBaseMaxValue);
        }

        private void LightfallMaxHealth_OnValueChanged(float maxHealthMultiplier)
        {
            ChangeAttributeMaxValue(m_HealthAttribute, maxHealthMultiplier, m_HealthBaseMaxValue);
        }

        private void ChangeAttributeMaxValue(Attribute attribute, float newMultiplier, float baseMaxValue)
        {
            if (attribute == null)
                return;

            float newMax = baseMaxValue * newMultiplier;
            float difference = newMax - attribute.MaxValue;
            float percentMissing = attribute.Value / attribute.MaxValue;

            attribute.MaxValue = newMax;

            if (difference > 0)
                attribute.Value += difference;
            else
                attribute.Value = attribute.MaxValue * percentMissing;

            if (attribute.Value > newMax)
                attribute.Value = newMax;
        }

        /// <summary>
        /// Get the damage popup manager.
        /// </summary>
        protected virtual void Start()
        {
            if (m_DamagePopupMonitor == null)
            {
                SetDamagePopupManagerUsingID();
            }
        }

        /// <summary>
        /// Use the Damage Popup Manager ID to get the Damage Popup Manager.
        /// </summary>
        public void SetDamagePopupManagerUsingID()
        {
            if (m_DamagePopupManagerID < 0) { return; }

            m_DamagePopupMonitor = GlobalDictionary.Get<DamagePopupMonitor>((uint)m_DamagePopupManagerID);
        }

        /// <summary>
        /// The object has been damaged.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        public void Damage(float amount)
        {
            Damage(amount, m_Transform.position, Vector3.zero, 1, 0, 0, null, null);
        }

        /// <summary>
        /// The object has been damaged.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="direction">The direction that the object took damage from.</param>
        /// <param name="forceMagnitude">The magnitude of the force that is applied to the object.</param>
        public void Damage(float amount, Vector3 position, Vector3 direction, float forceMagnitude)
        {
            Damage(amount, position, direction, forceMagnitude, 1, 0, null, null);
        }

        /// <summary>
        /// The object has been damaged.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="direction">The direction that the object took damage from.</param>
        /// <param name="forceMagnitude">The magnitude of the force that is applied to the object.</param>
        /// <param name="radius">The radius of the explosive damage. If 0 then a non-exposive force will be used.</param>
        public void Damage(float amount, Vector3 position, Vector3 direction, float forceMagnitude, float radius)
        {
            Damage(amount, position, direction, forceMagnitude, 1, radius, null, null);
        }

        /// <summary>
        /// The object has been damaged.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="direction">The direction that the object took damage from.</param>
        /// <param name="forceMagnitude">The magnitude of the force that is applied to the object.</param>
        /// <param name="attacker">The GameObject that did the damage.</param>
        public void Damage(float amount, Vector3 position, Vector3 direction, float forceMagnitude, GameObject attacker)
        {
            Damage(amount, position, direction, forceMagnitude, 1, 0, attacker, null);
        }

        /// <summary>
        /// The object has been damaged.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="direction">The direction that the object took damage from.</param>
        /// <param name="forceMagnitude">The magnitude of the force that is applied to the object.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        /// <param name="radius">The radius of the explosive damage. If 0 then a non-exposive force will be used.</param>
        /// <param name="attacker">The GameObject that did the damage.</param>
        public void Damage(float amount, Vector3 position, Vector3 direction, float forceMagnitude, int frames, float radius, GameObject attacker)
        {
            Damage(amount, position, direction, forceMagnitude, frames, radius, attacker, null);
        }

        /// <summary>
        /// The object has been damaged.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="direction">The direction that the object took damage from.</param>
        /// <param name="forceMagnitude">The magnitude of the force that is applied to the object.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        /// <param name="radius">The radius of the explosive damage. If 0 then a non-explosive force will be used.</param>
        /// <param name="attacker">The GameObject that did the damage.</param>
        /// <param name="hitCollider">The Collider that was hit.</param>
        public void Damage(float amount, Vector3 position, Vector3 direction, float forceMagnitude, int frames, float radius, GameObject attacker, Collider hitCollider)
        {
            Damage(amount, position, direction, forceMagnitude, frames, radius, attacker, null, hitCollider);
        }

        /// <summary>
        /// The object has been damaged.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="direction">The direction that the object took damage from.</param>
        /// <param name="forceMagnitude">The magnitude of the force that is applied to the object.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        /// <param name="radius">The radius of the explosive damage. If 0 then a non-explosive force will be used.</param>
        /// <param name="attacker">The GameObject that did the damage.</param>
        /// <param name="attackerObject">The object that did the damage.</param>
        /// <param name="hitCollider">The Collider that was hit.</param>
        public void Damage(float amount, Vector3 position, Vector3 direction, float forceMagnitude, int frames, float radius, GameObject attacker, object attackerObject, Collider hitCollider)
        {
            var pooledDamageData = GenericObjectPool.Get<Opsive.UltimateCharacterController.Traits.Damage.DamageData>();
            pooledDamageData.SetDamage(amount, position, direction, forceMagnitude, frames, radius, attacker, attackerObject, hitCollider);
            Damage(pooledDamageData);
            GenericObjectPool.Return(pooledDamageData);
        }

        /// <summary>
        /// The object has been damaged.
        /// </summary>
        /// <param name="damageData">The data associated with the damage.</param>
        public virtual void Damage(Opsive.UltimateCharacterController.Traits.Damage.DamageData damageData)
        {
            // Don't take any damage if the object is invincible, already dead, or just spawned and is invincible for a small amount of time.
            if (m_Invincible || !IsAlive() || m_SpawnTime + m_TimeInvincibleAfterSpawn > Time.time || damageData.Amount == 0)
            {
                return;
            }

#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_MULTIPLAYER
            if (m_NetworkInfo != null) {
                if (m_NetworkInfo.IsLocalPlayer()) {
                    m_NetworkHealthMonitor.OnDamage(damageData.Amount, damageData.Position, damageData.Direction, damageData.ForceMagnitude, damageData.Frames, damageData.Radius, damageData.DamageOriginator, damageData.HitCollider);
                }
                return;
            }
#endif

            OnDamage(damageData);
        }

        /// <summary>
        /// The object has taken been damaged.
        /// </summary>
        /// <param name="damageData">The data associated with the damage.</param>
        public virtual void OnDamage(Opsive.UltimateCharacterController.Traits.Damage.DamageData damageData)
        {
            if (damageData == null) { return; }
            MBSExtraDamageData extraDamageFields = damageData.GetUserData<MBSExtraDamageData>();
            //reduce damage by DamageResistance
            if (m_ModifierHandler != null)
            {
                float damageResistance = m_ModifierHandler.GetStatModifierValue(StatName.DamageReduction);
                damageData.Amount -= (damageData.Amount * damageResistance) - damageData.Amount;
            }

            // Add a multiplier if a particular collider was hit. Do not apply a multiplier if the damage is applied through a radius because multiple
            // collider are hit.
            if (damageData.Radius == 0 && damageData.Direction != Vector3.zero && damageData.HitCollider != null)
            {
                if (m_ColliderHitboxMap != null && m_ColliderHitboxMap.Count > 0)
                {
                    Hitbox hitbox;
                    if (m_ColliderHitboxMap.TryGetValue(damageData.HitCollider, out hitbox))
                    {
                        //if (extraDamageFields.UseColliderDamageMultiplier)
                        //damageData.Amount *= hitbox.DamageMultiplier;
                        Debug.Log("You hit a collider which may be a weakpoint, or may be hardened... at any rate, it is not handled");
                    }
                    else
                    {
                        // The main collider may be overlapping child hitbox colliders. Perform one more raycast to ensure a hitbox collider shouldn't be hit.
                        float distance = 0.2f;
                        if (damageData.HitCollider is CapsuleCollider capsuleCollider)
                        {
                            distance = capsuleCollider.radius;
                        }
                        else if (damageData.HitCollider is SphereCollider sphereCollider)
                        {
                            distance = sphereCollider.radius;
                        }

                        // The hitbox collider may be underneath the base collider. Fire a raycast to detemine if there are any colliders underneath the hit collider 
                        // that should apply a multiplier.
                        var hitCount = Physics.RaycastNonAlloc(damageData.Position, damageData.Direction, m_RaycastHits, distance,
                                        ~(1 << LayerManager.IgnoreRaycast | 1 << LayerManager.Overlay | 1 << LayerManager.VisualEffect), QueryTriggerInteraction.Ignore);
                        for (int i = 0; i < hitCount; ++i)
                        {
                            var closestRaycastHit = QuickSelect.SmallestK(m_RaycastHits, hitCount, i, m_RaycastHitComparer);
                            if (closestRaycastHit.collider == damageData.HitCollider)
                            {
                                continue;
                            }
                            // A new collider has been found - stop iterating if the hitbox map exists and use the hitbox multiplier.
                            if (m_ColliderHitboxMap.TryGetValue(closestRaycastHit.collider, out hitbox))
                            {
                                //if (extraDamageFields.UseColliderDamageMultiplier)
                                //damageData.Amount *= hitbox.DamageMultiplier;
                                Debug.Log("You hit a collider which may be a weakpoint, or may be hardened... at any rate, it is not handled");
                                damageData.HitCollider = hitbox.Collider;
                                break;
                            }
                        }
                    }
                }
            }
            float damageFromStaggerForce = extraDamageFields.ApplyExtraDamageFromStaggerForce ? extraDamageFields.StaggerForce / 10 : 0;
            DamageShield(damageData, extraDamageFields, damageFromStaggerForce, out damageFromStaggerForce);
            DamageHealth(damageData, extraDamageFields, damageFromStaggerForce);
            TryStagger(extraDamageFields.StaggerForce);

            var force = damageData.Direction * damageData.ForceMagnitude;
            if (damageData.ForceMagnitude > 0)
            {
                // Apply a force to the object.
                if (m_ForceObject != null)
                {
                    m_ForceObject.AddForce(force, damageData.Frames);
                }
                else
                {
                    // Apply a force to the rigidbody if the object isn't a character.
                    if (m_Rigidbody != null && !m_Rigidbody.isKinematic)
                    {
                        if (damageData.Radius == 0)
                        {
                            m_Rigidbody.AddForceAtPosition(force * MathUtility.RigidbodyForceMultiplier, damageData.Position);
                        }
                        else
                        {
                            m_Rigidbody.AddExplosionForce(force.magnitude * MathUtility.RigidbodyForceMultiplier, damageData.Position, damageData.Radius);
                        }
                    }
                }
            }

            var attacker = damageData.DamageSource?.SourceOwner;
            // Let other interested objects know that the object took damage.
            EventHandler.ExecuteEvent<float, Vector3, Vector3, GameObject, Collider>(m_GameObject, "OnHealthDamage", damageData.Amount, damageData.Position, force, attacker, damageData.HitCollider);
            EventHandler.ExecuteEvent<Opsive.UltimateCharacterController.Traits.Damage.DamageData>(m_GameObject, "OnHealthDamageWithData", damageData);
            if (m_OnDamageEvent != null)
            {
                m_OnDamageEvent.Invoke(damageData.Amount, damageData.Position, force, attacker);
            }
            if (m_DamagePopupMonitor != null)
            {
                m_DamagePopupMonitor.OpenDamagePopup(damageData);
            }

            // The object is dead when there is no more health or shield.
            if (!IsAlive())
            {
#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_MULTIPLAYER
                if (m_NetworkInfo == null || m_NetworkInfo.IsLocalPlayer()) {
#endif
                Die(damageData.Position, force, attacker);
#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_MULTIPLAYER
                }
#endif
            }
            else
            {
                // Play any take damage audio if the object did not die. If the object died then the death audio will play.
                m_TakeDamageAudioClipSet.PlayAudioClip(m_GameObject);

            }
        }

        private void DamageShield(Opsive.UltimateCharacterController.Traits.Damage.DamageData damageData, MBSExtraDamageData extraDamageFields, float damageFromStaggerForce, out float damageFromStaggerForceOut)
        {
            // Apply the damage to the shield first because the shield can regenrate.
            if (m_ShieldAttribute != null && m_ShieldAttribute.Value > m_ShieldAttribute.MinValue)
            {
                float adjustedDamage = damageData.Amount * extraDamageFields.ShieldEffectiveness;
                var shieldAmount = Mathf.Min(adjustedDamage, m_ShieldAttribute.Value - m_ShieldAttribute.MinValue);
                damageData.Amount -= shieldAmount;
                m_ShieldAttribute.Value -= shieldAmount;

                //if we still have shield, take damage from stagger force damage
                if (m_ShieldAttribute.Value > m_ShieldAttribute.MinValue && damageFromStaggerForce > 0)
                {
                    shieldAmount = Mathf.Min(damageFromStaggerForce, m_ShieldAttribute.Value - m_ShieldAttribute.MinValue);
                    damageFromStaggerForce -= shieldAmount;
                    m_ShieldAttribute.Value -= shieldAmount;
                }
                //if we still have shield after damage and stagger force damage, reduce stagger force to 0, because targets with a shield cannot be staggered by normal damage.
                if (m_ShieldAttribute.Value > m_ShieldAttribute.MinValue && damageFromStaggerForce > 0)
                {
                    extraDamageFields.StaggerForce = 0;
                }
            }
            damageFromStaggerForceOut = damageFromStaggerForce;
        }

        private void DamageHealth(Opsive.UltimateCharacterController.Traits.Damage.DamageData damageData, MBSExtraDamageData extraDamageFields, float damageFromStaggerForce)
        {
            // Decrement the health by remaining amount after the shield has taken damage.
            if (m_HealthAttribute != null && m_HealthAttribute.Value > m_HealthAttribute.MinValue)
            {
                float adjustedDamage = damageData.Amount;
                //adjust incoming damage by armor
                if (m_ArmorAttribute != null && m_ArmorAttribute.Value > 0)
                {
                    adjustedDamage *= extraDamageFields.ArmorEffectiveness;
                    adjustedDamage -= m_ArmorAttribute.Value;
                    damageFromStaggerForce -= m_ArmorAttribute.Value;
                }

                var healthAmount = Mathf.Min(adjustedDamage, m_HealthAttribute.Value - m_HealthAttribute.MinValue);
                damageData.Amount -= healthAmount;
                m_HealthAttribute.Value -= healthAmount;

                //if we still have health, take damage from stagger force damage
                if (m_HealthAttribute.Value > m_HealthAttribute.MinValue && damageFromStaggerForce > 0)
                {
                    healthAmount = Mathf.Min(damageFromStaggerForce, m_HealthAttribute.Value - m_HealthAttribute.MinValue);
                    damageFromStaggerForce -= healthAmount;
                    m_HealthAttribute.Value -= healthAmount;
                }
            }
        }

        /// <summary>
        /// Is the object currently alive?
        /// </summary>
        /// <returns>True if the object is currently alive.</returns>
        public bool IsAlive()
        {
            return (m_HealthAttribute != null && m_HealthAttribute.Value > m_HealthAttribute.MinValue) ||
                   (m_ShieldAttribute != null && m_ShieldAttribute.Value > m_ShieldAttribute.MinValue);
        }

        /// <summary>
        /// The object is no longer alive.
        /// </summary>
        /// <param name="position">The position of the damage.</param>
        /// <param name="force">The amount of force applied to the object while taking the damage.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        public virtual void Die(Vector3 position, Vector3 force, GameObject attacker)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.IsLocalPlayer()) {
                m_NetworkHealthMonitor.Die(position, force, attacker);
            }
#endif

            // Spawn any objects on death, such as an explosion if the object is an explosive barrel.
            if (m_SpawnedObjectsOnDeath != null)
            {
                for (int i = 0; i < m_SpawnedObjectsOnDeath.Length; ++i)
                {
                    var spawnedObject = ObjectPoolBase.Instantiate(m_SpawnedObjectsOnDeath[i], m_Transform.position, m_Transform.rotation);
                    MBSExplosion explosion;
                    if ((explosion = spawnedObject.GetCachedComponent<MBSExplosion>()) != null)
                    {
                        explosion.Explode(gameObject);
                    }
                    var rigidbodies = spawnedObject.GetComponentsInChildren<Rigidbody>();
                    for (int j = 0; j < rigidbodies.Length; ++j)
                    {
                        rigidbodies[j].AddForceAtPosition(force, position);
                    }
                }
            }

            // Destroy any objects on death. The objects will be placed back in the object pool if they were created within it otherwise the object will be destroyed.
            if (m_DestroyedObjectsOnDeath != null)
            {
                for (int i = 0; i < m_DestroyedObjectsOnDeath.Length; ++i)
                {
                    if (ObjectPoolBase.InstantiatedWithPool(m_DestroyedObjectsOnDeath[i]))
                    {
                        ObjectPoolBase.Destroy(m_DestroyedObjectsOnDeath[i]);
                    }
                    else
                    {
                        Object.Destroy(m_DestroyedObjectsOnDeath[i]);
                    }
                }
            }

            // Change the layer to a death layer.
            if (m_DeathLayer.value != 0)
            {
                m_AliveLayer = m_GameObject.layer;
                m_GameObject.layer = m_DeathLayer;
            }

            // Play any take death audio. Use PlayAtPosition because the audio won't play if the GameObject is inactive.
            m_DeathAudioClipSet.PlayAtPosition(m_Transform.position);

            // Deactivate the object if requested.
            if (m_DeactivateOnDeath)
            {
                SchedulerBase.Schedule(m_DeactivateOnDeathDelay, Deactivate);
            }

            // The attributes shouldn't regenerate.
            if (m_ShieldAttribute != null)
            {
                m_ShieldAttribute.CancelAutoUpdate();
            }
            if (m_HealthAttribute != null)
            {
                m_HealthAttribute.CancelAutoUpdate();
            }

            // Notify those interested.
            EventHandler.ExecuteEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", position, force, attacker);
            if (m_OnDeathEvent != null)
            {
                m_OnDeathEvent.Invoke(position, force, attacker);
            }
        }

        /// <summary>
        /// Kills the object immediately.
        /// </summary>
        public void ImmediateDeath()
        {
            ImmediateDeath(m_Transform.position, Vector3.zero, 0);
        }

        /// <summary>
        /// Kills the object immediately.
        /// </summary>
        /// <param name="position">The position the character died.</param>
        /// <param name="direction">The direction that the object took damage from.</param>
        /// <param name="forceMagnitude">The magnitude of the force that is applied to the object.</param>
        public void ImmediateDeath(Vector3 position, Vector3 direction, float forceMagnitude)
        {
            var amount = 0f;
            if (m_HealthAttribute != null)
            {
                amount += m_HealthAttribute.Value;
            }
            if (m_ShieldAttribute != null)
            {
                amount += m_ShieldAttribute.Value;
            }
            // If ImmediateDeath is called then the object should die even if it is invincible.
            var invincible = m_Invincible;
            m_Invincible = false;
            Damage(amount, position, direction, forceMagnitude);
            m_Invincible = invincible;
        }

        /// <summary>
        /// Adds amount to health and then to the shield if there is still an amount remaining. Will not go over the maximum health or shield value.
        /// </summary>
        /// <param name="amount">The amount of health or shield to add.</param>
        /// <returns>True if the object was healed.</returns>
        public virtual bool Heal(float amount)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.IsLocalPlayer()) {
                m_NetworkHealthMonitor.Heal(amount);
            }
#endif

            var healAmount = 0f;

            // Contribute the amount of the health first.
            if (m_HealthAttribute != null && m_HealthAttribute.Value < m_HealthAttribute.MaxValue)
            {
                var healthAmount = Mathf.Min(amount, m_HealthAttribute.MaxValue - m_HealthAttribute.Value);
                amount -= healthAmount;
                m_HealthAttribute.Value += healthAmount;
                healAmount += healthAmount;
            }

            // Add any remaining amount to the shield.
            if (m_ShieldAttribute != null && amount > 0 && m_ShieldAttribute.Value < m_ShieldAttribute.MaxValue)
            {
                var shieldAmount = Mathf.Min(amount, m_ShieldAttribute.MaxValue - m_ShieldAttribute.Value);
                m_ShieldAttribute.Value += shieldAmount;
                healAmount += shieldAmount;
            }

            // Don't play any effects if the object wasn't healed.
            if (healAmount == 0)
            {
                return false;
            }

            // Play any heal audio.
            m_HealAudioClipSet.PlayAudioClip(m_GameObject);

            EventHandler.ExecuteEvent<float>(m_GameObject, "OnHealthHeal", healAmount);
            if (m_OnHealEvent != null)
            {
                m_OnHealEvent.Invoke(healAmount);
            }
            if (m_DamagePopupMonitor != null)
            {
                m_DamagePopupMonitor.OpenHealPopup(m_Transform.position, amount);
            }

            return true;
        }

        /// <summary>
        /// The object doesn't have any health or shield left and should be deactivated.
        /// </summary>
        private void Deactivate()
        {
            m_GameObject.SetActive(false);
        }

        /// <summary>
        /// The object has spawned again. Set the health and shield back to their starting values.
        /// </summary>
        protected virtual void OnRespawn()
        {
            if (m_HealthAttribute != null)
            {
                m_HealthAttribute.ResetValue();
            }
            if (m_ShieldAttribute != null)
            {
                m_ShieldAttribute.ResetValue();
            }
            // Change the layer back to the alive layer.
            if (m_DeathLayer.value != 0)
            {
                m_GameObject.layer = m_AliveLayer;
            }
            m_SpawnTime = Time.time;
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent(m_GameObject, "OnRespawn", OnRespawn);
        }

        public float GetColliderMultiplier(Opsive.UltimateCharacterController.Traits.Damage.DamageData damageData)
        {
            if (Hitboxes == null) return 1;

            // The main collider may be overlapping child hitbox colliders. Perform one more raycast to ensure a hitbox collider shouldn't be hit.
            float distance = 0.2f;
            if (damageData.HitCollider is CapsuleCollider capsuleCollider)
            {
                distance = capsuleCollider.radius;
            }
            else if (damageData.HitCollider is SphereCollider sphereCollider)
            {
                distance = sphereCollider.radius;
            }

            var hitCount = Physics.RaycastNonAlloc(damageData.Position, damageData.Direction, m_RaycastHits, distance,
                                        ~(1 << LayerManager.IgnoreRaycast | 1 << LayerManager.Overlay | 1 << LayerManager.VisualEffect), QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hitCount; ++i)
            {
                var closestRaycastHit = QuickSelect.SmallestK(m_RaycastHits, hitCount, i, m_RaycastHitComparer);
                if (closestRaycastHit.collider == damageData.HitCollider)
                {
                    continue;
                }
                // A new collider has been found - stop iterating if the hitbox map exists and use the hitbox multiplier.
                if (m_ColliderHitboxMap.TryGetValue(closestRaycastHit.collider, out Hitbox hitbox))
                {
                    return hitbox.DamageMultiplier;
                }
            }

            return 1;
        }

        public void TryStagger(float incomingForce)
        {

            float realWeightlessStaggerThreshold = WeightlessStaggerThreshold;
            if (resistantToCrowdControl == CrowdControlType.SoftCC)
                realWeightlessStaggerThreshold *= 1.25f;
            if (resistantToCrowdControl == CrowdControlType.HardCC)
                realWeightlessStaggerThreshold *= 1.5f;

            float chanceToStagger = Mathf.Pow((incomingForce / realWeightlessStaggerThreshold), 1.75f) * 100;
            float chanceToRagdoll = Mathf.Pow((incomingForce / (realWeightlessStaggerThreshold * 1.5f)), 1.75f) * 100;
            float randomRoll = UnityEngine.Random.Range(1, 100);
            //Debug.Log($"Rolled {randomRoll}, need {chanceToStagger} or lower to Stagger and {chanceToRagdoll} or lower to Ragdoll.");
            if (randomRoll <= chanceToRagdoll)
            {
                Debug.Log($"{gameObject.name} Ragdolled! -Normal");
                m_Rigidbody.isKinematic = true;
                //forceData.ApplyForceToRigidbody(rigidbody);
                //Ragdolled.Invoke();
            }
            else if (randomRoll <= chanceToStagger)
            {
                Debug.Log($"{gameObject.name} Staggered! -Normal");
                EventHandler.ExecuteEvent(gameObject, "Staggered");
            }
        }

        /// <summary>
        /// Try to stagger a target based on percent change and RNG. This cannot induce ragdoll/ weightlessness.
        /// </summary>
        /// <param name="chanceToStagger">0 is 0%. 100 is 100%. A target's CC resistance will reduce this chance. </param>
        public void TryStaggerRawPercent(float chanceToStagger, bool canStaggerIfShielded = false)
        {
            if (!canStaggerIfShielded && m_ShieldAttribute != null && m_ShieldAttribute.Value > m_ShieldAttribute.MinValue)
                return;

            if (resistantToCrowdControl == CrowdControlType.SoftCC)
                chanceToStagger *= .75f;
            if (resistantToCrowdControl == CrowdControlType.HardCC)
                chanceToStagger *= .5f;

            float randomRoll = UnityEngine.Random.Range(1, 100);
            //Debug.Log($"Rolled {randomRoll}, need {chanceToStagger} or lower to Stagger and {chanceToRagdoll} or lower to Ragdoll.");
            if (randomRoll <= chanceToStagger)
            {
                Debug.Log($"{gameObject.name} Staggered! -Raw Percent");
                EventHandler.ExecuteEvent(gameObject, "Staggered");
            }
        }
    }
}
