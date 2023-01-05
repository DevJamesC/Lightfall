using MBS.StatsAndTags;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    public interface IOrigin
    {
        public GameObject gameObject { get; }
        public void SetOrigin(GameObject newOrigin);
        public List<Tag> OriginTags { get; }
    }
}