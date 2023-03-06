using System.Collections.Generic;
using UnityEngine;

namespace MeeX.XMA
{
    
    public class SceneObject
    {
        public string uuid = "";
        public string pack = "";
        public string name = "";
        public Vector3 position = Vector3.zero;
        public Vector3 rotation = Vector3.zero;
        public Vector3 scale = Vector3.zero;
    }

    public class SceneFog
    {
        public bool active = false;
        public Color color = new Color(128, 128, 128);
        public string mode = "ExponentialSquared";
        public float density = 0.01f;
        public float distanceStart = 0f;
        public float distanceEnd = 300f;
    }

    public class SceneAmbient
    {
        public string source = "Skybox";
        public Color color = new Color(54, 58, 66);
        public float intensity = 1f;
    }

    public class SceneMap
    {
        public List<SceneObject> objects = new List<SceneObject>();
        public SceneFog fog = new SceneFog();
        public SceneAmbient ambient = new SceneAmbient();
    }
}