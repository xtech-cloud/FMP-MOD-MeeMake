using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeeX.XMA
{
    public class TextBoardRender
    {
        public Transform canvas3D {get;set;}
        public GameObject templateTextboard {get;set;}

        private FileCache fileCache { get; set; }

        private GameObject container {get;set;}

        public void Initialize(GameObject _spaceRoot)
        {
            container = new GameObject("_TextBoard");
            container.transform.SetParent(canvas3D);
            container.transform.localPosition = Vector3.zero;
            container.transform.localRotation = Quaternion.identity;
            container.transform.localScale = Vector3.one;
        }

        public void Release()
        {
            GameObject.Destroy(container);
        }

        public void Setup(FileCache _fileCache)
        {
            fileCache = _fileCache;
        }

        public void Preload(string _storyUUID, List<StoryModel.TextBoardAgent> _agents)
        {
            foreach (StoryModel.TextBoardAgent agent in _agents)
            {
                GameObject clone = GameObject.Instantiate(templateTextboard);
                clone.name = agent.uuid;
                clone.transform.SetParent(container.transform);
                clone.transform.position = new Vector3((float)agent.px, (float)agent.py, (float)agent.pz);
                clone.transform.rotation = Quaternion.Euler((float)agent.rx, (float)agent.ry, (float)agent.rz);
                RectTransform rtFrame = clone.transform.Find("adjust/frame").GetComponent<RectTransform>();
                rtFrame.sizeDelta = new Vector2(agent.width, agent.height);
                clone.transform.localScale = new Vector3((float)agent.sx, (float)agent.sx, (float)agent.sz);
                clone.SetActive(false);
                refreshText(clone.transform, agent.content);
            }
        }

        public void Render(StoryModel.Story _story)
        {
             //hide all boards
            foreach(Transform child in container.transform)
            {
                child.gameObject.SetActive(false);
            }

            foreach (StoryModel.TextBoardAgent agent in _story.textboards)
            {
                if(!agent.visible)
                    continue;
                Transform target = container.transform.Find(agent.uuid);
                if(null == target)
                    continue;
                target.gameObject.SetActive(true);
            }
        }

        public Transform FindTextBoard(string _uuid)
        {
            return container.transform.Find(_uuid);
        }

        private void refreshText(Transform _agent, string _text)
        {
            if(null == _agent)
                return;
            Text text = _agent.Find("adjust/frame/content").GetComponent<Text>();
            text.text = _text;
        }
    }
}