using Michsky.MUIP;
using PixelCrushers.DialogueSystem.UnityGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    public class LoadingScene : SingletonMonobehavior<LoadingScene>
    {
       public ProgressBar progressBar;
        public ButtonManager continueButton;
    }
}