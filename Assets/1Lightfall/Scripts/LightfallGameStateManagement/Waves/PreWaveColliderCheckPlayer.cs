using MBS.Lightfall;
using Opsive.Shared.Events;
using Opsive.UltimateCharacterController.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreWaveColliderCheckPlayer : CharacterMonitor
{
    private bool m_PreStartIsActive;
    [SerializeField] private float waitTimeUntilWaveStart = 1;

    private bool waitTimerActive;
    private float currentWaitCountdown;
    protected override void Start()
    {
        base.Start();
        m_PreStartIsActive = false;
        waitTimerActive = false;
    }

    private void Update()
    {
        if (!waitTimerActive)
            return;
        if (currentWaitCountdown <= 0)
        {
            Cancel();
            EventHandler.ExecuteEvent("OnWaveStart");
        }
        else
        {
            currentWaitCountdown -= Time.deltaTime;
        }


    }

    protected override void OnAttachCharacter(GameObject character)
    {
        base.OnAttachCharacter(character);
    }

    private void OnWavePreStart()
    {
        if (WaveManager.Instance.WaveIsActive)
            return;

        m_PreStartIsActive = true;
    }

    private void OnEnable()
    {
        EventHandler.RegisterEvent("OnWavePreStart", OnWavePreStart);
    }

    private void OnDisable()
    {
        EventHandler.UnregisterEvent("OnWavePreStart", OnWavePreStart);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!ValidatePlayerEnter(other))
        {
            Cancel();
            return;
        }
        EventHandler.ExecuteEvent("PlayerReadyForWaveStart");
        waitTimerActive = true;
        currentWaitCountdown = waitTimeUntilWaveStart;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!ValidatePlayerEnter(other))
        {
            Cancel();
            return;
        }

        Cancel();
        EventHandler.ExecuteEvent("OnWavePreStartFailed");

    }

    private bool ValidatePlayerEnter(Collider other)
    {
        if (!m_PreStartIsActive)
            return false;
        if (m_Character == null)
            return false;
        if (other.gameObject.transform.root.gameObject != m_Character)
            return false;
        if (WaveManager.Instance.WaveIsActive)
            return false;

        return true;
    }

    private void Cancel()
    {
        m_PreStartIsActive = false;
        waitTimerActive = false;
    }
}
