using System.Collections.Generic;
using UnityEngine;

namespace MeeX.XMA
{
    public class EffectRender
    {
        private FileCache fileCache { get; set; }
        private GameObject container {get;set;}

        public void Initialize(GameObject _spaceRoot)
        {
            container = new GameObject("_Effect");
            container.transform.SetParent(_spaceRoot.transform);
            container.transform.localPosition = Vector3.zero;
        }

        public void Release()
        {
            GameObject.Destroy(container);
        }

        public void Setup(FileCache _fileCache)
        {
            fileCache = _fileCache;
        }

        public void Preload(string _storyUUID, List<StoryModel.EffectAgent> _agents)
        {
            foreach (StoryModel.EffectAgent agent in _agents)
            {
                // already preload
                if (fileCache.HasPrefab(agent.assetCode))
                    continue;

                FileCache.Index index = fileCache.FindIndex(agent.assetCode);
                if (null == index)
                    return;

                AssetBundle ab = fileCache.AccessAssetBundle(index.pack);
                if (null == ab)
                    return;

                GameObject go = ab.LoadAsset<GameObject>(index.file);
                if (null == go)
                    return;

                fileCache.CachePrefab(agent.assetCode, go);
            }
        }

        public void Render(StoryModel.Story _story)
        {
            foreach (StoryModel.EffectAgent agent in _story.effects)
            {
                GameObject go = fileCache.AccessPrefab(agent.assetCode);
                if(null == go)
                    continue;
                
                GameObject clone = GameObject.Instantiate(go);
                clone.name = agent.uuid;
                clone.transform.SetParent(container.transform);
                clone.transform.position = new Vector3((float)agent.px, (float)agent.py, (float)agent.pz);
                clone.transform.rotation = Quaternion.Euler((float)agent.rx, (float)agent.ry, (float)agent.rz);
                clone.transform.localScale = new Vector3((float)agent.sx, (float)agent.sx, (float)agent.sz);
                clone.SetActive(agent.visible);
            }
        }

        public Transform FindEffect(string _uuid)
        {
            return container.transform.Find(_uuid);
        }
    }
}
