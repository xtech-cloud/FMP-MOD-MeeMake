using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeeX.MeeMake;
using LibMVCS = XTC.FMP.LIB.MVCS;
using XTC.oelArchive;
using Newtonsoft.Json;

namespace MeeX.XMA
{
    public class Options
    {
        //使用摄像机的位置同步canvas3D的位置
        public bool canvas3DTracking = false;
        public bool storyboardVisible = false;
    }

    public class Runtime
    {
        public LibMVCS.Logger logger;
        public static DecorateDelegate decorateHotspot;
        public MonoBehaviour mono
        {
            get;
            set;
        }
        public Action<int> onPreloadReady;
        public Action<int> onPreloadChanged;
        public Action<LibMVCS.Error> onPreloadError
        {
            get;
            set;
        }
        public Action onPreloadFinish
        {
            get;
            set;
        }
        public Transform slotCanvas2D
        {
            get;
            set;
        }
        public Transform slotCanvas3D
        {
            get;
            set;
        }
        public Transform camera
        {
            get;
            set;
        }
        public Options options
        {
            get;
            set;
        }


        public List<string> stories
        {
            get
            {
                List<string> list = new List<string>();
                storyList.ForEach((_item) =>
                {
                    list.Add(_item.Name);
                });
                return list;
            }
        }

        public int totalPreload
        {
            get;
            private set;
        }
        public int finishPreload
        {
            get;
            private set;
        }

        private List<StoryModel.Story> storyList = new List<StoryModel.Story>();
        private FileCache fileCache = new FileCache();
        private EnvironmentRender environmentRender = new EnvironmentRender();
        private CameraRender cameraRender = new CameraRender();
        private CharacterRender characterRender = new CharacterRender();
        private ModelRender modelRender = new ModelRender();
        private HotspotRender hotspotRender = new HotspotRender();
        private TextBoardRender textboardRender = new TextBoardRender();
        private PictureBoardRender pictureboardRender = new PictureBoardRender();
        private VideoBoardRender videoboardRender = new VideoBoardRender();
        private EffectRender effectRender = new EffectRender();
        private ThemeRender themeRender = new ThemeRender();
        private Mixer mixer = new Mixer();
        private BlocklyInterpreter blocklyInterpreter = new BlocklyInterpreter();

        private StoryAPI apiStory { get; set; }
        private CoreAPI apiCore { get; set; }

        public void Initialize(GameObject _spaceRoot)
        {
            options = new Options();
            //registerCustomType();

            themeRender.mixer = mixer;

            hotspotRender.canvas3D = slotCanvas3D;
            textboardRender.canvas3D = slotCanvas3D;
            pictureboardRender.canvas3D = slotCanvas3D;
            videoboardRender.canvas3D = slotCanvas3D;

            apiCore = new CoreAPI();
            StoryProxy storyProxy = new StoryProxy();
            storyProxy.runtime = this;
            apiStory = new StoryAPI();
            apiStory.proxy = storyProxy;

            apiStory.mono = mono;
            apiStory.activeCamera = camera;
            apiStory.cameraRender = cameraRender;
            apiStory.characterRender = characterRender;
            apiStory.modelRender = modelRender;
            apiStory.hotspotRender = hotspotRender;
            apiStory.textBoardRender = textboardRender;
            apiStory.pictureBoardRender = pictureboardRender;
            apiStory.videoBoardRender = videoboardRender;
            apiStory.effectRender = effectRender;
            apiStory.mixer = mixer;

            hotspotRender.decorateHotspot = decorateHotspot;
            hotspotRender.onHotspotClick = blocklyInterpreter.InvokeRun;

            themeRender.templateSkyboxRender = _spaceRoot.transform.Find("SkyboxRender").gameObject;
            themeRender.Initialize(_spaceRoot);
            cameraRender.Initialize(_spaceRoot);
            characterRender.Initialize(_spaceRoot);
            modelRender.Initialize(_spaceRoot);
            environmentRender.Initialize(_spaceRoot);
            blocklyInterpreter.logger = logger;
            blocklyInterpreter.apiStory = apiStory;
            blocklyInterpreter.apiCore = apiCore;
            blocklyInterpreter.Initialize(_spaceRoot);
            textboardRender.templateTextboard = _spaceRoot.transform.Find("TextBoard").gameObject;
            textboardRender.Initialize(_spaceRoot);
            pictureboardRender.templatePictureboard = _spaceRoot.transform.Find("PictureBoard").gameObject;
            pictureboardRender.Initialize(_spaceRoot);
            videoboardRender.templateVideoboard = _spaceRoot.transform.Find("VideoBoard").gameObject;
            videoboardRender.Initialize(_spaceRoot);
            hotspotRender.templateHotspot = _spaceRoot.transform.Find("Hotspot").gameObject;
            hotspotRender.Initialize(_spaceRoot);
            effectRender.Initialize(_spaceRoot);
            mixer.Initialize(_spaceRoot);
        }

        public void Preload(byte[] _data)
        {
            MemoryReader reader = new MemoryReader();
            reader.Open(_data);
            totalPreload = reader.entries.Length;
            finishPreload = 0;
            if (null != onPreloadReady)
            {
                onPreloadReady(totalPreload);
            }
            mono.StartCoroutine(preload(reader));
        }

        public void Preload(string _file)
        {
            if (!File.Exists(_file))
            {
                onPreloadError(LibMVCS.Error.NewAccessErr("file not found"));
                return;
            }

            FileReader reader = new FileReader();
            reader.Open(_file);
            totalPreload = reader.entries.Length;
            finishPreload = 0;
            if (null != onPreloadReady)
            {
                onPreloadReady(totalPreload);
            }
            mono.StartCoroutine(preload(reader));
        }

        public void Execute()
        {
            RenderStory(storyList[0].Name);
        }

        public void Update()
        {
            if (null != options)
            {
                if (options.canvas3DTracking)
                {
                    slotCanvas3D.position = camera.position;
                }
            }

            blocklyInterpreter.Update();
        }

        public bool TryGetStoryThumb(string _storyName, out Sprite _sprite)
        {
            _sprite = null;
            var story = storyList.Find((_item) =>
            {
                return _item.Name.Equals(_storyName);
            });

            if (null == story)
                return false;

            _sprite = fileCache.AccessSprite(string.Format("thumb_{0}", story.UUID));
            return null != _sprite;
        }

        private IEnumerator preload(XTC.oelArchive.StreamReader _reader)
        {
            yield return new WaitForEndOfFrame();
            themeRender.Setup(fileCache);
            environmentRender.Setup(fileCache);
            characterRender.Setup(fileCache);
            modelRender.Setup(fileCache);
            hotspotRender.Setup(fileCache);
            textboardRender.Setup(fileCache);
            pictureboardRender.Setup(fileCache);
            videoboardRender.Setup(fileCache);
            effectRender.Setup(fileCache);
            cameraRender.Setup(fileCache);
            mixer.Setup(fileCache);
            blocklyInterpreter.Setup();

            yield return extractArchive(_reader);
            _reader.Close();

            foreach (StoryModel.Story story in storyList)
            {
                themeRender.Preload(story.UUID, story.theme);
                environmentRender.Preload(story.UUID, story.theme.environment);
                characterRender.Preload(story.UUID, story.characters);
                modelRender.Preload(story.UUID, story.models);
                hotspotRender.Preload(story.UUID, story.hotspots);
                textboardRender.Preload(story.UUID, story.textboards);
                pictureboardRender.Preload(story.UUID, story.pictureboards);
                videoboardRender.Preload(story.UUID, story.videoboards);
                effectRender.Preload(story.UUID, story.effects);
                cameraRender.Preload(story.UUID, story.cameras);
            }

            if (storyList.Count == 0)
            {
                onPreloadFinish();
                yield break;
            }

            yield return new WaitForEndOfFrame();
            onPreloadFinish();
            yield return new WaitForEndOfFrame();
        }

        public void Release()
        {
            //unregisterCustomType();
            cameraRender.Release();
            characterRender.Release();
            modelRender.Release();
            environmentRender.Release();
            blocklyInterpreter.Release();
            textboardRender.Release();
            pictureboardRender.Release();
            videoboardRender.Release();
            effectRender.Release();
            hotspotRender.Release();
            mixer.Release();
            themeRender.Release();
            fileCache.Clean();
        }

        public void RenderStory(string _storyName)
        {
            StoryModel.Story story = storyList.Find((_item) =>
            {
                return _item.Name.Equals(_storyName);
            });
            if (null == story)
            {
                Debug.LogError("story not found");
                return;
            }

            //mixer必须在theme前面执行
            mixer.Render(story);
            themeRender.Render(story);
            environmentRender.Render(story);
            characterRender.Render(story);
            modelRender.Render(story);
            hotspotRender.Render(story);
            textboardRender.Render(story);
            pictureboardRender.Render(story);
            videoboardRender.Render(story);
            effectRender.Render(story);
            cameraRender.Render(story);
            blocklyInterpreter.Execute(story);
        }

        private IEnumerator extractArchive(XTC.oelArchive.StreamReader _reader)
        {
            fileCache.dynamic.Clear();
            List<string> entries = new List<string>(_reader.entries);
            entries.Sort((_left, _right) =>
            {
                return string.Compare(_left, _right);
            });
            foreach (string entity in entries)
            {
                yield return new WaitForEndOfFrame();

                if (entity.Equals("_manifest.json"))
                {
                    byte[] data = _reader.Read(entity);
                    string json = Encoding.UTF8.GetString(data);
                    Debug.Log(json);
                    storyList = JsonConvert.DeserializeObject<List<StoryModel.Story>>(json);
                }
                else if (entity.Equals("_index.json"))
                {
                    byte[] data = _reader.Read(entity);
                    string json = Encoding.UTF8.GetString(data);
                    List<FileCache.Index> indexTable = JsonConvert.DeserializeObject<List<FileCache.Index>>(json);
                    fileCache.UpdateIndex(indexTable);
                }
                else if (entity.Equals("_version.cfg"))
                {
                    byte[] data = _reader.Read(entity);
                    string version = Encoding.UTF8.GetString(data);
                    logger.Info("version is {0}", version);
                }
                else if (entity.Equals("_language.cfg"))
                {
                    byte[] data = _reader.Read(entity);
                    string language = Encoding.UTF8.GetString(data);
                    blocklyInterpreter.UseLanguage(language);
                    logger.Info("lauguage is {0}", language);
                }
                else if (entity.Equals("_hotspot.atlas"))
                {
                    byte[] data = _reader.Read(entity);
                    Texture2D texture = new Texture2D(10, 10, TextureFormat.RGBA32, false);
                    texture.LoadImage(data);
                    texture.Apply();
                    hotspotRender.UseHotspotAltas(texture);
                }
                else if (entity.Equals("_dynamic.json"))
                {
                    byte[] data = _reader.Read(entity);
                    string json = Encoding.UTF8.GetString(data);
                    fileCache.dynamic = JsonConvert.DeserializeObject<List<string>>(json);
                }
                else if (entity.Equals("_options.json"))
                {
                    byte[] data = _reader.Read(entity);
                    string json = Encoding.UTF8.GetString(data);
                    options = JsonConvert.DeserializeObject<Options>(json);
                }
                else if (entity.EndsWith(".luablockly"))
                {
                    byte[] data = _reader.Read(entity);
                    string code = Encoding.UTF8.GetString(data);
                    logger.Debug("{0}", code);
                    blocklyInterpreter.AddScript(entity, code);
                }
                else if (entity.StartsWith("file://"))
                {
                    string extension = Path.GetExtension(entity).ToLower();
                    if (extension.Equals(".jpg") || extension.Equals(".png"))
                        yield return cacheTexture(entity, _reader);
                    else if (extension.Equals(".wav"))
                        yield return cacheWAV(entity, _reader);
                    else if (extension.Equals(".acv"))
                        yield return cacheACV(entity, _reader);
                    else if (extension.Equals(".mp4"))
                        yield return cacheMP4(entity, _reader);
                }
                else if (entity.StartsWith("thumb_"))
                {
                    yield return cacheSprite(entity, _reader);
                }
                else if (entity.StartsWith("asset://"))
                {
                    yield return cacheAsset(entity, _reader);
                }
                else if (entity.EndsWith(".lua"))
                {
                    byte[] data = _reader.Read(entity);
                    string code = Encoding.UTF8.GetString(data);
                    //logger.Debug("{0}", code);
                    blocklyInterpreter.AddModule(entity, code);
                }

                finishPreload += 1;
                if (null != onPreloadChanged)
                {
                    onPreloadChanged(finishPreload);
                }
            }
        }

        private IEnumerator cacheTexture(string _entity, XTC.oelArchive.StreamReader _reader)
        {
            string filename = _entity.Remove(0, "file://".Length);
            if (fileCache.HasTexture(filename))
                yield break;

            logger.Debug("cache texture: {0}", filename);
            byte[] data = _reader.Read(_entity);
            yield return new WaitForEndOfFrame();
            Texture2D texture = new Texture2D(10, 10, TextureFormat.RGBA32, false, false);
            // 动态加载的文件只标记名字，在真正使用时才加载数据
            if (fileCache.dynamic.Contains(filename))
            {
                texture.name = "dynamic";
                fileCache.CacheBinary(filename, data);
            }
            else
            {
                texture.LoadImage(data);
            }
            fileCache.CacheTexture(filename, texture);
        }

        private IEnumerator cacheSprite(string _entity, XTC.oelArchive.StreamReader _reader)
        {
            string filename = Path.GetFileNameWithoutExtension(_entity);
            if (fileCache.HasSprite(filename))
                yield break;

            logger.Debug("cache sprite: {0}", filename);
            byte[] data = _reader.Read(_entity);
            yield return new WaitForEndOfFrame();
            Texture2D texture = new Texture2D(10, 10, TextureFormat.RGBA32, false, false);
            texture.LoadImage(data);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            fileCache.CacheSprite(filename, sprite);
        }

        private IEnumerator cacheWAV(string _entity, XTC.oelArchive.StreamReader _reader)
        {
            string filename = _entity.Remove(0, "file://".Length);
            if (fileCache.HasAudioClip(filename))
                yield break;

            logger.Debug("cache wav: {0}", filename);
            byte[] data = _reader.Read(_entity);
            yield return new WaitForEndOfFrame();
            AudioUtility.WAV wav = new AudioUtility.WAV(data);
            AudioClip audioClip = AudioClip.Create(filename, wav.SampleCount, 1, wav.Frequency, false);
            audioClip.SetData(wav.LeftChannel, 0);
            fileCache.CacheAudioClip(filename, audioClip);
        }

        private IEnumerator cacheACV(string _entity, XTC.oelArchive.StreamReader _reader)
        {
            string filename = _entity.Remove(0, "file://".Length);
            if (fileCache.HasAudioClip(filename))
                yield break;

            logger.Debug("cache acv: {0}", filename);
            byte[] data = _reader.Read(_entity);
            yield return new WaitForEndOfFrame();

            int sampleCount = data.Length / sizeof(float);
            float[] samples = new float[sampleCount];
            int offset = 0;
            for (int i = 0; i < data.Length; i += sizeof(float))
            {
                samples[i / 4] = BitConverter.ToSingle(data, offset);
                offset += sizeof(float);
            }
            AudioClip audioClip = AudioClip.Create(filename, sampleCount, 1, 44100, false);
            audioClip.SetData(samples, 0);
            fileCache.CacheAudioClip(filename, audioClip);
        }

        private IEnumerator cacheAsset(string _entity, XTC.oelArchive.StreamReader _reader)
        {
            string filename = _entity.Remove(0, "asset://".Length);
            if (fileCache.HasAssetBundle(filename))
            {
                logger.Warning("asset {0} not found", filename);
                yield break;
            }

            logger.Debug("cache asset: {0}", filename);
            byte[] data = _reader.Read(_entity);
            yield return new WaitForEndOfFrame();
            AssetBundleCreateRequest abcr = AssetBundle.LoadFromMemoryAsync(data);
            yield return abcr;
            AssetBundle ab = abcr.assetBundle;
            if (null == ab)
            {
                logger.Warning("assetbundle {0} is null", filename);
                yield break;
            }
            fileCache.CacheAssetBundle(filename, ab);
        }

        private IEnumerator cacheMP4(string _entity, XTC.oelArchive.StreamReader _reader)
        {
            string filename = _entity.Remove(0, "file://".Length);
            yield return new WaitForEndOfFrame();
            if (RuntimePlatform.WindowsPlayer == Application.platform || RuntimePlatform.WindowsEditor == Application.platform)
            {
                if (fileCache.HasBinary(filename))
                    yield break;

                logger.Debug("cache binary: {0}", filename);
                byte[] data = _reader.Read(_entity);
                yield return new WaitForEndOfFrame();

                fileCache.CacheBinary(filename, data);
            }
            else
            {
                byte[] data = _reader.Read(_entity);
                string file = Path.Combine(Application.temporaryCachePath, filename);
                logger.Debug("write {0} to {1}", filename, file);
                File.WriteAllBytes(file, data);
            }
        }

        /*
        private static void registerCustomType()
        {
            JsonMapper.RegisterExporter<float>((obj, writer) => writer.Write(System.Convert.ToDouble(obj)));
            JsonMapper.RegisterImporter<double, float>(input => System.Convert.ToSingle(input));
            JsonMapper.RegisterExporter<Color>((obj, writer) => writer.Write(colorToLong(obj)));
            JsonMapper.RegisterImporter<int, Color>(input => intToColor(input));
            JsonMapper.RegisterImporter<long, Color>(input => longToColor(input));
        }

        private static void unregisterCustomType()
        {
            JsonMapper.UnregisterExporters();
            JsonMapper.UnregisterImporters();
        }
        */
        private static long colorToLong(Color _color)
        {
            long r = (long)(_color.r * 255);
            long g = (long)(_color.g * 255);
            long b = (long)(_color.b * 255);
            long a = (long)(_color.a * 255);

            long color = r << 24 | g << 16 | b << 8 | a;
            return color;
        }

        private static Color intToColor(int _color)
        {
            long r = (_color & 0xFF000000) >> 24;
            long g = (_color & 0x00FF0000) >> 16;
            long b = (_color & 0x0000FF00) >> 8;
            long a = (_color & 0x000000FF);
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        private static Color longToColor(long _color)
        {
            long r = (_color & 0xFF000000) >> 24;
            long g = (_color & 0x00FF0000) >> 16;
            long b = (_color & 0x0000FF00) >> 8;
            long a = (_color & 0x000000FF);
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }
    }


}
