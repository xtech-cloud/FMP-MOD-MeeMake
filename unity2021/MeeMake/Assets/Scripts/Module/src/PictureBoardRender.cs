using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeeX.XMA
{
    public class PictureBoardRender
    {
        public Transform canvas3D { get; set; }
        public GameObject templatePictureboard { get; set; }

        private FileCache fileCache { get; set; }
        private GameObject container {get;set;}

        public void Initialize(GameObject _spaceRoot)
        {
            container = new GameObject("_PictureBoard");
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

        public void Preload(string _storyUUID, List<StoryModel.PictureBoardAgent> _agents)
        {
            foreach (StoryModel.PictureBoardAgent agent in _agents)
            {
                GameObject clone = GameObject.Instantiate(templatePictureboard);
                clone.name = agent.uuid;
                clone.transform.SetParent(container.transform);
                clone.transform.position = new Vector3((float)agent.px, (float)agent.py, (float)agent.pz);
                clone.transform.rotation = Quaternion.Euler((float)agent.rx, (float)agent.ry, (float)agent.rz);
                clone.transform.localScale = new Vector3((float)agent.sx, (float)agent.sx, (float)agent.sz);
                
                RectTransform rtFrame = clone.transform.Find("adjust/frame").GetComponent<RectTransform>();
                rtFrame.sizeDelta = new Vector2(agent.width, agent.height);
                Color color = rtFrame.GetComponent<Image>().color;
                color.a = agent.backgroundAlpha;
                rtFrame.GetComponent<Image>().color = color;

                PictureAnimation picAnim = clone.AddComponent<PictureAnimation>();
                picAnim.image = rtFrame.transform.Find("content").GetComponent<Image>();
                picAnim.interval = agent.interval;
                Sprite[] sprites = new Sprite[agent.count];
                for (int i = 0; i < agent.count; i++)
                {
                    if (i >= agent.files.Length)
                        continue;

                    Texture2D texture = fileCache.AccessTexture(agent.files[i]);
                    sprites[i] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
                picAnim.sprites = sprites;
                clone.SetActive(false);
            }
        }

        public void Render(StoryModel.Story _story)
        {
            //hide all boards
            foreach(Transform child in container.transform)
            {
                child.gameObject.SetActive(false);
            }

            foreach (StoryModel.PictureBoardAgent agent in _story.pictureboards)
            {
                if(!agent.visible)
                    continue;
                Transform target = container.transform.Find(agent.uuid);
                if(null == target)
                    continue;
                target.gameObject.SetActive(true);
            }
        }

        public Transform FindPictureBoard(string _uuid)
        {
            return container.transform.Find(_uuid);
        }
    }
}