using System.Collections;
using UnityEngine;
using MeeX.XMA;
using RenderHeads.Media.AVProVideo;

namespace MeeX.MeeMake
{
    public interface IStoryProxy
    {
        void JumpStory(string _storyName);
    }

    public class StoryAPI
    {
        public MonoBehaviour mono;
        public CharacterRender characterRender { get; set; }
        public ModelRender modelRender { get; set; }
        public HotspotRender hotspotRender { get; set; }
        public TextBoardRender textBoardRender { get; set; }
        public PictureBoardRender pictureBoardRender { get; set; }
        public VideoBoardRender videoBoardRender { get; set; }
        public EffectRender effectRender { get; set; }
        public CameraRender cameraRender { get; set; }
        public Mixer mixer { get; set; }
        public Transform activeCamera { get; set; }
        public IStoryProxy proxy { get; set; }


        public GameObject FindCharacterAgent(string _uuid)
        {
            Transform agent = characterRender.FindCharacter(_uuid);
            if (null == agent)
                return null;
            return agent.gameObject;
        }

        
        public GameObject FindModelAgent(string _uuid)
        {
            Transform agent = modelRender.FindModel(_uuid);
            if (null == agent)
                return null;
            return agent.gameObject;
        }

        public GameObject FindHotspotAgent(string _uuid)
        {
            Transform agent = hotspotRender.FindHotspot(_uuid);
            if (null == agent)
                return null;
            return agent.gameObject;
        }

        public GameObject FindTextBoardAgent(string _uuid)
        {
            Transform agent = textBoardRender.FindTextBoard(_uuid);
            if (null == agent)
                return null;
            return agent.gameObject;
        }

        public GameObject FindPictureBoardAgent(string _uuid)
        {
            Transform agent = pictureBoardRender.FindPictureBoard(_uuid);
            if (null == agent)
                return null;
            return agent.gameObject;
        }

        public GameObject FindVideoBoardAgent(string _uuid)
        {
            Transform agent = videoBoardRender.FindVideoBoard(_uuid);
            if (null == agent)
                return null;
            return agent.gameObject;
        }

        public GameObject FindEffectAgent(string _uuid)
        {
            Transform agent = effectRender.FindEffect(_uuid);
            if (null == agent)
                return null;
            return agent.gameObject;
        }


        public GameObject FindCameraAgent(string _uuid)
        {
            Transform agent = cameraRender.FindCamera(_uuid);
            if (null == agent)
                return null;
            return agent.gameObject;
        }

        public AudioSource FindAudioSource(string _track)
        {
            return mixer.FindAudioSource(_track);
        }


        public AudioSource CreateAudioSource(string _track)
        {
            return mixer.CreateAudioSource(_track);
        }

        public void MountAudio(string _audio, AudioSource _audioSource, System.Action _onFinish, System.Action<string> _onError)
        {
            mono.StartCoroutine(mountAudio(_audio, _audioSource, _onFinish, _onError));
        }

        
        public void PlayVideo(GameObject _agent)
        {
            MediaPlayer player = _agent.GetComponent<MediaPlayer>();
            if(null == player)
                return;
            player.Play();
        }

        public void StopVideo(GameObject _agent)
        {
            MediaPlayer player = _agent.GetComponent<MediaPlayer>();
            if(null == player)
                return;
            player.Rewind(true);
            player.Stop();
        }

        public void PauseVideo(GameObject _agent)
        {
            MediaPlayer player = _agent.GetComponent<MediaPlayer>();
            if(null == player)
                return;
            player.Pause();
        }

        public void UnpauseVideo(GameObject _agent)
        {
            MediaPlayer player = _agent.GetComponent<MediaPlayer>();
            if(null == player)
                return;
            player.Play();
        }



        public GameObject GetActiveCamera()
        {
            return activeCamera.gameObject;
        }

        public void JumpStory(string _storyName)
        {
            proxy.JumpStory(_storyName);
        }

        private IEnumerator mountAudio(string _audio, AudioSource _audioSource, System.Action _onFinish, System.Action<string> _onError )
        {
             //此行对于lua携程的正确执行是必须的
            yield return new WaitForEndOfFrame();

            if (string.IsNullOrEmpty(_audio))
            {
                _onError("_audio is null or empty");
                yield break;
            }
            if (null == _audioSource)
            {
                _onError("_audioSource is null");
                yield break;
            }

            mixer.MountAudio(_audio, _audioSource);
            _onFinish();
        }
    }
}//namespace
