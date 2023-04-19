using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightfallMunitionBoxInteractor : MonoBehaviour
{
    [Tooltip("Does this agent reduce the ammo in the ammo box when picking up ammo? For AI, the answer is generally no.")]
    public bool DoNotReduceBoxAmmoValue;
}
