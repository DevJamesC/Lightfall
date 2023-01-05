using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.AbilitySystem
{
    public class AbilityLoadoutPlayerInputForDemo : MonoBehaviour
    {
        [SerializeField]
        protected KeyCode Ability1Keycode = KeyCode.Q;
        [SerializeField]
        protected KeyCode Ability2Keycode = KeyCode.E;
        [SerializeField]
        protected KeyCode Ability3Keycode = KeyCode.C;
        [SerializeField]
        protected KeyCode Ability4Keycode = KeyCode.X;

        AbilityLoadout abilityLoadout;

        private void Awake()
        {
            abilityLoadout = GetComponent<AbilityLoadout>();
        }

        // Update is called once per frame
        void Update()
        {
            if (abilityLoadout == null)
                return;


            if (Input.GetKeyDown(Ability1Keycode))
            {
                abilityLoadout.TryUseAbility(0);
            }

            if (Input.GetKeyDown(Ability2Keycode))
            {
                abilityLoadout.TryUseAbility(1);
            }

            if (Input.GetKeyDown(Ability3Keycode))
            {
                abilityLoadout.TryUseAbility(2);
            }

            if (Input.GetKeyDown(Ability4Keycode))
            {
                abilityLoadout.TryUseAbility(3);
            }
        }
    }
}
