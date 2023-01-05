using Sirenix.OdinInspector;
using UnityEngine;

namespace MBS.StatsAndTags
{
    [CreateAssetMenu(fileName = "Tag_new", menuName = "Tag/ Create Tag")]
    public class Tag : ScriptableObject
    {
        public string Value { get => name; }
    }
}


