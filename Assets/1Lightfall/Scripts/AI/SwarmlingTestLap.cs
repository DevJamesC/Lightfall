using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Character.Abilities.Items;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    public class SwarmlingTestLap : MonoBehaviour
    {

        public Transform startTransform;
        public Transform endTransform;
        public Transform leapTransform;
        private Transform target;
        private int progress;
        private float delayUntilNextAction;
        private UltimateCharacterLocomotion uccLocomotion;
        private SpeedChange changeSpeedAbility;
        private Jump jumpAbility;
        private Use UseItemAbility;
        private bool shouldSprint;
        private bool isWaiting;
        private Animator animator;
        IAstarAI ai;



        void OnEnable()
        {
            ai = GetComponent<IAstarAI>();
            // Update the destination right before searching for a path as well.
            // This is enough in theory, but this script will also update the destination every
            // frame as the destination is used for debugging and may be used for other things by other
            // scripts as well. So it makes sense that it is up to date every frame.
            if (ai != null) ai.onSearchPath += Update;
            target = startTransform;

            uccLocomotion = GetComponent<UltimateCharacterLocomotion>();
            changeSpeedAbility = uccLocomotion.GetAbility<SpeedChange>();
            jumpAbility = uccLocomotion.GetAbility<Jump>();
            UseItemAbility = uccLocomotion.GetItemAbility<Use>();
            animator = GetComponentInChildren<Animator>();
        }

        void OnDisable()
        {
            if (ai != null) ai.onSearchPath -= Update;
        }

        /// <summary>Updates the AI's destination every frame</summary>
        void Update()
        {
            if (target == null || ai == null)
                return;

            if (shouldSprint && !changeSpeedAbility.IsActive)
                changeSpeedAbility.StartAbility();

            if (!shouldSprint && changeSpeedAbility.IsActive)
                changeSpeedAbility.StopAbility();

            if (delayUntilNextAction > 0)
            {
                delayUntilNextAction -= Time.deltaTime;
                return;
            }

            UseItemAbility.StopAbility();
            //animator.SetBool("Attack", false);
            if (jumpAbility.IsActive)
                jumpAbility.StopAbility(true, false);
            //animator.SetBool("Leap", false);

            if (ai.reachedDestination)
            {
                if (!isWaiting)
                {
                    isWaiting = true;
                    delayUntilNextAction = 2;
                    switch (progress)
                    {
                        case 0: UseItemAbility.StartAbility(); return;
                        case 1: jumpAbility.StartAbility(); return;
                        case 2: UseItemAbility.StartAbility(); return;
                    }
                }
                isWaiting = false;

                progress++;
                if (progress > 2)
                    progress = 0;

                switch (progress)
                {
                    case 0: target = startTransform; shouldSprint = true; break;
                    case 1: target = leapTransform; shouldSprint = false; break;
                    case 2: target = endTransform; break;
                }
            }

            ai.destination = target.position;

        }
    }
}