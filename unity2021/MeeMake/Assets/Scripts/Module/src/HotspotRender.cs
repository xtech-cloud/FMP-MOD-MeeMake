using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeeX.XMA
{
    public class HotspotRender
    {
        public Transform canvas3D { get; set; }

        public Action<string> onHotspotClick;

        public DecorateDelegate decorateHotspot;
        public GameObject templateHotspot;

        private FileCache fileCache { get; set; }
        private Texture2D atlas { get; set; }
        private GameObject container {get;set;}

        private Dictionary<string, string> blockly = new Dictionary<string, string>();

        public void Initialize(GameObject _spaceRoot)
        {
            container = new GameObject("_Hotspot");
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
            blockly.Clear();
        }

        public void Preload(string _storyUUID, List<StoryModel.HotspotAgent> _agents)
        {
            foreach (StoryModel.HotspotAgent agent in _agents)
            {
                GameObject clone = GameObject.Instantiate(templateHotspot);
                clone.name = agent.uuid;
                clone.transform.SetParent(container.transform);
                clone.transform.position = new Vector3((float)agent.px, (float)agent.py, (float)agent.pz);
                clone.transform.rotation = Quaternion.Euler((float)agent.rx, (float)agent.ry, (float)agent.rz);
                clone.transform.localScale = new Vector3((float)agent.sx, (float)agent.sx, (float)agent.sz);
                clone.transform.Rotate(0, 180, 0);
                clone.SetActive(false);
                clone.transform.Find("adjust/name").gameObject.SetActive(agent.nameVisible);
                refresh(agent.uuid, agent.style, agent.name);
                if (null != decorateHotspot)
                    decorateHotspot(clone);

                blockly[clone.name] = agent.blocklyUUID;
                clone.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (!blockly.ContainsKey(clone.name))
                        return;
                    onHotspotClick(blockly[clone.name]);
                });
            }
        }

        public void UseHotspotAltas(Texture2D _atlas)
        {
            atlas = _atlas;
        }

        public void Render(StoryModel.Story _story)
        {
             //hide all hotspots
            foreach(Transform child in container.transform)
            {
                child.gameObject.SetActive(false);
            }

            foreach (StoryModel.HotspotAgent agent in _story.hotspots)
            {
                if(!agent.visible)
                    continue;
                Transform target = container.transform.Find(agent.uuid);
                if(null == target)
                    continue;
                target.gameObject.SetActive(agent.visible);
            }
        }

        public Transform FindHotspot(string _uuid)
        {
            return container.transform.Find(_uuid);
        }

        private void refresh(string _uuid, int _style, string _name)
        {
            Transform target = container.transform.Find(_uuid);
            if (null == target)
                return;

            Image imgOutline = target.Find("adjust/outline").GetComponent<Image>();
            Image imgIcon = target.Find("adjust/icon").GetComponent<Image>();
            Text txtname = target.Find("adjust/name").GetComponent<Text>();
            txtname.text = _name;
            int row = atlas.width / 96;
            int column = atlas.height / 96;
            int x = (_style % column) * 96;
            int y = (row - _style / row - 1) * 96;
            Sprite sprite = Sprite.Create(atlas, new Rect(x, y, 96, 96), new Vector2(0.5f, 0.5f));
            imgOutline.sprite = sprite;
            imgIcon.sprite = sprite;
        }
    }
}