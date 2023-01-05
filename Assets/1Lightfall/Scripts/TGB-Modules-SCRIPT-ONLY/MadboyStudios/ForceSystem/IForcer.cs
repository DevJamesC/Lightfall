using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ForceSystem
{
    public interface IForcer
    {
        public GameObject gameObject { get; }
        public int Force { get; set; }
    }
}
