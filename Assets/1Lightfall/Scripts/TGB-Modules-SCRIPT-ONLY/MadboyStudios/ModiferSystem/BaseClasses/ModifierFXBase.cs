using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    //[CreateAssetMenu(fileName = "FX_base new", menuName = "Modifier System/ Create FX")]
    [Serializable]
    public class ModifierFXBase : IModifierFX
    {
        public virtual void Activate(ModifierEntry targetEntry)
        {
        }

        public virtual void Deactivate(ModifierEntry targetEntry)
        {

        }
    }
}


