using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using LibMVCS = XTC.FMP.LIB.MVCS;
using XTC.FMP.MOD.MeeMake.LIB.Proto;
using XTC.FMP.MOD.MeeMake.LIB.MVCS;
using System.Collections;
using System.IO;

namespace XTC.FMP.MOD.MeeMake.LIB.Unity
{
    /// <summary>
    /// 实例类
    /// </summary>
    public class MyInstance : MyInstanceBase
    {
        public class Picture
        {
            public int index;
            public Sprite imageLayer;
        }

        public class UiReference
        {
            public GameObject objToolbar;
            public Text textTipError;
            public GameObject objLoading;
            public List<GameObject> storyThumbAry = new List<GameObject>();
        }

        private UiReference uiReference_ { get; set; } = new UiReference();

        private Camera camera_;
        private MeeX.XMA.Runtime runtime_;
        private Coroutine playCoroutine_;

        public MyInstance(string _uid, string _style, MyConfig _config, MyCatalog _catalog, LibMVCS.Logger _logger, Dictionary<string, LibMVCS.Any> _settings, MyEntryBase _entry, MonoBehaviour _mono, GameObject _rootAttachments)
            : base(_uid, _style, _config, _catalog, _logger, _settings, _entry, _mono, _rootAttachments)
        {
        }

        /// <summary>
        /// 当被创建时
        /// </summary>
        /// <remarks>
        /// 可用于加载主题目录的数据
        /// </remarks>
        public void HandleCreated()
        {
            create();
        }

        /// <summary>
        /// 当被删除时
        /// </summary>
        public void HandleDeleted()
        {
        }

        /// <summary>
        /// 当被打开时
        /// </summary>
        /// <remarks>
        /// 可用于加载内容目录的数据
        /// </remarks>
        public void HandleOpened(string _source, string _uri)
        {
            play(_source, _uri);
        }

        /// <summary>
        /// 当被关闭时
        /// </summary>
        public void HandleClosed()
        {
            stop();
        }

        private void create()
        {
            uiReference_.objToolbar = rootUI.transform.Find("toolbar").gameObject;
            uiReference_.textTipError = rootUI.transform.Find("loading/txtError").GetComponent<Text>();
            uiReference_.textTipError.gameObject.SetActive(false);
            uiReference_.objLoading = rootUI.transform.Find("loading").gameObject;
            uiReference_.objLoading.SetActive(false);

            Color colorBackground;
            if (UnityEngine.ColorUtility.TryParseHtmlString(style_.loading.background, out colorBackground))
            {
                var imgLoading = uiReference_.objLoading.GetComponent<Image>();
                imgLoading.color = colorBackground;
            }

            if (style_.camera.provide)
            {
                camera_ = rootWorld.transform.Find("Camera").GetComponent<Camera>();
                camera_.transform.SetParent(rootWorld.transform);
                camera_.transform.localPosition = Vector3.zero;
                camera_.rect = new Rect(style_.camera.viewport.x, style_.camera.viewport.y, style_.camera.viewport.w, style_.camera.viewport.h);
                camera_.depth = style_.camera.depth;
                Rect rect = camera_.pixelRect;
                wrapSwipeCamera(camera_.transform);
            }

            rootUI.transform.Find("btnConsoleLeft").GetComponent<Button>().onClick.AddListener(() =>
            {
                uiReference_.objToolbar.GetComponent<Animator>().SetTrigger("switch");
            });
            rootUI.transform.Find("btnConsoleRight").GetComponent<Button>().onClick.AddListener(() =>
            {
                uiReference_.objToolbar.GetComponent<Animator>().SetTrigger("switch");
            });

            runtime_ = new MeeX.XMA.Runtime();
            runtime_.mono = mono_;
            runtime_.logger = logger_;
            runtime_.options = new MeeX.XMA.Options();
            runtime_.onPreloadReady = (_value) =>
            {

            };
            runtime_.onPreloadChanged = (_value) =>
            {

            };
            runtime_.onPreloadError = (_err) =>
            {
                uiReference_.textTipError.text = _err.getMessage();
                uiReference_.textTipError.gameObject.SetActive(true);
            };
            runtime_.onPreloadFinish = () =>
            {
                onPreloadFinish();
                uiReference_.objLoading.SetActive(false);
                uiReference_.objToolbar.SetActive(true);
                if (null != camera_)
                    camera_.gameObject.SetActive(true);
            };
            runtime_.camera = camera_ == null ? Camera.main.transform : camera_.transform;
            runtime_.slotCanvas2D = rootUI.transform.Find("[slot]");
            var canvas3D = rootWorld.transform.Find("Canvas3D").GetComponent<Canvas>();
            canvas3D.worldCamera = camera_;
            runtime_.slotCanvas3D = canvas3D.transform;
        }

        void play(string _source, string _uri)
        {
            rootUI.SetActive(true);
            rootWorld.SetActive(true);
            uiReference_.textTipError.gameObject.SetActive(false);
            uiReference_.objLoading.SetActive(true);
            // 隐藏后toogle的isOn会出问题
            uiReference_.objToolbar.SetActive(true);

            if (null != camera_)
                camera_.gameObject.SetActive(false);

            string uri = _uri;
            if (_source == "assloud://")
            {
                uri = Path.Combine(settings_["path.assets"].AsString(), _uri.Replace(".xma", "@"+settings_["platform"].AsString() + ".xma"));
            }
            runtime_.Initialize(rootWorld);
            // 移动Canvas3D到世界坐标原点，加载完成后，再移动到spaceRoot的原点
            runtime_.slotCanvas3D.position = Vector3.zero;
            runtime_.Preload(uri);
        }

        void stop()
        {
            runtime_.Release();
            rootUI.SetActive(false);
            rootWorld.SetActive(false);
            foreach (var storyThumb in uiReference_.storyThumbAry)
                GameObject.Destroy(storyThumb);
            uiReference_.storyThumbAry.Clear();
        }

        private void onPreloadFinish()
        {
            GameObject templateStory = uiReference_.objToolbar.transform.Find("storyContainer/Viewport/Content/tgStory").gameObject;
            templateStory.SetActive(false);
            Toggle firstToggle = null;
            foreach (var story in runtime_.stories)
            {
                GameObject goStory = GameObject.Instantiate(templateStory, templateStory.transform.parent);
                uiReference_.storyThumbAry.Add(goStory);
                goStory.name = story;
                goStory.SetActive(true);
                Sprite sprite = null;
                if (runtime_.TryGetStoryThumb(story, out sprite))
                    goStory.transform.Find("thumb").GetComponent<Image>().sprite = sprite;
                goStory.transform.Find("Label").GetComponent<Text>().text = story;
                var toggle = goStory.GetComponent<Toggle>();
                toggle.onValueChanged.AddListener((_toggled) =>
                {
                    runtime_.RenderStory(goStory.name);
                });
                if (null == firstToggle)
                    firstToggle = toggle;
            }

            if (null != firstToggle)
                firstToggle.isOn = true;

            runtime_.slotCanvas3D.localPosition = Vector3.zero;
            runtime_.Execute();
        }

        private void wrapSwipeCamera(Transform _camera)
        {
            var camera = _camera.GetComponent<Camera>();
            var swipeH = _camera.gameObject.AddComponent<HedgehogTeam.EasyTouch.QuickSwipe>();
            swipeH.swipeDirection = HedgehogTeam.EasyTouch.QuickSwipe.SwipeDirection.Horizontal;
            swipeH.onSwipeAction = new HedgehogTeam.EasyTouch.QuickSwipe.OnSwipeAction();
            swipeH.onSwipeAction.AddListener((_gesture) =>
            {
                // 忽略摄像机视窗外
                if (_gesture.position.x < camera.pixelRect.x ||
                _gesture.position.x > camera.pixelRect.x + camera.pixelRect.width ||
                _gesture.position.y < camera.pixelRect.y ||
                _gesture.position.y > camera.pixelRect.y + camera.pixelRect.height)
                    return;
                var vec = _camera.localRotation.eulerAngles;
                vec.y = vec.y + _gesture.swipeVector.x;
                _camera.localRotation = Quaternion.Euler(vec.x, vec.y, vec.z);
            });
            var swipeV = _camera.gameObject.AddComponent<HedgehogTeam.EasyTouch.QuickSwipe>();
            swipeV.swipeDirection = HedgehogTeam.EasyTouch.QuickSwipe.SwipeDirection.Vertical;
            swipeV.onSwipeAction = new HedgehogTeam.EasyTouch.QuickSwipe.OnSwipeAction();
            swipeV.onSwipeAction.AddListener((_gesture) =>
            {
                // 忽略摄像机视窗外
                if (_gesture.position.x < camera.pixelRect.x ||
                _gesture.position.x > camera.pixelRect.x + camera.pixelRect.width ||
                _gesture.position.y < camera.pixelRect.y ||
                _gesture.position.y > camera.pixelRect.y + camera.pixelRect.height)
                    return;
                var vec = _camera.localRotation.eulerAngles;
                vec.x = vec.x - _gesture.swipeVector.y;
                if (vec.x > 70 && vec.x < 180)
                    vec.x = 70;
                if (vec.x < 290 && vec.x > 180)
                    vec.x = 290;
                _camera.rotation = Quaternion.Euler(vec.x, vec.y, vec.z);
            });
        }
    }
}
