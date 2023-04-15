using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MBS.Lightfall
{
    public class QuitApplication : MonoBehaviour
    {
        public void QuiteApp()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
