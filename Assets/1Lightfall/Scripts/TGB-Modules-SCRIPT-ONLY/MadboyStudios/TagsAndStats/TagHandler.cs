using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.StatsAndTags
{
    public class TagHandler : MonoBehaviour
    {
        [SerializeField]
        private List<Tag> tags;

        public List<Tag> Tags { get => tags; }

        private void Awake()
        {
            if (tags == null)
                tags = new List<Tag>();
        }

        /// <summary>
        /// Return true if the tag is contained in the TagHandler
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool ContainsTag(Tag tag)
        {
            return tags.Contains(tag);
        }

        /// <summary>
        /// Return true if any tag is contained in the TagHandler
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool ContainsTag(List<Tag> tags, bool mustContainAll = false)
        {
            if (!mustContainAll)
            {
                foreach (var tag in tags)
                {
                    if (this.tags.Contains(tag))
                        return true;
                }
                return false;
            }
            else
            {
                foreach (var tag in tags)
                {
                    if (!this.tags.Contains(tag))
                        return false;
                }
                return true;
            }

        }

        public void AddTag(Tag tag)
        {
            if (!tags.Contains(tag))
                tags.Add(tag);
        }

        public void RemoveTag(Tag tag)
        {
            if (tags.Contains(tag))
                tags.Remove(tag);
        }

        /// <summary>
        /// returns true if both lists contain the same tags. Will return false if any tag does not have a 1:1 mapping.
        /// </summary>
        /// <param name="incomingTags"></param>
        /// <returns></returns>
        public bool CompareTags(List<Tag> incomingTags)
        {
            foreach (Tag tag in incomingTags)
            {
                if (!tags.Contains(tag))
                    return false;
            }

            foreach (Tag tag in tags)
            {
                if (!incomingTags.Contains(tag))
                {
                    return false;
                }
            }

            return true;
        }
    }
}