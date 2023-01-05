using MBS.StatsAndTags;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    public class ApplyModifierToSelfOnStart : MonoBehaviour, IOrigin
    {
        [SerializeField]
        private ModifierBase modifier;
        public List<Tag> OriginTags { get; }

        // Start is called before the first frame update
        private void Start()
        {
            ModifierHandler target = GetComponent<ModifierHandler>();

            ModifierService.Instance.ApplyModifier(this, target, modifier);
        }

        public void SetOrigin(GameObject newOrigin)
        {
            throw new System.NotImplementedException();
        }
    }
}
