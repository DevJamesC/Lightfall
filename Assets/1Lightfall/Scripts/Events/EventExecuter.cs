using Opsive.Shared.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MBS.Lightfall
{
    public class EventExecuter : MonoBehaviour
    {
        public void ExecuteEvent(string eventName)
        {
            EventHandler.ExecuteEvent(eventName);
        }
    }
}
