using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    public class FXPlayAnimation : ModifierFXBase
    {
        [SerializeField]
        private string animationStateName;
        [SerializeField]
        private int layerIndex = 0;

        public override void Activate(ModifierEntry targetEntry)
        {
            Animator animator = targetEntry.Target.gameObject.GetComponentInChildren<Animator>();

            if (animator == null)
                return;

            animator.Play(animationStateName, layerIndex);



        }


    }
}
