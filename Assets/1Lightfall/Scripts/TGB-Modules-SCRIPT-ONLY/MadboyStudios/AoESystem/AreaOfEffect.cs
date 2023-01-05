using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MBS.Misc;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MBS.AoeSystem
{
    /// <summary>
    /// Used for instant AoE effects like explosions or sprays
    /// </summary>
    public class AreaOfEffect : AreaOfEffectBase
    {
        [SerializeField]
        private LayerMask collisionLayers;
        [SerializeField, Tooltip("For instant AoE, this is the time till the secondary radius is triggered. For duration AoE, this the time till the radius reaches its max radius (the secondary radius)")]
        private float areaExpansionRate = .25f;
        [SerializeField]
        private bool instantAoE;
        [SerializeField, ShowIf("@instantAoE==false")]
        private float duration;
        [SerializeField]
        private RadiusType radiusType = RadiusType.Sphere;
        //offset for all radius type
        [SerializeField]
        private Vector3 offset;
        //rotation for capsule, box, and particle
        [SerializeField, ShowIf("@radiusType != RadiusType.Sphere")]
        private Vector3 rotation;
        //radius for sphere and capsule
        [SerializeField, ShowIf("@radiusType != RadiusType.Box")]
        private float primaryRadius;
        //length for capsule
        [SerializeField, ShowIf("@radiusType == RadiusType.Capsule")]
        private float height;
        //Width & height for box
        [SerializeField, ShowIf("@radiusType == RadiusType.Box")]
        private Vector3 bounds = Vector3.one;
        //secondary radius for all radius types
        [SerializeField, Tooltip("The percent of the primary radius which extends a little damage/ effects outside the stated range."), ShowIf("@radiusType != RadiusType.Particle")]
        private float secondaryRadiusPercent = 30;
        [SerializeField]
        private EndAction endAction = EndAction.disable;

        [SerializeReference]
        private List<GenericFX> PrimaryRadiusFX;
        //[SerializeReference]
        //private List<GenericFX> SecondaryRadiusFX;
        //[SerializeReference]
        //private List<GenericFX> OutlyingRadiusFX;

        public float PrimaryRadius { get => primaryRadius; }
        public float SecondaryRadius { get { return primaryRadius * (1 + (secondaryRadiusPercent / 100)); } }


        //AoE damage should be multiplied by weakpoint damage if it only intersects a weakpoint.
        //AoE should deal normal damage if they intersect any normal collider... but should activate any weakpoint hit effects (like exploding fuel tanks or whatever)

        //should be able to handle explosions, implosions, and sustained AoE effects

        private int expansionProgress;
        private float timeTillNextExpansionProgress;

        private List<GameObject> gameObjectsHit;
        private List<Collider> collidersHit;
        private ParticleSystem particleColliderSystem;
        private List<ParticleCollisionEvent> collisionEvents;
        private int framesTillObjectsHitCleared;//used for particles only

        private bool initalized;

        //used for duration only
        private SphereCollider sphereCollider;
        private CapsuleCollider capsuleCollider;
        private BoxCollider boxCollider;
        private float currentExpansionTime;

        private void Awake()
        {
            gameObjectsHit = new List<GameObject>();
            collidersHit = new List<Collider>();
            switch (radiusType)
            {
                case RadiusType.Sphere: sphereCollider = GetComponent<SphereCollider>(); break;
                case RadiusType.Capsule: capsuleCollider = GetComponent<CapsuleCollider>(); break;
                case RadiusType.Box: boxCollider = GetComponent<BoxCollider>(); break;
                case RadiusType.Particle: particleColliderSystem = GetComponent<ParticleSystem>(); break;
            }
            collisionEvents = new List<ParticleCollisionEvent>();
            initalized = false;
        }

        private void OnEnable()
        {
            if (!initalized)
                Setup(primaryRadius);
        }

        private void OnDisable()
        {
            gameObjectsHit.Clear();
            collidersHit.Clear();
            initalized = false;
        }

        public override void Setup(float radius)
        {
            timeTillNextExpansionProgress = instantAoE ? 0 : duration;
            expansionProgress = 0;
            framesTillObjectsHitCleared = 5;
            currentExpansionTime = 0;
            primaryRadius = radius;
            initalized = true;

            //if not instant AoE, cast to see what things are already in our trigger
            if (!instantAoE)
            {
                List<Collider> collidersInRadius = RadiusCast(transform.position + (offset / 2), Quaternion.Euler(rotation), false);

                foreach (var target in collidersInRadius)
                {
                    if (gameObjectsHit.Contains(target.gameObject.transform.root.gameObject))
                        continue;

                    if (!collidersHit.Contains(target))
                        collidersHit.Add(target);

                    gameObjectsHit.Add(target.gameObject.transform.root.gameObject);
                }
            }
            else //resize colliders
            {
                switch (radiusType)
                {
                    case RadiusType.Sphere:
                        sphereCollider.radius = primaryRadius / 2;
                        sphereCollider.center = offset;
                        break;
                    case RadiusType.Capsule:
                        capsuleCollider.radius = primaryRadius / 2;
                        capsuleCollider.height = height;
                        capsuleCollider.center = offset;
                        break;
                    case RadiusType.Box:
                        boxCollider.size = bounds;
                        boxCollider.center = offset;
                        break;
                }
            }
        }

        private void Update()
        {
            if (instantAoE)
            {
                if (timeTillNextExpansionProgress > 0)
                {
                    timeTillNextExpansionProgress -= Time.deltaTime;
                    return;
                }

                switch (expansionProgress)
                {
                    case 0:
                        HandleTargetsInPrimaryArea();
                        timeTillNextExpansionProgress = areaExpansionRate / 2;
                        expansionProgress++;
                        return;
                    case 1:
                        HandleTargetsInSecondaryArea();
                        timeTillNextExpansionProgress = areaExpansionRate / 2;
                        expansionProgress++;
                        return;
                    case 2:
                        //foreach (var fx in OutlyingRadiusFX)
                        //{
                        //    fx.Activate(gameObject);
                        //}
                        expansionProgress++;
                        break;
                }
            }
            else
            {
                currentExpansionTime += Time.deltaTime;
                float secondaryMult = (1 + (secondaryRadiusPercent / 100));
                float percent = Mathf.Clamp01(currentExpansionTime / areaExpansionRate);
                //expand the radius (TODO: fix "cannot detect new obejcts in radius after resizing if they don't move". may not be too much of an issue... we will see)
                switch (radiusType)
                {
                    case RadiusType.Sphere:
                        sphereCollider.radius = Mathf.Lerp(primaryRadius / 2, (primaryRadius / 2) * secondaryMult, percent);
                        break;
                    case RadiusType.Capsule:
                        capsuleCollider.radius = Mathf.Lerp(primaryRadius / 2, (primaryRadius / 2) * secondaryMult, percent);
                        capsuleCollider.height = Mathf.Lerp(height, height * secondaryMult, percent);
                        break;
                    case RadiusType.Box:
                        boxCollider.size = Vector3.Lerp(bounds, bounds * secondaryMult, percent);
                        break;
                }

                //handle targets in the radius continuously (only has a primary radius, no secondary radius)
                expansionProgress = 1;
                HandleTargetsContinuously();
                if (timeTillNextExpansionProgress > 0)
                {
                    timeTillNextExpansionProgress -= Time.deltaTime;
                    return;
                }
                expansionProgress = 2;
            }


            EndAreaOfEffect();

        }

        private void FixedUpdate()
        {
            //TODO: we will need a more consistant way to detect that gameobjects have left the particle radius... maybe have a timer for each entry, where if an object is not hit for 10 frames then remove it?
            if (framesTillObjectsHitCleared > 0)
            {
                framesTillObjectsHitCleared--;
                return;
            }

            if (radiusType == RadiusType.Particle)
            {
                gameObjectsHit.Clear();
                collidersHit.Clear();
            }
            framesTillObjectsHitCleared = 5;
        }

        private void HandleTargetsInPrimaryArea()
        {
            List<Collider> collidersInPrimaryRadius = RadiusCast(transform.position + (offset / 2), Quaternion.Euler(rotation), false);

            foreach (var target in collidersInPrimaryRadius)
            {
                if (gameObjectsHit.Contains(target.gameObject.transform.root.gameObject))
                    continue;

                if (!collidersHit.Contains(target))
                    collidersHit.Add(target);

                gameObjectsHit.Add(target.gameObject.transform.root.gameObject);
                InvokeInsidePrimaryRadius(target);
            }

            foreach (var fx in PrimaryRadiusFX)
            {
                if (fx == null)
                {
                    Debug.LogWarning($"The FX on {gameObject.name} is null. Make sure that code changes have not cleared any prefab arguments");
                    continue;
                }

                fx.Activate(gameObject, gameObjectsHit);
            }

        }

        private void HandleTargetsInSecondaryArea()
        {
            List<GameObject> gameObjectsHitInSecondaryArea = new List<GameObject>();
            List<Collider> collidersInSecondaryRadius = RadiusCast(transform.position + (offset / 2), Quaternion.Euler(rotation), true);

            foreach (var target in collidersInSecondaryRadius)
            {
                if (gameObjectsHit.Contains(target.gameObject.transform.root.gameObject))
                    continue;

                collidersHit.Add(target);
                gameObjectsHitInSecondaryArea.Add(target.gameObject.transform.root.gameObject);
                gameObjectsHit.Add(target.gameObject.transform.root.gameObject);
                InvokeInsideSecondaryRadius(target);
            }

            //foreach (var fx in SecondaryRadiusFX)
            //{
            //    fx.Activate(gameObject, gameObjectsHitInSecondaryArea);
            //}
        }

        private void HandleTargetsContinuously()
        {
            //handle all targets which have been "caught" in a collider
            List<GameObject> objsHitThisFrame = new List<GameObject>();

            foreach (var target in collidersHit)
            {
                if (objsHitThisFrame.Contains(target.gameObject.transform.root.gameObject))
                    continue;

                objsHitThisFrame.Add(target.gameObject.transform.root.gameObject);

                InvokeInsidePrimaryRadius(target);
            }
        }

        private List<Collider> RadiusCast(Vector3 pos, Quaternion rot, bool useSecondaryRadius)
        {
            //Vector3 pos = transform.position + offset;
            //Quaternion rot = Quaternion.Euler(rotation);
            float secondaryMult = useSecondaryRadius ? (1 + (secondaryRadiusPercent / 100)) : 1;
            List<Collider> collidersInSecondaryRadius = new List<Collider>();
            switch (radiusType)
            {
                case RadiusType.Sphere:
                    collidersInSecondaryRadius = Physics.OverlapSphere(pos, (primaryRadius / 2) * secondaryMult, collisionLayers).ToList();
                    break;
                case RadiusType.Capsule:
                    Vector3 pos1 = transform.position;
                    Vector3 pos2 = transform.position;
                    pos1.y += ((height * secondaryMult) / 2);
                    pos2.y -= ((height * secondaryMult) / 2);
                    pos1 = rot * pos1;
                    pos2 = rot * pos2;
                    pos1 += offset;
                    pos2 += offset;
                    collidersInSecondaryRadius = Physics.OverlapCapsule(pos1, pos2, (primaryRadius / 2) * secondaryMult, collisionLayers).ToList();
                    break;
                case RadiusType.Box:
                    collidersInSecondaryRadius = Physics.OverlapBox(pos, (bounds / 2) * secondaryMult, rot, collisionLayers).ToList();
                    break;
            }

            return collidersInSecondaryRadius;
        }

        private void OnParticleCollision(GameObject other)
        {
            if (radiusType != RadiusType.Particle)
                return;

            //List<GameObject> hitRecently = new List<GameObject>();


            int numCollisionEvents = particleColliderSystem.GetCollisionEvents(other, collisionEvents);
            //do OnPrimaryCollision stuff
            int i = 0;
            //add any colliders and gameobjects to our "hit" list
            while (i < numCollisionEvents)
            {
                if (gameObjectsHit.Contains(collisionEvents[i].colliderComponent.gameObject.transform.root.gameObject))
                    return;

                if (!collidersHit.Contains(collisionEvents[i].colliderComponent))
                    collidersHit.Add(collisionEvents[i].colliderComponent as Collider);

                gameObjectsHit.Add(collisionEvents[i].colliderComponent.gameObject.transform.root.gameObject);
                //hitRecently.Add(collisionEvents[i].colliderComponent.gameObject.transform.root.gameObject);

                i++;
            }

            //add any objects not hit to the notHitRecently list
            //foreach (var obj in gameObjectsHit)
            //{
            //    if (!hitRecently.Contains(obj))
            //    {
            //        if (!objsNotHitByParticlesRecently.Contains(obj))
            //            objsNotHitByParticlesRecently.Add(obj);
            //    }
            //}
        }

        private void OnTriggerEnter(Collider other)
        {
            if (gameObjectsHit.Contains(other.gameObject.transform.root.gameObject))
                return;

            if (!collidersHit.Contains(other))
                collidersHit.Add(other);

            gameObjectsHit.Add(other.gameObject.transform.root.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!gameObjectsHit.Contains(other.gameObject.transform.root.gameObject))
                return;

            if (collidersHit.Contains(other))
                collidersHit.Remove(other);

            gameObjectsHit.Remove(other.gameObject.transform.root.gameObject);
        }

        private void EndAreaOfEffect()
        {
            switch (endAction)
            {
                case EndAction.destroy:
                    Destroy(gameObject);
                    break;
                case EndAction.disable:
                    gameObject.SetActive(false);
                    break;
                case EndAction.none:
                    break;
            }

        }

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            float minimumValue = .001f;

            if (primaryRadius <= 0)
                primaryRadius = minimumValue;

            if (bounds.x <= 0)
                bounds.x = minimumValue;
            if (bounds.y <= 0)
                bounds.y = minimumValue;
            if (bounds.z <= 0)
                bounds.z = minimumValue;

            if (radiusType == RadiusType.Capsule)
            {
                if (height < primaryRadius)
                    height = primaryRadius;
            }

            if (radiusType == RadiusType.Particle)
            {
                ParticleSystem particleSystem = GetComponent<ParticleSystem>();
                if (particleSystem == null)
                {
                    particleSystem = gameObject.AddComponent<ParticleSystem>();
                }
                //make sure particle system has colliders on
                ParticleSystem.CollisionModule collisionMod = particleSystem.collision;
                collisionMod.enabled = true;
                collisionMod.enableDynamicColliders = true;
                collisionMod.type = ParticleSystemCollisionType.World;
                collisionMod.collidesWith = collisionLayers;

                ParticleSystem.MainModule mainMod = particleSystem.main;
                mainMod.playOnAwake = true;
                if (instantAoE)
                {
                    Debug.Log("Instant AoE cannot have a particle system collision detetion type.");
                    radiusType = RadiusType.Sphere;
                    mainMod.playOnAwake = false;
                    particleSystem.Stop();
                }

            }
            else
            {
                ParticleSystem particleSystem = GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    ParticleSystem.MainModule mainMod = particleSystem.main;
                    mainMod.playOnAwake = false;
                    particleSystem.Stop();
                }
            }

            if (!instantAoE)
            {
                if (duration < areaExpansionRate)
                    duration = areaExpansionRate;
                //also need to add/ remove trigger components for primary radius
                transform.rotation = Quaternion.Euler(rotation);
                SphereCollider sphereCollider;
                CapsuleCollider capsuleCollider;
                BoxCollider boxCollider;
                switch (radiusType)
                {
                    case RadiusType.Sphere:
                        boxCollider = GetComponent<BoxCollider>();
                        if (boxCollider != null)
                            DestroyImmediate(boxCollider);
                        capsuleCollider = GetComponent<CapsuleCollider>();
                        if (capsuleCollider != null)
                            DestroyImmediate(capsuleCollider);

                        sphereCollider = GetComponent<SphereCollider>();
                        if (sphereCollider == null)
                            sphereCollider = gameObject.AddComponent<SphereCollider>();
                        sphereCollider.isTrigger = true;
                        sphereCollider.radius = primaryRadius / 2;
                        sphereCollider.center = offset;
                        break;
                    case RadiusType.Capsule:
                        boxCollider = GetComponent<BoxCollider>();
                        if (boxCollider != null)
                            DestroyImmediate(boxCollider);
                        sphereCollider = GetComponent<SphereCollider>();
                        if (sphereCollider != null)
                            DestroyImmediate(sphereCollider);

                        capsuleCollider = GetComponent<CapsuleCollider>();
                        if (capsuleCollider == null)
                            capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
                        capsuleCollider.isTrigger = true;
                        capsuleCollider.radius = primaryRadius / 2;
                        capsuleCollider.height = height;
                        capsuleCollider.center = offset;
                        break;
                    case RadiusType.Box:
                        capsuleCollider = GetComponent<CapsuleCollider>();
                        if (capsuleCollider != null)
                            DestroyImmediate(capsuleCollider);
                        sphereCollider = GetComponent<SphereCollider>();
                        if (sphereCollider != null)
                            DestroyImmediate(sphereCollider);

                        boxCollider = GetComponent<BoxCollider>();
                        if (boxCollider == null)
                            boxCollider = gameObject.AddComponent<BoxCollider>();
                        boxCollider.isTrigger = true;
                        boxCollider.size = bounds;
                        boxCollider.center = offset;
                        break;
                }
            }
            else
            {
                SphereCollider sphereCollider;
                CapsuleCollider capsuleCollider;
                BoxCollider boxCollider;
                capsuleCollider = GetComponent<CapsuleCollider>();
                if (capsuleCollider != null)
                    DestroyImmediate(capsuleCollider);
                sphereCollider = GetComponent<SphereCollider>();
                if (sphereCollider != null)
                    DestroyImmediate(sphereCollider);
                boxCollider = GetComponent<BoxCollider>();
                if (boxCollider != null)
                    DestroyImmediate(boxCollider);

                transform.rotation = Quaternion.identity;
            }

        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 pos = transform.position + (offset / 2);
            Quaternion rot = Quaternion.Euler(rotation);
            float secondaryMult = (1 + (secondaryRadiusPercent / 100));

            switch (radiusType)
            {
                case RadiusType.Sphere:
                    Gizmos.DrawWireSphere(pos, (primaryRadius / 2));
                    Gizmos.DrawWireSphere(pos, (primaryRadius / 2) * secondaryMult);
                    break;
                case RadiusType.Capsule:
#if UNITY_EDITOR
                    DrawWireCapsule(pos, rot, (primaryRadius / 2), height, Color.red);
                    DrawWireCapsule(pos, rot, (primaryRadius / 2) * secondaryMult, height * secondaryMult, Color.red);
#endif
                    //Gizmos.DrawWireSphere(pos, primaryRadius * secondaryMult);
                    break;
                case RadiusType.Box:
                    Gizmos.matrix = Matrix4x4.TRS(Vector3.zero, rot, Vector3.one);
                    Gizmos.DrawWireCube(pos, bounds);
                    Gizmos.DrawWireCube(pos, bounds * secondaryMult);
                    break;
            }
        }

        [Serializable]
        public enum EndAction
        {
            none,
            destroy,
            disable
        }

        [Serializable]
        public enum RadiusType
        {
            Sphere,
            Capsule,
            Box,
            Particle
        }

#if UNITY_EDITOR
        public static void DrawWireCapsule(Vector3 _pos, Quaternion _rot, float _radius, float _height, Color _color = default(Color))
        {
            if (_color != default(Color))
                Handles.color = _color;
            Matrix4x4 angleMatrix = Matrix4x4.TRS(_pos, _rot, Handles.matrix.lossyScale);
            using (new Handles.DrawingScope(angleMatrix))
            {
                var pointOffset = (_height - (_radius * 2)) / 2;

                //draw sideways
                Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, _radius);
                Handles.DrawLine(new Vector3(0, pointOffset, -_radius), new Vector3(0, -pointOffset, -_radius));
                Handles.DrawLine(new Vector3(0, pointOffset, _radius), new Vector3(0, -pointOffset, _radius));
                Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, _radius);
                //draw frontways
                Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, _radius);
                Handles.DrawLine(new Vector3(-_radius, pointOffset, 0), new Vector3(-_radius, -pointOffset, 0));
                Handles.DrawLine(new Vector3(_radius, pointOffset, 0), new Vector3(_radius, -pointOffset, 0));
                Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, _radius);
                //draw center
                Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, _radius);
                Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, _radius);

            }
        }
#endif
    }
}


