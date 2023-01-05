using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MBS.ModifierSystem
{
    public class FXAddMaterial : ModifierFXBase
    {
        [SerializeField]
        private Material material;

        public override void Activate(ModifierEntry targetEntry)
        {
            Renderer renderer = targetEntry.Target.gameObject.GetComponentInChildren<Renderer>();

            if (renderer == null)
                return;

            List<Material> targetMaterials = renderer.sharedMaterials.ToList();

            targetMaterials.Add(material);
            renderer.sharedMaterials = targetMaterials.ToArray();


        }

        public override void Deactivate(ModifierEntry targetEntry)
        {
            Renderer renderer = targetEntry.Target.gameObject.GetComponentInChildren<Renderer>();

            if (renderer == null)
                return;

            List<Material> targetMaterials = renderer.sharedMaterials.ToList();
            if (targetMaterials.Contains(material))
                targetMaterials.Remove(material);



            renderer.sharedMaterials = targetMaterials.ToArray();
        }

        //TODO: Add state for the FX the same way Effects have state? make a FXStateData script to extend?
    }
}
