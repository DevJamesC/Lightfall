using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    public class InstanciateObjectDetonationFXBase : DetonationFXBase
    {
        public GameObject prefabToInstanciate;
        [SerializeField]
        private InstantiateSetting option;
        [SerializeField, Tooltip("Will search the prefab for IOrigin. If found, will apply the source as the origin")]
        private bool applySourceObjectToPrefabs;
        public override void Activate(GameObject source, List<GameObject> targets, EffectWithIntensityData data, MajorEffects primerType, MajorEffects detonatorType)
        {
            List<GameObject> newObjects = new List<GameObject>();
            switch (option)
            {
                case InstantiateSetting.InstantiateAtSourceTransform:
                    newObjects.Add(GameObject.Instantiate(prefabToInstanciate, source.transform));
                    break;
                case InstantiateSetting.InstantiateAtSourcePosition:
                    newObjects.Add(GameObject.Instantiate(prefabToInstanciate, source.transform.position, source.transform.rotation));
                    break;
                case InstantiateSetting.InstantiateAtTargetTransform:
                    foreach (GameObject target in targets)
                    {
                        newObjects.Add(GameObject.Instantiate(prefabToInstanciate, target.transform));
                    }
                    break;
                case InstantiateSetting.InstantiateAtTargetPosition:
                    foreach (GameObject target in targets)
                    {
                        newObjects.Add(GameObject.Instantiate(prefabToInstanciate, target.transform.position, target.transform.rotation));
                    }
                    break;
            }

            foreach (GameObject obj in newObjects)
            {
                Vector3 transformValues = obj.transform.localScale;
                transformValues *= data.DetonationSizeModifier;
                obj.transform.localScale = transformValues;
                if (applySourceObjectToPrefabs)
                {
                    IOrigin origin = obj.GetComponentInChildren<IOrigin>();
                    if (origin != null)
                        origin.SetOrigin(source);
                }
            }

        }

        [Serializable]
        private enum InstantiateSetting
        {
            InstantiateAtTargetTransform,
            InstantiateAtSourceTransform,
            InstantiateAtTargetPosition,
            InstantiateAtSourcePosition
        }
    }
}