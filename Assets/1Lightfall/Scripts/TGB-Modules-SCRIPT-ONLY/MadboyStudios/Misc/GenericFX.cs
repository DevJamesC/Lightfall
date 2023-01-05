using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Misc
{
    [Serializable]
    public class GenericFX
    {
        public virtual void Activate(GameObject source, List<GameObject> targets = null)
        {

        }

        public virtual void Deactivate(GameObject source = null, List<GameObject> targets = null)
        {

        }
    }
}
