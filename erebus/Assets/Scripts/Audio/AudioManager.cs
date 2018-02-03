using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour, MemoryPopulater {

    private const string NoBGMKey = "none";

    private Dictionary<string, AudioClip> sfx;
    private Dictionary<string, AudioClip> bgm;

    private AudioSource sfxSource;
    private AudioSource bgmSource;

    public string CurrentBGMKey { get; private set; }

    public void Start() {
        // sound effects are loaded in the background via import settings

        Global.Instance().Memory.RegisterMemoryPopulater(this);

        sfx = new Dictionary<string, AudioClip>();
        foreach (AudioKeyDataEntry entry in Global.Instance().Config.SoundEffects.data) {
            sfx[entry.Key] = entry.Clip;
        }

        bgm = new Dictionary<string, AudioClip>();
        foreach (AudioKeyDataEntry entry in Global.Instance().Config.BackgroundMusic.data) {
            bgm[entry.Key] = entry.Clip;
        }

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;

        CurrentBGMKey = NoBGMKey;
    }

    public void Update() {
        PlayBGM("wavetest");
        if (GetComponent<WaveSource>() == null) {
            gameObject.AddComponent<WaveSource>();
        }
    }

    public void PlaySFX(string key) {
        AudioClip clip = sfx[key];
        StartCoroutine(PlaySFXRoutine(sfxSource, clip));
    }

    public void PlayBGM(string key) {
        if (key != CurrentBGMKey) {
            CurrentBGMKey = key;
            if (key == null || key == NoBGMKey) {
                bgmSource.Stop();
            } else {
                bgmSource.volume = 1.0f;
                AudioClip clip = bgm[key];
                bgmSource.clip = clip;
                bgmSource.Play();
            }
        }
    }

    public AudioClip BGMClip() {
        return bgmSource.clip;
    }

    public WaveSource GetWaveSource() {
        return GetComponent<WaveSource>();
    }

    public IEnumerator FadeOutRoutine(float durationSeconds) {
        CurrentBGMKey = NoBGMKey;
        while (bgmSource.volume > 0.0f) {
            bgmSource.volume -= Time.deltaTime / durationSeconds;
            if (bgmSource.volume < 0.0f) {
                bgmSource.volume = 0.0f;
            }
            yield return null;
        }
    }

    public void PopulateMemory(Memory memory) {
        memory.bgmKey = CurrentBGMKey;
    }

    public void PopulateFromMemory(Memory memory) {
        PlayBGM(memory.bgmKey);
    }

    private IEnumerator PlaySFXRoutine(AudioSource source, AudioClip clip) {
        while (clip.loadState == AudioDataLoadState.Loading) {
            yield return null;
        }
        if (clip.loadState == AudioDataLoadState.Loaded) {
            source.clip = clip;
            source.Play();
        }
    }
}
