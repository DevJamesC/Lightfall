using Opsive.Shared.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecuteGameComplete : MonoBehaviour
{
    public void ExecuteGameCompleteEvent()
    {
        EventHandler.ExecuteEvent("GameComplete");
    }
}
