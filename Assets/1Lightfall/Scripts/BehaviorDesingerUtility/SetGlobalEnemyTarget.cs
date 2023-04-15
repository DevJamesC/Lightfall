using BehaviorDesigner.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace MBS.Lightfall
{
    public class SetGlobalEnemyTarget : MonoBehaviour
    {

        // Start is called before the first frame update
        void Start()
        {
            SetTarget(gameObject);
        }

        public void SetTarget(GameObject target)
        {
            SharedVariable sharedVar = GlobalVariables.Instance.GetVariable("GlobalTarget");
            sharedVar.SetValue(target);
        }
    }
}
