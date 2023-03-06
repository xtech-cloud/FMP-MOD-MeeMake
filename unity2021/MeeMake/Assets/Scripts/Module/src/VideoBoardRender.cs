using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProVideo;

namespace MeeX.XMA
{
    public class VideoBoardRender
    {
        public Transform canvas3D { get; set; }
        public GameObject templateVideoboard { get; set; }

        private FileCache fileCache { get; set; }
        private GameObject container { get; set; }

        public void Initialize(GameObject _spaceRoot)
        {
            container = new GameObject("_VideoBoard");
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

        public void Preload(string _storyUUID, List<StoryModel.VideoBoardAgent> _agents)
        {
            foreach (StoryModel.VideoBoardAgent agent in _agents)
            {
                GameObject clone = GameObject.Instantiate(templateVideoboard);
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

                if (!string.IsNullOrEmpty(agent.file))
                {
                    MediaPlayer player = clone.GetComponent<MediaPlayer>();
                    player.m_AutoStart = agent.autoPlay;
                    player.m_Volume = agent.volume;
                    if (RuntimePlatform.WindowsPlayer == Application.platform || RuntimePlatform.WindowsEditor == Application.platform)
                    {
                        byte[] data = fileCache.AccessBinary(agent.file);
                        player.OpenVideoFromBuffer(data, false);
                    }
                    else
                    {
                        string file = Path.Combine(Application.temporaryCachePath, agent.file);
                        player.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, file, false);
                    }
                }

                clone.SetActive(false);
            }
        }

        public void Render(StoryModel.Story _story)
        {
            //hide all boards
            foreach (Transform child in container.transform)
            {
                child.gameObject.SetActive(false);
            }

            foreach (StoryModel.VideoBoardAgent agent in _story.videoboards)
            {
                if (!agent.visible)
                    continue;
                Transform target = container.transform.Find(agent.uuid);
                if (null == target)
                    continue;
                target.gameObject.SetActive(true);
            }
        }

        public Transform FindVideoBoard(string _uuid)
        {
            return container.transform.Find(_uuid);
        }
    }
}