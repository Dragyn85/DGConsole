using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragynGames
{
    [DisallowMultipleComponent]
    public class HighLighting : MonoBehaviour
    {
        Dictionary<Renderer, Material[]> originalMaterials = new();
        Material highlightMaterial;
        float fadeDuration = 0.5f;

        public void ActivateHighlight(Material highlightMaterial, bool includeChildren)
        {
            this.highlightMaterial = highlightMaterial;
            if (includeChildren)
            {
                HighlightIncludeChildren();
            }
            else
            {
                HighlightExcludeChildren();
            }
        }
        
        public void HighlightPulse(Material highlightMaterial, bool includeChildren)
        {
            if(originalMaterials.Count == 0)
            {
                ActivateHighlight(highlightMaterial, includeChildren);
            }

            if (originalMaterials.Count > 0)
            {
                StartCoroutine(PulseHighlight());
            }
            
        }

        private IEnumerator PulseHighlight()
        {
            float fadePercentage = 0;
            while (fadePercentage < 1)
            {
                fadePercentage += Time.deltaTime / fadeDuration;
                foreach (var renderer in originalMaterials.Keys)
                {
                    renderer.material = highlightMaterial;
                    Color color = renderer.material.color;
                    color.a = fadePercentage;
                    renderer.material.color = color;
                }
                yield return null;
            }

            while (fadePercentage > 0)
            {
                fadePercentage -= Time.deltaTime / fadeDuration;
                foreach (var renderer in originalMaterials.Keys)
                {
                    Color color = renderer.material.color;
                    color.a = fadePercentage;
                    renderer.material.color = color;
                }
                yield return null;
            }

            foreach (var renderer in originalMaterials.Keys)
            {
                renderer.materials = originalMaterials[renderer];
            }
            Destroy(this);
        }

        private void HighlightExcludeChildren()
        {
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                originalMaterials[renderer] = renderer.materials;
                renderer.material = new Material(highlightMaterial);
            }
        }

        private void HighlightIncludeChildren()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    originalMaterials[renderer] = renderer.materials;
                    renderer.material = new Material(highlightMaterial);
                }
            }
        }

        public void RemoveHighlight()
        {
            foreach (var renderer in originalMaterials.Keys)
            {
                renderer.materials = originalMaterials[renderer];
            }
            Destroy(this);
        }
    }
}