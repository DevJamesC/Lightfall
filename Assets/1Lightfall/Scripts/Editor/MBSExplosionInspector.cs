using Opsive.Shared.Editor.UIElements;
using Opsive.UltimateCharacterController.Objects;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MBS.Lightfall
{
    [CustomEditor(typeof(MBSExplosion))]
    public class MBSExplosionInspector : UIElementsInspector
    {
        private MBSExplosion m_Explosion;

        /// <summary>
        /// The inspector has been enabled.
        /// </summary>
        public void OnEnable()
        {
            m_Explosion = target as MBSExplosion;
        }
    }
}
