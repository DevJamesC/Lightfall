using BehaviorDesigner.Runtime.Tasks;
using Opsive.Shared.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MBS.Lightfall
{
    public class EventRegisterer : MonoBehaviour
    {
        public string EventName;


        public UnityEvent actions;

        // Start is called before the first frame update
        void Start()
        {
            EventHandler.RegisterEvent(EventName, () => { actions.Invoke(); });
        }
    }
}
