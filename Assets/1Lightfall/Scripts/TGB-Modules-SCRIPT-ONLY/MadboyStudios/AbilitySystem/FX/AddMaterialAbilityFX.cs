using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MBS.AbilitySystem
{
    public class AddMaterialAbilityFX : AbilityFXBase
    {
        [SerializeField]
        private Material material;
        [SerializeField]
        private int durationInMiliseconds;

        public override void Activate(AbilityWrapperBase wrapper)
        {
            Renderer renderer = wrapper.Origin.GetComponentInChildren<Renderer>();

            if (renderer == null)
                return;

            List<Material> targetMaterials = renderer.sharedMaterials.ToList();

            targetMaterials.Add(material);
            renderer.sharedMaterials = targetMaterials.ToArray();


            Task.Delay(durationInMiliseconds).ContinueWith(t => Remove(renderer));
        }

        private void Remove(Renderer renderer)
        {
            Debug.Log(renderer);
            if (renderer == null)
                return;

            List<Material> targetMaterials = renderer.sharedMaterials.ToList();
            if (targetMaterials.Contains(material))
                targetMaterials.Remove(material);

            renderer.sharedMaterials = targetMaterials.ToArray();
        }

    }
}
