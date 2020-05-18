using QGF.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate
{
    public class AudioManager:MonoBehaviour
    {
        class SoundTrack
        {
            public float StartTime;
            public float WaitTime;
            public string Sound;
        }
        private AudioSource MusicPlayer;
        private AudioSource SoundPlayer;
        private AudioSource UIPlayer;//ui音效

        private static Dictionary<string, AudioClip> mClips=new Dictionary<string, AudioClip>();

        private static List<SoundTrack> mTracks;//所有待播放音效

        private static string mMusicRootPath = "Music/";
        private static string mSoundRootPath = "Sound/";

        public static AudioManager Instance;



        private void Awake()
        {
            Instance = this;
            //初始化音乐
            AudioSource[] audios = GetComponents<AudioSource>();
            if (audios.Length < 3)
            {
                Debug.LogError("init audio error");
            }
            audios[0].loop = true;
            Init(audios[0], audios[1], audios[2]);

            mTracks = new List<SoundTrack>();
        }

        private void Start()
        {
            StartCoroutine(WaitForPlay());
        }

        private void Init(AudioSource music,AudioSource sound,AudioSource ui)
        {
            MusicPlayer = music;
            SoundPlayer = sound;
            UIPlayer = ui;
        }

        public void PlayMusic(string name)
        {
            AudioClip clip;
            if (mClips.ContainsKey(name))
            {
                clip = mClips[name];
            }
            else {
                clip= Resources.Load<AudioClip>(mMusicRootPath+name);
                mClips.Add(name, clip);
            }

            
            MusicPlayer.clip = clip;
            MusicPlayer.Play();
        }

        public void PlaySound(string name,float delay=0)
        {
            SoundTrack t = new SoundTrack();
            float current = Time.time;
            t.StartTime = current;
            t.WaitTime = delay;
            t.Sound = name;
            mTracks.Add(t);
        }

        public void PlayUISound(string name)
        {
            AudioClip clip;
            if (mClips.ContainsKey(name))
            {
                clip = mClips[name];
            }
            else
            {
                clip = Resources.Load<AudioClip>(mSoundRootPath + name);
                mClips.Add(name, clip);
            }


            UIPlayer.clip = clip;
            UIPlayer.PlayOneShot(clip);
        }

        private void _PlaySound(string name)
        {
            AudioClip clip;
            if (mClips.ContainsKey(name))
            {
                clip = mClips[name];
            }
            else
            {
                clip = Resources.Load<AudioClip>(mSoundRootPath + name);
                mClips.Add(name, clip);
            }


            SoundPlayer.clip = clip;
            SoundPlayer.PlayOneShot(clip);
        }

        IEnumerator WaitForPlay()
        {
            yield return new WaitForEndOfFrame();
            //等该帧结束时播放音乐
            while (true)
            {
                if (mTracks.Count > 0)
                {
                    float currentTime = Time.time;
                    for (int i = mTracks.Count - 1; i > 0; --i)
                    {
                        SoundTrack track = mTracks[i];
                        if (currentTime - track.StartTime >= track.WaitTime)
                        {
                            mTracks.RemoveAt(i);
                            _PlaySound(track.Sound);
                        }
                    }
                }
                yield return null;
            }
        }
    }
}
