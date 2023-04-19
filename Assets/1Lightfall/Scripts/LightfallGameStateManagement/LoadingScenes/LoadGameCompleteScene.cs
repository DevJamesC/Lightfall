using MBS.Lightfall;
using Opsive.Shared.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadGameCompleteScene : MonoBehaviour
{
    public string GameCompleteSceneName;

    private void OnGameComplete()
    {
        SceneManager.Instance.LoadSceneNoLoadingScene(GameCompleteSceneName);
    }

    private void OnEnable()
    {
        EventHandler.RegisterEvent("GameComplete", OnGameComplete);
    }

    private void OnDisable()
    {
        EventHandler.UnregisterEvent("GameComplete", OnGameComplete);

    }
}
