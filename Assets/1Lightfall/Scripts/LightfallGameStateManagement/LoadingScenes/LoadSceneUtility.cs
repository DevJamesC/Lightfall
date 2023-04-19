using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MBS.Lightfall
{
    public class LoadSceneUtility : MonoBehaviour
    {
        public void LoadScene(string sceneName)
        {
            SceneManager.Instance.LoadScene(sceneName);
        }

        public void LoadSceneNoContinueButton(string sceneName)
        {
            SceneManager.Instance.LoadSceneNoContinueButton(sceneName);
        }

        public void LoadSceneNoLoadingScene(string sceneName)
        {
            SceneManager.Instance.LoadSceneNoLoadingScene(sceneName);
        }

    }
}