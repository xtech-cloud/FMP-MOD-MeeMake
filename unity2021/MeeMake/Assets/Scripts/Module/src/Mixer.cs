using System.Collections.Generic;
using UnityEngine;

namespace MeeX.XMA
{
    public class Mixer
    {
        private Dictionary<string, AudioSource> tracks = new Dictionary<string, AudioSource>();
        private FileCache fileCache_ {get;set;}
        private GameObject spaceRoot_ { get; set; }

        public void Initialize(GameObject _spaceRoot)
        {
            spaceRoot_ = _spaceRoot;
            // 背景音乐音轨，空字符
            CreateAudioSource("");
        }

        public void Release()
        {
            foreach(AudioSource source in tracks.Values)
            {
                GameObject.Destroy(source.gameObject);
            }
            tracks.Clear();
        }
        
        public void Setup(FileCache _fileCache)
        {
            fileCache_ = _fileCache;
        }

        public void Render(StoryModel.Story _story)
        {
            foreach(AudioSource source in tracks.Values)
            {
                source.Stop();
            }
        }

        public AudioSource FindAudioSource(string _track)
        {
            AudioSource source;
            tracks.TryGetValue(_track, out source);
            return source;
        }

        public AudioSource CreateAudioSource(string _track)
        {
            AudioSource source;
            if(!tracks.TryGetValue(_track, out source))
            {
                GameObject go = new GameObject("mixer_" + _track);
                go.transform.SetParent(spaceRoot_.transform);
                go.transform.localPosition = Vector3.zero;
                source = go.AddComponent<AudioSource>();
                tracks[_track] = source;
            }

            return source;
        }

        public void MountAudio(string _audio, AudioSource _audioSource)
        {
            if(null == _audioSource)
                return;
                
            if (FileCache.FileFormat.AUDIO == FileCache.GetFileFormat(_audio.ToLower()))
            {
                AudioClip audioClip = fileCache_.AccessAudioClip(_audio);
                if (null == audioClip)
                    return;
                _audioSource.clip = audioClip;
            }
            else
            {

            }
        }
    }
}