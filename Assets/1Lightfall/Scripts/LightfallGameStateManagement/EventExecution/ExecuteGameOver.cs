
using Opsive.Shared.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecuteGameOver : MonoBehaviour
{
    public bool GameOverOnZeroHealth = true;

    public void ExecuteGameOverEvent(Vector3 pos, Vector3 force, GameObject attacker)
    {
        EventHandler.ExecuteEvent("GameOver");
    }
    public void ExecuteGameOverEvent()
    {
        EventHandler.ExecuteEvent("GameOver");
    }

    private void OnEnable()
    {
        if (GameOverOnZeroHealth)
        {
            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(gameObject, "OnDeath", ExecuteGameOverEvent);
        }
    }

    private void OnDisable()
    {
        EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(gameObject, "OnDeath", ExecuteGameOverEvent);
    }
}
