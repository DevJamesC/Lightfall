using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ForceSystem
{
    [Serializable]
    public class ForceData : IShallowCloneable<ForceData>
    {
        public IForcer Source { get; private set; }

        [SerializeField]
        private float force;

        [SerializeField]
        private ForceType forceType;

        [SerializeField]
        private bool ignoreShield;

        [SerializeField, ShowIf("@forceType==ForceType.Explosive")]
        private float explosiveRadius;

        [SerializeField, ShowIf("@forceType==ForceType.Explosive")]
        private float explosiveUpwardsModifier;

        public float ExplosiveRadius { get => explosiveRadius; set => explosiveRadius = value; }

        public bool IgnoreShield { get => ignoreShield; }

        /// <summary>
        /// Negative force is implosive force, or pulling towards the pointOfForce
        /// </summary>
        public float Force { get => force; private set => force = value; }

        private Vector3 pointOfForce;
        public Vector3 PointOfForce
        {
            get
            {
                if (PointOfForceTransform == null)
                    return pointOfForce;
                else
                    return pointOfForceTransform.position;
            }
            private set => pointOfForce = value;
        }

        private Transform pointOfForceTransform;
        public Transform PointOfForceTransform { get => pointOfForceTransform; private set => pointOfForceTransform = value; }

        public ForceData(IForcer source)
        {
            Source = source;
            if (source != null)
            {
                Force = source.Force;
                PointOfForce = source.gameObject.transform.position;
            }
        }

        public ForceData(IForcer source, int force, Vector3 pointOfForce)
        {
            Source = source;
            Force = force;
            PointOfForce = pointOfForce;
        }
        public ForceData(IForcer source, int force, Transform pointOfForceTransform)
        {
            Source = source;
            Force = force;
            PointOfForceTransform = pointOfForceTransform;
        }
        public ForceData()
        {

        }

        public void SetForce(float value)
        {
            Force = value;
        }



        public void ChangeSource(IForcer source)
        {
            Source = source;
        }


        public ForceData GetShallowCopy()
        {
            return (ForceData)MemberwiseClone();
        }
        public ForceData GetShallowCopy(IForcer source, Vector3 pointOfHit)
        {
            ForceData forceData = (ForceData)MemberwiseClone();
            forceData.SetPointOfForce(pointOfHit);
            forceData.ChangeSource(source);
            return forceData;
        }
        public ForceData GetShallowCopy(IForcer source, Transform pointOfHit)
        {
            ForceData forceData = (ForceData)MemberwiseClone();
            forceData.SetPointOfForce(pointOfHit);
            forceData.ChangeSource(source);
            return forceData;
        }
        public void ApplyForceToRigidbody(Rigidbody rigidbody)
        {
            ApplyForceToRigidbody(rigidbody, explosiveRadius, explosiveUpwardsModifier);
        }

        public void ApplyForceToRigidbody(Rigidbody rigidbody, float explosiveRadius = 0, float upwardsModifierForExplosions = 0)
        {
            if (Force == 0)
                return;

            if (forceType == ForceType.Explosive)
            {
                ApplyExplosiveForceToRigidbody(rigidbody, explosiveRadius, upwardsModifierForExplosions);
                return;
            }

            if (pointOfForceTransform != null || pointOfForce != Vector3.zero)
            {
                ApplyForceWithTorqueToRigidbody(rigidbody, PointOfForce);
                return;
            }

            ApplyStandardForceToRigidbody(rigidbody);
        }

        private void ApplyStandardForceToRigidbody(Rigidbody rigidbody)
        {

            if (rigidbody == null)
                return;

            Vector3 forceToAdd = PointOfForce * Force;

            switch (forceType)
            {
                case ForceType.AccelerateWithMass:
                    rigidbody.AddForce(forceToAdd, ForceMode.Force);
                    break;
                case ForceType.AccelerateIgnoreMass:
                    rigidbody.AddForce(forceToAdd, ForceMode.Acceleration);
                    break;
                case ForceType.ImpulseWithMass:
                    rigidbody.AddForce(forceToAdd, ForceMode.Impulse);
                    break;
                case ForceType.ImpulseIgnoreMass:
                    rigidbody.AddForce(forceToAdd, ForceMode.VelocityChange);
                    break;
            }
        }

        public void SetPointOfForce(Vector3 hitPoint)
        {
            pointOfForce = hitPoint;
        }
        public void SetPointOfForce(Transform transform)
        {
            pointOfForceTransform = transform;
        }

        private void ApplyForceWithTorqueToRigidbody(Rigidbody rigidbody, Vector3 hitPoint)
        {
            if (rigidbody == null)
                return;

            Vector3 forceToAdd = PointOfForce * Force;

            switch (forceType)
            {
                case ForceType.AccelerateWithMass:
                    rigidbody.AddForceAtPosition(forceToAdd, hitPoint, ForceMode.Force);
                    break;
                case ForceType.AccelerateIgnoreMass:
                    rigidbody.AddForceAtPosition(forceToAdd, hitPoint, ForceMode.Acceleration);
                    break;
                case ForceType.ImpulseWithMass:
                    rigidbody.AddForceAtPosition(forceToAdd, hitPoint, ForceMode.Impulse);
                    break;
                case ForceType.ImpulseIgnoreMass:
                    rigidbody.AddForceAtPosition(forceToAdd, hitPoint, ForceMode.VelocityChange);
                    break;
            }
        }

        private void ApplyExplosiveForceToRigidbody(Rigidbody rigidbody, float radius, float upwardsModifier)
        {
            if (rigidbody == null)
                return;

            switch (forceType)
            {
                case ForceType.AccelerateWithMass:
                    rigidbody.AddExplosionForce(Force, PointOfForce, radius, upwardsModifier, ForceMode.Force);
                    break;
                case ForceType.AccelerateIgnoreMass:
                    rigidbody.AddExplosionForce(Force, PointOfForce, radius, upwardsModifier, ForceMode.Acceleration);
                    break;
                case ForceType.ImpulseWithMass:
                    rigidbody.AddExplosionForce(Force, PointOfForce, radius, upwardsModifier, ForceMode.Impulse);
                    break;
                case ForceType.ImpulseIgnoreMass:
                    rigidbody.AddExplosionForce(Force, PointOfForce, radius, upwardsModifier, ForceMode.VelocityChange);
                    break;
                case ForceType.Explosive:
                    rigidbody.AddExplosionForce(Force, PointOfForce, radius, upwardsModifier, ForceMode.Impulse);
                    break;
            }
        }

    }
    [Serializable]
    public enum ForceType
    {
        ImpulseWithMass,
        ImpulseIgnoreMass,
        AccelerateWithMass,
        AccelerateIgnoreMass,
        Explosive

    }
}
