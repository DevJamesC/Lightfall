using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Misc
{
    public class InstanciateObjectGenericFX : GenericFX
    {
        public GameObject prefabToInstanciate;
        [SerializeField]
        private InstantiateSetting option;


        public override void Activate(GameObject source, List<GameObject> targets = null)
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
                obj.transform.localScale = source.transform.localScale;
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