using System.Collections.Generic;
using UnityEngine;

namespace MeeX.XMA
{

    public class StoryModel
    {
        public class Theme
        {
            public string skybox = "";
            public double rotation = 0f;
            public string environment = "";
            public string music = "";
            public double volume = 1.0f;
            public bool loop = true;
        }

        public class MaterialStack
        {
            public string id = "";
            public string name = "";
            public double slider = 0f;
            public Color color = Color.white;
        }

        public class Agent
        {
            public string uuid = "";
            public string name = "";
            public bool visible = true;
            public double px = 0f;
            public double py = 0f;
            public double pz = 0f;
            public double rx = 0f;
            public double ry = 0f;
            public double rz = 0f;
            public double sx = 1f;
            public double sy = 1f;
            public double sz = 1f;
        }

        public class HotspotAgent : Agent
        {
            public bool nameVisible = true;
            public int style = 0;
            public string blocklyName = "";
            public string blocklyUUID = "";
        }

        public class TextBoardAgent : Agent
        {
            public string content = "";
            public int width = 256;
            public int height = 256;
        }

        public class PictureBoardAgent : Agent
        {
            public string[] files = new string[1] { "" };
            public int width = 256;
            public int height = 256;
            public float backgroundAlpha = 1.0f;
            public int count = 1;
            public float interval = 1;
        }

        public class VideoBoardAgent : Agent
        {
            public string file = "";
            public int width = 256;
            public int height = 256;
            public float backgroundAlpha = 1.0f;
            public float volume = 1.0f;
            public bool autoPlay = false;
        }

        public class ModelAgent : Agent
        {
            public string assetCode = "";
            public string filename = "";
            public Dictionary<string, MaterialStack> materials = new Dictionary<string, MaterialStack>();
        }

        public class CharacterAgent : Agent
        {
            public string assetCode = "";
            public string internalAnimation = "";
            public string externalAnimation = "";
            public Dictionary<string, MaterialStack> materials = new Dictionary<string, MaterialStack>();
        }

        public class CameraAgent : Agent
        {
        }

        public class EffectAgent : Agent
        {
            public string assetCode = "";
            public string filename = "";
        }


        public class Story
        {
            public string UUID = "";
            public string ProjectUUID = "";
            public string Name = "";
            public int Index = 0;

            public Theme theme = new Theme();
            public List<CameraAgent> cameras = new List<CameraAgent>();
            public List<HotspotAgent> hotspots = new List<HotspotAgent>();
            public List<TextBoardAgent> textboards = new List<TextBoardAgent>();
            public List<PictureBoardAgent> pictureboards = new List<PictureBoardAgent>();
            public List<VideoBoardAgent> videoboards = new List<VideoBoardAgent>();
            public List<ModelAgent> models = new List<ModelAgent>();
            public List<CharacterAgent> characters = new List<CharacterAgent>();
            public List<EffectAgent> effects = new List<EffectAgent>();
        }
    }
}
