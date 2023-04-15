using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatePreWaveTriggers : MonoBehaviour
{

    private List<GameObject> triggers;
    private void Start()
    {
        triggers = new List<GameObject>();
        foreach (Transform child in transform)
        {
            triggers.Add(child.gameObject);
            child.gameObject.SetActive(false);
        }
    }

    private void OnWavePreStart()
    {
        foreach (GameObject trigger in triggers)
        {
            trigger.SetActive(true);
        }
    }

    private void OnEnable()
    {
        Opsive.Shared.Events.EventHandler.RegisterEvent("OnWavePreStart", OnWavePreStart);
    }

    private void OnDisable()
    {
        Opsive.Shared.Events.EventHandler.UnregisterEvent("OnWavePreStart", OnWavePreStart);
    }
}
