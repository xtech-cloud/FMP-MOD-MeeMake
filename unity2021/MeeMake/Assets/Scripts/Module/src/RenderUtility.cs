using System.Collections.Generic;
using UnityEngine;

namespace MeeX.XMA.RenderUtility
{
    public class Agent
    {
        public static void AssignMaterial(Transform _target, Dictionary<string, StoryModel.MaterialStack> _colors)
        {
            var renderers = _target.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                var materials = renderers[i].materials;
                for (int j = 0; j < materials.Length; j++)
                {
                    string id = string.Format("m:{0}#{1}", i, j);
                    if (_colors.ContainsKey(id))
                        materials[j].color = _colors[id].color;
                }
            }
        }
    }

}//namespace 
