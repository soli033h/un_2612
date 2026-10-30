using System;
using System.Collections.Generic;
using UnityEngine;

namespace Racing.Client.Vehicles
{
    /// <summary>
    /// Applies Car.PaintColor to the paint material. Each car gets its own material instances,
    /// so cars sharing the prefab can wear different colours.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CarPaint : MonoBehaviour
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [Tooltip("Renderers whose GameObject name ends with this are painted.")]
        [SerializeField] string paintRendererSuffix = "_PAINT";

        readonly List<Material> paintMaterials = new List<Material>();

        public void Apply(Color color)
        {
            if (paintMaterials.Count == 0)
                CollectPaintMaterials();

            foreach (Material material in paintMaterials)
                material.SetColor(BaseColorId, color);
        }

        void CollectPaintMaterials()
        {
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
                if (renderer.name.EndsWith(paintRendererSuffix, StringComparison.OrdinalIgnoreCase))
                    paintMaterials.AddRange(renderer.materials);

            if (paintMaterials.Count == 0)
                Debug.LogWarning($"{name}: no renderer ending with '{paintRendererSuffix}' found to paint.", this);
        }

        void OnDestroy()
        {
            foreach (Material material in paintMaterials)
                Destroy(material);
        }
    }
}
