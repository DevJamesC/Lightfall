using Opsive.UltimateCharacterController.Items.Actions.Impact;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    public class DealDamageThenArc : LightfallDamage
    {
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            base.OnImpactInternal(ctx);

            Debug.Log("arcing will happen next");
        }
    }
}
