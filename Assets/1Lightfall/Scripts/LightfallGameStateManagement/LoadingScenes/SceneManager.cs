using Michsky.MUIP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace MBS.Lightfall
{
    public class SceneManager : SingletonMonobehavior<SceneManager>
    {
        public string loadingSceneName;
        public UnityEngine.UI.Image SceneTransitionFader;
        public float minimumSceneLoadProgressBarInterpolationSpeed;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            if (SceneTransitionFader != null)
            {
                SceneTransitionFader.CrossFadeAlpha(0, 0, true);
                SceneTransitionFader.gameObject.SetActive(true);
            }
            if (minimumSceneLoadProgressBarInterpolationSpeed <= 0)
                minimumSceneLoadProgressBarInterpolationSpeed = .01f;
        }

        protected override void OnAutoCreate()
        {
            base.OnAutoCreate();
            loadingSceneName = "LoadingScene";
        }


        public void LoadScene(string sceneName)
        {
            //load the loading scene
            StartCoroutine(LoadLoadingScene(loadingSceneName, sceneName, true));

        }

        public void LoadSceneNoContinueButton(string sceneName)
        {
            StartCoroutine(LoadLoadingScene(loadingSceneName, sceneName, false));

        }

        public void LoadSceneNoLoadingScene(string sceneName)
        {
            StartCoroutine(LoadSceneNoLoading(sceneName));
        }

        IEnumerator LoadLoadingScene(string loadingSceneName, string queriedSceneName, bool holdWhenFinished)
        {


            AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(loadingSceneName);
            operation.completed += (AsyncOp) => { StartCoroutine(loadSceneAsync(queriedSceneName, LoadingScene.Instance.progressBar, 1f, holdWhenFinished)); };
            operation.allowSceneActivation = false;
            StartCoroutine(SceneTransition(operation));

            while (!operation.isDone)
            {

                yield return null;
            }

        }

        IEnumerator loadSceneAsync(string sceneName, Michsky.MUIP.ProgressBar progressBar, float waitSeconds, bool holdWhenLoadingFinished)
        {
            yield return new WaitForSeconds(waitSeconds);
            AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            ButtonManager continueButton = LoadingScene.Instance.continueButton;
            if (holdWhenLoadingFinished)
                continueButton.onClick.AddListener(() => { StartCoroutine(SceneTransition(operation)); });


            operation.allowSceneActivation = false;
            float interpolatedValue = 0;

            while (!operation.isDone)
            {
                float progressValue = Mathf.Clamp01(operation.progress / 0.9f) * 100;
                interpolatedValue = Mathf.MoveTowards(interpolatedValue, progressValue, Time.fixedUnscaledDeltaTime * 10 * (1 / minimumSceneLoadProgressBarInterpolationSpeed));

                progressBar.currentPercent = interpolatedValue;
                progressBar.UpdateUI();

                if (interpolatedValue >= 100)
                {
                    if (!holdWhenLoadingFinished)
                        StartCoroutine(SceneTransition(operation));
                    else
                    {
                        continueButton.gameObject.SetActive(true);
                        continueButton.UpdateUI();
                    }
                }

                yield return null;
            }




        }

        IEnumerator SceneTransition(AsyncOperation loadedSceneOperation)
        {
            if (SceneTransitionFader == null)
            {
                loadedSceneOperation.allowSceneActivation = true;
                yield break;
            }
            SceneTransitionFader.CrossFadeAlpha(1, .5f, true);
            yield return new WaitForSeconds(.5f);

            while (loadedSceneOperation.progress < .9f)
            {
                yield return null;
            }

            loadedSceneOperation.allowSceneActivation = true;
            yield return new WaitForSeconds(.25f);
            SceneTransitionFader.CrossFadeAlpha(0, .5f, true);


        }

        IEnumerator LoadSceneNoLoading(string sceneName)
        {
            AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;
            StartCoroutine(SceneTransition(operation));
            yield return null;
        }
    }
}
