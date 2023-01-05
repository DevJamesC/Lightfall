using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.InteractionSystem
{
    public class HandleEquipmentDemoForAbilityEquipmentInput : MonoBehaviour
    {
        [SerializeField]
        protected KeyCode UseMainKeycode = KeyCode.Mouse0;
        [SerializeField]
        protected KeyCode UseAlternateKeycode = KeyCode.Mouse1;
        [SerializeField]
        protected KeyCode UseAlternate2Keycode = KeyCode.Mouse2;

        public event Action MainKeyDown = delegate { };
        public event Action AlternateKeyDown = delegate { };
        public event Action Alternate2KeyDown = delegate { };

        // Update is called once per frame
        void Update()
        {

            if (Input.GetKeyDown(UseMainKeycode))
            {
                MainKeyDown.Invoke();
            }

            if (Input.GetKeyDown(UseAlternateKeycode))
            {
                AlternateKeyDown.Invoke();
            }

            if (Input.GetKeyDown(UseAlternate2Keycode))
            {
                Alternate2KeyDown.Invoke();
            }
        }
    }
}
