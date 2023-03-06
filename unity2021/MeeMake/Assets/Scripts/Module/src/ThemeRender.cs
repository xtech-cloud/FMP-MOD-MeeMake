using System.IO;
using UnityEngine;

namespace MeeX.XMA
{
    public class ThemeRender
    {
        public GameObject templateSkyboxRender;
        public Mixer mixer { get; set; }
        private FileCache fileCache { get; set; }
        private Transform skyboxRender { get; set; }
        private Material skyboxMaterial { get; set; }
        private GameObject container { get; set; }

        public void Initialize(GameObject _spaceRoot)
        {
            container = new GameObject("_Theme");
            container.transform.SetParent(_spaceRoot.transform);
            container.transform.localPosition = Vector3.zero;
            skyboxRender = GameObject.Instantiate(templateSkyboxRender, _spaceRoot.transform).transform;
            skyboxRender.transform.SetParent(container.transform);
            skyboxRender.transform.localPosition = Vector3.zero;
            skyboxRender.name = "_Skybox";
            skyboxMaterial = skyboxRender.GetComponent<MeshRenderer>().material;
            skyboxRender.gameObject.SetActive(false);

        }

        public void Release()
        {
            GameObject.Destroy(container);
        }

        public void Setup(FileCache _fileCache)
        {
            fileCache = _fileCache;
        }

        public void Preload(string _storyUUID, StoryModel.Theme _theme)
        {
            preloadMusic(_theme);
            preloadSkybox(_theme);
        }

        public void Render(StoryModel.Story _story)
        {
            renderBGM(_story);
            renderSkybox(_story);
        }

        private void preloadMusic(StoryModel.Theme _theme)
        {
            if (string.IsNullOrEmpty(_theme.music))
                return;

            string extension = Path.GetExtension(_theme.music).ToLower();
            // wav 和 mp3 已经在以audioClip的形式存放到缓存了
            if (extension.EndsWith(".wav") || extension.EndsWith(".mp3"))
                return;

            // already preload
            if (fileCache.HasAudioClip(_theme.music))
                return;

            FileCache.Index index = fileCache.FindIndex(_theme.music);
            if (null == index)
                return;

            AssetBundle ab = fileCache.AccessAssetBundle(index.pack);
            if (null == ab)
                return;

            AudioClip ac = ab.LoadAsset<AudioClip>(index.file);
            if (null == ac)
                return;

            fileCache.CacheAudioClip(_theme.music, ac);
        }

        private void preloadSkybox(StoryModel.Theme _theme)
        {
            if (string.IsNullOrEmpty(_theme.skybox))
                return;

            string extension = Path.GetExtension(_theme.skybox).ToLower();
            // jpg 和 png  已经以Texture的形式存放到缓存了
            if (extension.EndsWith(".jpg") || extension.EndsWith(".png"))
                return;

            // already preload
            if (fileCache.HasMaterial(_theme.skybox))
                return;

            FileCache.Index index = fileCache.FindIndex(_theme.skybox);
            if (null == index)
                return;

            AssetBundle ab = fileCache.AccessAssetBundle(index.pack);
            if (null == ab)
                return;

            Material material = ab.LoadAsset<Material>(index.file);
            if (null == material)
                return;

            fileCache.CacheMaterial(_theme.skybox, material);
        }

        private void renderBGM(StoryModel.Story _story)
        {
            if (string.IsNullOrEmpty(_story.theme.music))
                return;

            // 空字符表示背景音乐音轨
            AudioSource source = mixer.FindAudioSource("");
            if (null == source)
                return;

            AudioClip ac = fileCache.AccessAudioClip(_story.theme.music);
            if (null == ac)
                return;
            source.clip = ac;
            source.loop = _story.theme.loop;
            source.volume = (float)_story.theme.volume;
            source.Play();
        }

        private void renderSkybox(StoryModel.Story _story)
        {
            //动态纹理，需要先销毁，释放内存
            Texture ttSkybox = skyboxMaterial.GetTexture("_MainTex");
            if (null != ttSkybox && ttSkybox.name.Equals("dynamic"))
            {
                GameObject.DestroyImmediate(ttSkybox);
            }

            if (string.IsNullOrEmpty(_story.theme.skybox))
            {
                skyboxRender.gameObject.SetActive(false);
                //TODO defualt 
                return;
            }
            skyboxRender.gameObject.SetActive(true);

            if (_story.theme.skybox.EndsWith(".jpg") || _story.theme.skybox.EndsWith(".png"))
            {
                //如果是动态纹理
                if (fileCache.dynamic.Contains(_story.theme.skybox))
                {
                    //是否存在原始数据
                    if (fileCache.HasBinary(_story.theme.skybox))
                    {
                        //读取原始数据
                        byte[] data = fileCache.AccessBinary(_story.theme.skybox);
                        if (null != data)
                        {
                            //创建纹理，加载原始数据
                            Texture2D skybox = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
                            skybox.name = "dynamic";
                            skybox.LoadImage(data);
                            //skyboxMaterial.SetTexture("_MainTex", skybox);
                            skyboxMaterial.mainTexture = skybox;
                        }
                    }
                }
                //不是动态纹理
                else
                {
                    Texture2D skybox = fileCache.AccessTexture(_story.theme.skybox);
                    // use default skybox
                    if (null == skybox)
                        return;
                    //skyboxMaterial.SetTexture("_MainTex", skybox);
                    skyboxMaterial.mainTexture = skybox;
                }
            }
            else
            {
                Material skybox = fileCache.AccessMaterial(_story.theme.skybox);
                // use default skybox
                if (null == skybox)
                    return;
                skyboxMaterial.SetTexture("_MainTex", skybox.mainTexture);
            }
            skyboxRender.localRotation = Quaternion.Euler(0, (float)_story.theme.rotation, 0);
        }
    }
}
