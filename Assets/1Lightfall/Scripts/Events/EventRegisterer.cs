using BehaviorDesigner.Runtime.Tasks;
using Opsive.Shared.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MBS.Lightfall
{
    public class EventRegisterer : MonoBehaviour
    {
        public string EventName;

       [SerializeField] private ArgumentHandle argumentHandle;

        public UnityEvent actions;

        // Start is called before the first frame update
        void Start()
        {
            switch (argumentHandle)
            {
                case ArgumentHandle.None:
                    Opsive.Shared.Events.EventHandler.RegisterEvent(EventName, () => { actions.Invoke(); });
                    break;
                case ArgumentHandle.Integer:
                    Opsive.Shared.Events.EventHandler.RegisterEvent(EventName, (int integer) => { actions.Invoke(); });
                    break;
            }

            
        }

        [Serializable]
        private enum ArgumentHandle
        {
            None,
            Integer
        }
    }
}
