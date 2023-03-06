using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace MeeX.XMA
{
    public class FileCache
    {
        public enum FileFormat
        {
            UNKNOW,
            IMAGE,
            AUDIO,
            VIDEO,
            MODEL,
        }

        public class Index
        {
            public string code = "";
            public string pack = "";
            public string file = "";
        }

        public class ScenenObject
        {
            public string pack = "";
            public string name = "";
            public Vector3 position = Vector3.zero;
            public Vector3 rotation = Vector3.zero;
            public Vector3 scale = Vector3.one;
        }

        public List<string> dynamic = new List<string>();

        // <code of asset, <pack of asset, name of asset>>
        private List<Index> indexTable = new List<Index>();
        private Dictionary<string, AssetBundle> assetbundles = new Dictionary<string, AssetBundle>();
        private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        private Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        private Dictionary<string, Material> materials = new Dictionary<string, Material>();
        private Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
        private Dictionary<string, AnimationClip> animationClips = new Dictionary<string, AnimationClip>();
        private Dictionary<string, byte[]> binaries = new Dictionary<string, byte[]>();


        public static FileFormat GetFileFormat(string _filename)
        {
            if (_filename.ToLower().EndsWith(".jpg") || _filename.ToLower().EndsWith(".png"))
                return FileFormat.IMAGE;
            if (_filename.ToLower().EndsWith(".wav") || _filename.ToLower().EndsWith(".acv"))
                return FileFormat.AUDIO;
            if (_filename.ToLower().EndsWith(".fbx") || _filename.ToLower().EndsWith(".stl"))
                return FileFormat.MODEL;
            return FileFormat.UNKNOW;
        }

        
        public void Clean()
        {
            foreach(AnimationClip ac in animationClips.Values)
            {
                if(null == ac)
                    continue;
                GameObject.DestroyImmediate(ac);
            }
            animationClips.Clear();

            foreach(AudioClip ac in audioClips.Values)
            {
                if(null == ac)
                    continue;
                ac.UnloadAudioData();
                GameObject.DestroyImmediate(ac);
            }
            audioClips.Clear();

            foreach(GameObject go in prefabs.Values)
            {
                if(null == go)
                    continue;
                GameObject.DestroyImmediate(go);
            }
            prefabs.Clear();

            foreach(Material mat in materials.Values)
            {
                if(null == mat)
                    continue;
                GameObject.DestroyImmediate(mat);
            }
            materials.Clear();

            foreach(Texture2D tt in textures.Values)
            {
                if(null == tt)
                    continue;
                GameObject.DestroyImmediate(tt);
            }
            textures.Clear();

            foreach(Sprite sprite in sprites.Values)
            {
                if(null == sprite)
                    continue;
                GameObject.DestroyImmediate(sprite);
            }
            sprites.Clear();


            foreach(AssetBundle ab in assetbundles.Values)
            {
                ab.Unload(true);
            }
            assetbundles.Clear();

            binaries.Clear();

            indexTable.Clear();

            dynamic.Clear();

            Resources.UnloadUnusedAssets();
        }

        public void UpdateIndex(List<Index> _index)
        {
            indexTable = _index;
        }

        public Index FindIndex(string _assetCode)
        {
            Index index = indexTable.Find((_item) =>
            {
                return _item.code.Equals(_assetCode);
            });
            return index;
        }

        public void CacheBinary(string _name, byte[] _binary)
        {
            binaries[_name] = _binary;
        }

        public bool HasBinary(string _name)
        {
            return binaries.ContainsKey(_name);
        }

        public byte[] AccessBinary(string _name)
        {
            byte[] binaray = null;
            binaries.TryGetValue(_name, out binaray);
            return binaray;
        }

        public void CacheAssetBundle(string _name, AssetBundle _assetbundle)
        {
            assetbundles[_name] = _assetbundle;
        }

        public bool HasAssetBundle(string _name)
        {
            return assetbundles.ContainsKey(_name);
        }

        public AssetBundle AccessAssetBundle(string _name)
        {
            AssetBundle ab = null;
            assetbundles.TryGetValue(_name, out ab);
            return ab;
        }

        public void CacheTexture(string _name, Texture2D _texture)
        {
            textures[_name] = _texture;
        }

        public bool HasTexture(string _name)
        {
            return textures.ContainsKey(_name);
        }

        public Texture2D AccessTexture(string _name)
        {
            if (!textures.ContainsKey(_name))
                return null;
            return textures[_name];
        }

        public void CacheSprite(string _name, Sprite _sprite)
        {
            sprites[_name] = _sprite;
        }

        public bool HasSprite(string _name)
        {
            return sprites.ContainsKey(_name);
        }

        public Sprite AccessSprite(string _name)
        {
            if (!sprites.ContainsKey(_name))
                return null;
            return sprites[_name];
        }

        public void CacheMaterial(string _name, Material _material)
        {
            materials[_name] = _material;
        }

        public bool HasMaterial(string _name)
        {
            return materials.ContainsKey(_name);
        }

        public Material AccessMaterial(string _name)
        {
            if (!materials.ContainsKey(_name))
                return null;
            return materials[_name];
        }

        public void CachePrefab(string _name, GameObject _prefab)
        {
            //this.LogDebug("cache prefab {0}", _name);
            prefabs[_name] = _prefab;
        }

        public bool HasPrefab(string _name)
        {
            return prefabs.ContainsKey(_name);
        }

        public GameObject AccessPrefab(string _name)
        {
            if (!prefabs.ContainsKey(_name))
                return null;
            return prefabs[_name];
        }

        public void CacheAudioClip(string _name, AudioClip _audioClip)
        {
            audioClips[_name] = _audioClip;
        }

        public bool HasAudioClip(string _name)
        {
            return audioClips.ContainsKey(_name);
        }

        public AudioClip AccessAudioClip(string _name)
        {
            if (!audioClips.ContainsKey(_name))
                return null;
            return audioClips[_name];
        }

        public void CacheAnimationClip(string _name, AnimationClip _animationClip)
        {
            //this.LogDebug("cache animation {0}", _name);
            animationClips[_name] = _animationClip;
        }

        public bool HasAnimationClip(string _name)
        {
            return animationClips.ContainsKey(_name);
        }

        public AnimationClip AccessAnimationClip(string _name)
        {
            if (!animationClips.ContainsKey(_name))
                return null;
            return animationClips[_name];
        }

        public static string ToUUID(string _text)
        {
            MD5 md5 = MD5.Create();
            byte[] byteOld = Encoding.UTF8.GetBytes(_text);
            byte[] byteNew = md5.ComputeHash(byteOld);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

    }
}
