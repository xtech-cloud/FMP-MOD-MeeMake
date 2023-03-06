using System.Collections.Generic;
using UnityEngine;

namespace MeeX.XMA
{
    public class ModelRender
    {
        private FileCache fileCache { get; set; }
        private GameObject container {get;set;}

        public void Initialize(GameObject _spaceRoot)
        {
            container = new GameObject("_Model");
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

        public void Preload(string _storyUUID, List<StoryModel.ModelAgent> _agents)
        {
            foreach (StoryModel.ModelAgent agent in _agents)
            {
                // already preload
                if (fileCache.HasPrefab(agent.assetCode))
                    continue;

                FileCache.Index index = fileCache.FindIndex(agent.assetCode);
                if (null == index)
                    continue;

                AssetBundle ab = fileCache.AccessAssetBundle(index.pack);
                if (null == ab)
                    continue;

                GameObject go = ab.LoadAsset<GameObject>(index.file);
                if (null == go)
                    continue;

                fileCache.CachePrefab(agent.assetCode, go);
            }
        }

        public void Render(StoryModel.Story _story)
        {
            foreach (StoryModel.ModelAgent agent in _story.models)
            {
                GameObject go = fileCache.AccessPrefab(agent.assetCode);
                if(null == go)
                    continue;
                
                Vector3 position = new Vector3((float)agent.px, (float)agent.py, (float)agent.pz);
                Quaternion rotation = Quaternion.Euler((float)agent.rx, (float)agent.ry, (float)agent.rz);
                GameObject clone = GameObject.Instantiate(go, position, rotation, container.transform);
                clone.name = agent.uuid;
                clone.transform.localScale = new Vector3((float)agent.sx, (float)agent.sx, (float)agent.sz);
                clone.SetActive(agent.visible);

                RenderUtility.Agent.AssignMaterial(clone.transform, agent.materials);
            }
        }

        public Transform FindModel(string _uuid)
        {
            return container.transform.Find(_uuid);
        }
    }
}
