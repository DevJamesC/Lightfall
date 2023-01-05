using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    public interface IModifierFX
    {
        public void Activate(ModifierEntry targetEntry);
        public void Deactivate(ModifierEntry targetEntry);

    }
}