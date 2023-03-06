using System.Collections.Generic;
using UnityEngine;

namespace MeeX.XMA
{
    public class CameraRender
    {
        private FileCache fileCache { get; set; }
        private GameObject container {get;set;}

        public void Initialize(GameObject _spaceRoot)
        {
            container = new GameObject("_Camera");
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


        public void Preload(string _storyUUID, List<StoryModel.CameraAgent> _agents)
        {
            foreach (StoryModel.CameraAgent agent in _agents)
            {
                GameObject go = new GameObject();
                go.name = agent.uuid;
                go.transform.SetParent(container.transform);
                go.transform.position = new Vector3((float)agent.px, (float)agent.py, (float)agent.pz);
                go.transform.rotation = Quaternion.Euler((float)agent.rx, (float)agent.ry, (float)agent.rz);
                go.transform.localScale = new Vector3((float)agent.sx, (float)agent.sx, (float)agent.sz);
            }
        }

        public void Render(StoryModel.Story _story)
        {
            
        }

        public Transform FindCamera(string _uuid)
        {
            return container.transform.Find(_uuid);
        }
    }
}