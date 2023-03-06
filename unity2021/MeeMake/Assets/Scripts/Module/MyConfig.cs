
using System.Xml.Serialization;

namespace XTC.FMP.MOD.MeeMake.LIB.Unity
{
    /// <summary>
    /// 配置类
    /// </summary>
    public class MyConfig : MyConfigBase
    {
        public class Loading
        {
            [XmlAttribute("background")]
            public string background { get; set; } = "#282828FF";
        }

        public class Viewport
        {
            [XmlAttribute("x")]
            public float x { get; set; }
            [XmlAttribute("y")]
            public float y { get; set; }
            [XmlAttribute("w")]
            public float w { get; set; }
            [XmlAttribute("h")]
            public float h { get; set; }
        }

        public class Camera
        {
            [XmlAttribute("provide")]
            public bool provide { get; set; }
            [XmlAttribute("depth")]
            public int depth { get; set; }
            [XmlElement("Viewport")]
            public Viewport viewport { get; set; }
        }

        public class Style
        {
            [XmlAttribute("name")]
            public string name { get; set; } = "";

            [XmlElement("Camera")]
            public Camera camera { get; set; } = new Camera();
            [XmlElement("Loading")]
            public Loading loading { get; set; } = new Loading();

        }


        [XmlArray("Styles"), XmlArrayItem("Style")]
        public Style[] styles { get; set; } = new Style[0];
    }
}

