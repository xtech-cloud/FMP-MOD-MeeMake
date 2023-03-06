using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeeX.XMA
{
    public class EnvironmentRender
    {
        private List<GameObject> objects = new List<GameObject>();

        private FileCache fileCache { get; set; }
        private Dictionary<string, SceneMap> sceneMaps = new Dictionary<string, SceneMap>();
        private GameObject container {get;set;}

        public void Initialize(GameObject _spaceRoot)
        {
            container = new GameObject("_Environment");
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
            sceneMaps.Clear();
        }

        public void Preload(string _storyUUID, string _environment)
        {
            if (string.IsNullOrEmpty(_environment))
                return;

            FileCache.Index index = fileCache.FindIndex(_environment);
            if (null == index)
                return;

            AssetBundle ab = fileCache.AccessAssetBundle(index.pack);
            if (null == ab)
                return;

            TextAsset text = ab.LoadAsset<TextAsset>(index.file);
            if (null == text)
                return;

            SceneMap sceneMap = JsonConvert.DeserializeObject<SceneMap>(text.text);
            preloadSceneMap(_storyUUID, sceneMap);
        }

        private void preloadSceneMap(string _story, SceneMap _sceneMap)
        {
            sceneMaps[_story] = _sceneMap;
            foreach (SceneObject obj in _sceneMap.objects)
            {
                string assetCode = FileCache.ToUUID(string.Format("{0}.{1}", obj.pack, obj.name));
                if (fileCache.HasPrefab(assetCode))
                    continue;

                AssetBundle ab = fileCache.AccessAssetBundle(obj.pack);
                if (null == ab)
                    continue;
                GameObject go = ab.LoadAsset<GameObject>(obj.name);
                if (null == go)
                    continue;

                fileCache.CachePrefab(assetCode, go);
            }
        }

        public void Render(StoryModel.Story _story)
        {
            if (!sceneMaps.ContainsKey(_story.UUID))
                return;

            SceneMap sceneMap = sceneMaps[_story.UUID];

            // Ambient
            if (sceneMap.ambient.source.Equals(AmbientMode.Skybox.ToString()))
                RenderSettings.ambientMode = AmbientMode.Skybox;
            else if (sceneMap.ambient.source.Equals(AmbientMode.Flat.ToString()))
                RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientSkyColor = sceneMap.ambient.color;
            RenderSettings.ambientIntensity = sceneMap.ambient.intensity;

            // Fog
            if (sceneMap.fog.mode.Equals(FogMode.Linear.ToString()))
                RenderSettings.fogMode = FogMode.Linear;
            else if (sceneMap.fog.mode.Equals(FogMode.Exponential.ToString()))
                RenderSettings.fogMode = FogMode.Exponential;
            else if (sceneMap.fog.mode.Equals(FogMode.ExponentialSquared.ToString()))
                RenderSettings.fogMode = FogMode.ExponentialSquared;
            //RenderSettings.fog = sceneMap.fog.active;
            RenderSettings.fog = false;
            RenderSettings.fogColor = sceneMap.fog.color;
            RenderSettings.fogDensity = sceneMap.fog.density;
            RenderSettings.fogStartDistance = sceneMap.fog.distanceStart;
            RenderSettings.fogEndDistance = sceneMap.fog.distanceEnd;

            foreach (SceneObject obj in sceneMap.objects)
            {
                string assetCode = FileCache.ToUUID(string.Format("{0}.{1}", obj.pack, obj.name));
                GameObject go = fileCache.AccessPrefab(assetCode);
                if (null == go)
                    continue;

                GameObject clone = GameObject.Instantiate(go);
                clone.transform.SetParent(container.transform);
                clone.transform.position = obj.position;
                clone.transform.rotation = Quaternion.Euler(obj.rotation);
                clone.transform.localScale = obj.scale;
                objects.Add(clone);
            }
        }

    }
}
