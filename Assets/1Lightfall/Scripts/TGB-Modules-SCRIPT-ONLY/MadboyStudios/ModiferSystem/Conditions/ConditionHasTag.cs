using MBS.StatsAndTags;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    public class ConditionHasTag : ModifierConditionBase
    {
        [SerializeField, Tooltip("All: target must have all of the tags\n" +
                                "Any: target must have at least one of the tags\n" +
                                "Exact: target must have all of the tags, and no others" +
                                "None: target must have none of the tags")]
        private EvaluationType evaluationType = EvaluationType.All;

        [SerializeField]
        private List<Tag> tags = new List<Tag>();


        public override bool Evaluate(ModifierHandler handlerContext)
        {
            TagHandler tagHandler = handlerContext.gameObject.GetComponent<TagHandler>();

            if (tagHandler == null)
                return false;

            foreach (var tag in tags)
            {
                switch (evaluationType)
                {
                    case EvaluationType.All:
                        if (!tagHandler.ContainsTag(tag))
                            return false;
                        break;
                    case EvaluationType.None:
                        if (tagHandler.ContainsTag(tag))
                            return false;
                        break;
                    case EvaluationType.Any:
                        if (tagHandler.ContainsTag(tag))
                            return true;

                        break;
                }
            }

            if (evaluationType == EvaluationType.Exact)
            {
                return tagHandler.CompareTags(tags);
            }

            return true;
        }

        [Serializable]
        private enum EvaluationType
        {
            Any,
            All,
            Exact,
            None
        }
    }
}
