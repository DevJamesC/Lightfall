using MBS.DamageSystem;
using MBS.ForceSystem;
using MBS.ModifierSystem;
using MBS.StatsAndTags;
using Opsive.UltimateCharacterController.Traits.Damage;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.HealthSystem
{
    public class Health : ValuePool, IDamageable, IForceable, ICrowdControllable
    {
        [SerializeField, Tooltip("The flat amount to reduce all incoming damage by.")]
        private float armorValue;
        public Shield shield { get; private set; }
        private Dictionary<Collider, ValuePoolColliderModifier> colliderModifiers;
        public bool HasArmor { get => ArmorValue > 0; }
        public float ArmorValue { get => armorValue; protected set => armorValue = value; }

        private ModifierHandler modifierHandler;
        private new Rigidbody rigidbody;

        [SerializeField, Tooltip("used to calculate stagger thresholds. 1000 is good for small targets, 10k is good for large targets. This has nothing to do with rigidbodies and will not effect ragdoll force.")]
        private float WeightlessStaggerThreshold = 1000;

        [SerializeField, Tooltip("Resistant to soft cc increases threashold by .25, and hard cc increases threshold by .5. It will also be used by other systems to figure out the cc resist level.")]
        private CrowdControlType resistantToCrowdControl;
        public CrowdControlType ResistantToCrowndControl { get => resistantToCrowdControl; }

        public event Action Staggered = delegate { };
        public event Action Ragdolled = delegate { };

        [SerializeField, Range(0, 100)]
        private float damageReductionFromSelfDamage = 100;
        [SerializeField, Range(0, 100)]
        private float damageReductionFromFreindlyFire = 100;
        [SerializeField, Tooltip("Use this for determining what damage is from enemies and allies with regard to freindly fire.")]
        public List<Tag> FreindlyTags = new List<Tag>();

        private void Awake()
        {
            shield = GetComponent<Shield>();
            modifierHandler = GetComponent<ModifierHandler>();
            rigidbody = GetComponent<Rigidbody>();
            colliderModifiers = new Dictionary<Collider, ValuePoolColliderModifier>();
        }

        public void AddColliderModifierToDictionary(ValuePoolColliderModifier colliderMod)
        {
            if (colliderModifiers.ContainsKey(colliderMod.Collider))
                return;

            colliderModifiers.Add(colliderMod.Collider, colliderMod);
        }

        public override void TakeDamage(MBS.DamageSystem.DamageData damageData, Collider colliderHit = null)
        {
            if (!IsAlive)
            {
                Debug.Log("force was applied to health... but it is unahndled.");
                //TakeForce(damageData.ForceData);
                return;
            }

            bool damagerIsSelf = false;
            //check self damage (only handle this if the incoming damage is from self, and not intended to be self inflicted damage
            if (damageData.DamageSource != null && !damageData.GetUserData<MBSExtraDamageData>().IsSelfDamage)
            {
                if (damageData.DamageSource.SourceGameObject.transform.root.gameObject == gameObject.transform.root.gameObject)
                    if (damageReductionFromSelfDamage >= 100)
                        return;
                    else
                    {
                        damageData.Amount = damageData.Amount * (1 - (damageReductionFromSelfDamage / 100));
                        //damageData.ForceData.SetForce(damageData.ForceData.Force * (1 - (damageReductionFromSelfDamage / 100)));
                        damagerIsSelf = true;
                    }

            }

            if (!damagerIsSelf)
            {
                //check friendly fire damage. if it is freindly fire, and we cannot take freindly damage, then return.
                if (HandleFriendlyFire(damageData))
                    return;
            }

            HandleColliderModifiers(damageData, colliderHit);

            HandleDamageReduction(damageData);

            HandleShield(damageData);

            InvokePreDamageEvent(damageData);

            ModifyDamageByArmor(damageData);

            //TakeForce(damageData.ForceData);
            Debug.Log("force was applied to health... but it is unahndled.");

            DecreaseCurrentValue(damageData.Amount);

            InvokePostDamageEvent(damageData);

        }

        private bool HandleFriendlyFire(MBS.DamageSystem.DamageData damageData)
        {
            //check if the damage is from a friendly
            bool isFreindlyDamage = false;
            foreach (var freindlyTag in FreindlyTags)
            {

                if (damageData.GetUserData<MBSExtraDamageData>().SourceTags.Contains(freindlyTag))
                {
                    isFreindlyDamage = true;
                    break;
                }
            }

            if (!isFreindlyDamage)
                return false;

            if (damageReductionFromFreindlyFire >= 100)
                return true;

            damageData.Amount = damageData.Amount * (damageReductionFromFreindlyFire / 100);
            damageData.ForceMagnitude = damageData.ForceMagnitude * (1 - (damageReductionFromFreindlyFire / 100));
            return false;
        }

        private void HandleDamageReduction(MBS.DamageSystem.DamageData damageData)
        {
            if (modifierHandler == null)
                return;

            float modifierValue = modifierHandler.GetStatModifierValue(StatName.DamageReduction) - 1;
            damageData.Amount = damageData.Amount - (damageData.Amount * modifierValue);
        }

        private void HandleShield(MBS.DamageSystem.DamageData damageData)
        {
            if (shield != null && !damageData.GetUserData<MBSExtraDamageData>().IgnoreShield)
            {
                shield.TakeDamage(damageData);
            }
        }

        private void ModifyDamageByArmor(MBS.DamageSystem.DamageData damageData)
        {

            if (HasArmor && !damageData.GetUserData<MBSExtraDamageData>().IgnoreArmor)
                damageData.Amount = (Mathf.RoundToInt(damageData.Amount * damageData.GetUserData<MBSExtraDamageData>().ArmorEffectiveness)) - ArmorValue;

        }

        private void HandleColliderModifiers(MBS.DamageSystem.DamageData damageData, Collider collider = null)
        {
            if (collider == null)
                return;

            if (!colliderModifiers.ContainsKey(collider))
                return;

            if (colliderModifiers[collider] == null)
            {
                colliderModifiers.Remove(collider);
                return;
            }

            colliderModifiers[collider].InvokeOnTakeHit(damageData);
        }

        protected override void ResetValuePoolComponent()
        {
            if (modifierHandler == null)
            {
                currentValue = maxValue;
                return;
            }

            currentValue = maxValue * modifierHandler.GetStatModifierValue(StatName.MaxHealth);
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
                currentValue = maxValue;
        }

        public void TakeForce(ForceData forceData)
        {
            if (forceData == null)
                return;

            if (!IsAlive)
            {
                forceData.ApplyForceToRigidbody(rigidbody);
                return;
            }

            if (!forceData.IgnoreShield)
            {
                if (shield != null)
                    if (shield.IsAlive)
                        return;
            }

            float realWeightlessStaggerThreshold = WeightlessStaggerThreshold;
            if (resistantToCrowdControl == CrowdControlType.SoftCC)
                realWeightlessStaggerThreshold *= 1.25f;
            if (resistantToCrowdControl == CrowdControlType.HardCC)
                realWeightlessStaggerThreshold *= 1.5f;

            float chanceToStagger = Mathf.Pow((forceData.Force / realWeightlessStaggerThreshold), 1.75f) * 100;
            float chanceToRagdoll = Mathf.Pow((forceData.Force / (realWeightlessStaggerThreshold * 1.5f)), 1.75f) * 100;
            float randomRoll = UnityEngine.Random.Range(1, 100);
            //Debug.Log($"Rolled {randomRoll}, need {chanceToStagger} or lower to Stagger and {chanceToRagdoll} or lower to Ragdoll.");
            if (randomRoll <= chanceToRagdoll)
            {
                Debug.Log("Ragdolled!");
                forceData.ApplyForceToRigidbody(rigidbody);
                Ragdolled.Invoke();
            }
            else if (randomRoll <= chanceToStagger)
            {
                Debug.Log("Staggered!");
                Staggered.Invoke();
            }


        }
    }
}