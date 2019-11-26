using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enviro.Structures
{
    public class StructureObject : MonoBehaviour
    {
        public byte ID;
        public Material DestroyedMaterial;
        private Material[] OriginalMaterials;

        MeshRenderer meshRenderer;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            OriginalMaterials = meshRenderer.materials;
        }

        public void UpdateStructure(short health)
        {
            if (health <= 0)
            {
                GetComponent<Collider>().enabled = false;
                // Set the materials on the object to the one we specified when it's destroyed
                Material[] mats = new Material[meshRenderer.materials.Length];
                for (int i = 0; i < meshRenderer.materials.Length; i++)
                {
                    mats[i] = DestroyedMaterial;
                }
                meshRenderer.materials = mats;
            }
            else
            {
                GetComponent<Collider>().enabled = true;
                // Set the material back to the original materials
                meshRenderer.materials = OriginalMaterials;
            }
        }
    }
}