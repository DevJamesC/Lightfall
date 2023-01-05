using MBS.ForceSystem;
using MBS.StatsAndTags;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.DamageSystem
{
    [Serializable]
    public class DamageData : IShallowCloneable<DamageData>
    {
        public IDamager Source { get; private set; }
        public List<Tag> SourceTags { get; private set; }

        [SerializeReference]
        private ForceData forceData = new ForceData();
        public ForceData ForceData { get => forceData; private set => forceData = value; }
        public float Damage { get => damage; private set => damage = value; }

        public float DamageWithForce
        {
            get
            {
                if (ForceData == null)
                    return damage;
                else
                    return damage + (ForceData.Force * .1f);
            }
        }
        public float WeakpointMultiplier { get => weakpointMultiplier; private set => weakpointMultiplier = value; }
        public float HealthEffectiveness { get => healthEffectiveness; private set => healthEffectiveness = value; }
        public float ArmorEffectiveness { get => armorEffectiveness; private set => armorEffectiveness = value; }
        public float SheildEffectiveness { get => sheildEffectiveness; private set => sheildEffectiveness = value; }

        public bool IgnoreArmor { get => ignoreArmor; private set => ignoreArmor = value; }
        public bool IgnoreShield { get => ignoreShield; private set => ignoreShield = value; }

        [SerializeField]
        private List<Tag> tags = new List<Tag>();
        public List<Tag> Tags { get => tags; private set => tags = value; }

        /// <summary>
        /// If true, damage can hurt damager even if they have "cannot self damage" set to true
        /// </summary>
        public bool IsSelfDamage { get => isSelfDamage; private set => isSelfDamage = value; }

        [Title("DamageData", null, TitleAlignments.Centered), PropertyOrder(-1)]
        [SerializeField, BoxGroup("fields", false)]
        private float damage = 1;
        [SerializeField, BoxGroup("fields")]
        private float weakpointMultiplier = 1;
        [SerializeField, BoxGroup("fields")]
        private float healthEffectiveness = 1;
        [SerializeField, BoxGroup("fields")]
        private float armorEffectiveness = 1;
        [SerializeField, BoxGroup("fields")]
        private float sheildEffectiveness = 1;

        [SerializeField, HorizontalGroup("fields/Split", 0.225f, LabelWidth = 75), BoxGroup("fields")]
        private bool ignoreArmor = false;
        [SerializeField, HorizontalGroup("fields/Split"), BoxGroup("fields")]
        private bool ignoreShield = false;
        [SerializeField, HorizontalGroup("fields/Split"), BoxGroup("fields"), Tooltip("If true, damage can hurt damager even if they have 'cannot self damage' set to true")]
        private bool isSelfDamage = false;


        public DamageData(IDamager source, ForceData forceData, List<Tag> tags = null, List<Tag> sourceTags = null, bool isSelfDamage = false)
        {
            Source = source;
            if (source != null)
                Damage = source.Damage;
            ForceData = forceData;
            WeakpointMultiplier = 1.5f;
            HealthEffectiveness = 1;
            ArmorEffectiveness = 1;
            SheildEffectiveness = 1;
            IgnoreArmor = false;
            IgnoreShield = false;
            SourceTags = new List<Tag>();
            if (sourceTags != null)
                SourceTags.AddRange(sourceTags);
            if (this.tags == null)
                this.tags = new List<Tag>();
            if (tags != null)
                Tags.AddRange(tags);
            IsSelfDamage = isSelfDamage;
        }

        public DamageData(IDamager source, ForceData forceData, float weakpointMultiplier, float healthEffectiveness, float armorEffectiveness, float sheildEffectiveness, bool ignoreArmor, bool ignoreShield,
            List<Tag> tags = null, List<Tag> sourceTags = null, bool isSelfDamage = false)
        {
            Source = source;
            Damage = source.Damage;
            ForceData = forceData;
            WeakpointMultiplier = weakpointMultiplier;
            HealthEffectiveness = healthEffectiveness;
            ArmorEffectiveness = armorEffectiveness;
            SheildEffectiveness = sheildEffectiveness;
            IgnoreArmor = ignoreArmor;
            IgnoreShield = ignoreShield;
            SourceTags = new List<Tag>();
            if (sourceTags != null)
                SourceTags.AddRange(sourceTags);
            if (this.tags == null)
                this.tags = new List<Tag>();
            if (tags != null)
                Tags.AddRange(tags);
            IsSelfDamage = isSelfDamage;
        }

        public void SetForceData(ForceData forceData)
        {
            ForceData = forceData;
        }

        public DamageData()
        {

        }

        public void SetDamage(float value)
        {
            Damage = value;

            if (Damage < 0)
                Damage = 0;
        }

        public void AddTags(List<Tag> tags)
        {
            Tags.AddRange(tags);
        }

        public void AddTag(Tag tag)
        {
            Tags.Add(tag);
        }

        public void RemoveTag(Tag tag)
        {
            if (Tags.Contains(tag))
                Tags.Remove(tag);
        }

        internal void SetWeakpointMultiplier(float weakpointMult)
        {
            weakpointMultiplier = weakpointMult;

            if (weakpointMultiplier < 1)
                weakpointMultiplier = 1;
        }

        public void ChangeSource(IDamager source, List<Tag> sourceTags = null)
        {
            Source = source;
            SourceTags = new List<Tag>();
            if (sourceTags != null)
                SourceTags.AddRange(sourceTags);
        }

        [SerializeField, HorizontalGroup("fields/Split"), BoxGroup("fields"), Button()]
        private void Reset()
        {
            damage = 1;
            weakpointMultiplier = 1.5f;
            healthEffectiveness = 1;
            armorEffectiveness = 1;
            sheildEffectiveness = 1;
            ignoreArmor = false;
            ignoreShield = false;
        }

        internal DamageData GetShallowCopy()
        {
            return (DamageData)MemberwiseClone();
        }
    }
}