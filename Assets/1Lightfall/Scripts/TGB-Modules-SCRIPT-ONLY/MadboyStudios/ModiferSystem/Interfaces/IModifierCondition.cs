using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    public interface IModifierCondition
    {
        public bool Evaluate(ModifierHandler handlerContext);
    }
}
