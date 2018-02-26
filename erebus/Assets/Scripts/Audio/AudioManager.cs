using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour, MemoryPopulater {

    private const string NoBGMKey = "none";
    private const float FadeSeconds = 0.5f;

    private AudioSource sfxSource;
    private AudioSource bgmSource;

    private Setting<float> bgmVolumeSetting;
    private Setting<float> sfxVolumeSetting;

    public string CurrentBGMKey { get; private set; }

    public void Awake() {
        Global.Instance().Memory.RegisterMemoryPopulater(this);

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;

        CurrentBGMKey = NoBGMKey;
        
        sfxVolumeSetting = Global.Instance().Settings.GetFloatSetting(SettingsConstants.SoundEffectVolume);
        bgmVolumeSetting = Global.Instance().Settings.GetFloatSetting(SettingsConstants.BGMVolume);

        gameObject.AddComponent<WaveSource>();
    }

    public void Update() {
        bgmSource.volume = bgmVolumeSetting.Value;
        sfxSource.volume = sfxVolumeSetting.Value;
    }

    public void PlaySFX(string key) {
        AudioClip clip = Global.Instance().Database.SFX.GetData(key).clip;
        StartCoroutine(PlaySFXRoutine(sfxSource, clip));
    }

    public void PlayBGM(string key) {
        if (key != CurrentBGMKey) {
            CurrentBGMKey = key;
            if (key == null || key == NoBGMKey) {
                bgmSource.Stop();
            } else {
                bgmSource.volume = 1.0f;
                AudioClip clip = Global.Instance().Database.BGM.GetData(key).track;
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

    public IEnumerator CrossfadeRoutine(string tag) {
        if (CurrentBGMKey != null && CurrentBGMKey != NoBGMKey) {
            yield return FadeOutRoutine(FadeSeconds);
        }
        PlayBGM(tag);
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
