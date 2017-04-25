using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    private Dictionary<string, AudioClip> sfx;

    public void Start() {
        // sound effects are loaded in the background via import settings
        SoundEffectData data = Global.Instance().Config.SoundEffects;

        sfx = new Dictionary<string, AudioClip>();
        foreach (SoundEffectDataEntry entry in data.data) {
            sfx[entry.Key] = entry.Clip;
        }

        gameObject.AddComponent<AudioSource>();
    }

    public void PlaySFX(string key) {
        AudioClip clip = sfx[key];
        StartCoroutine(SfxRoutine(clip));
    }

    private IEnumerator SfxRoutine(AudioClip clip) {
        while (clip.loadState == AudioDataLoadState.Loading) {
            yield return null;
        }
        if (clip.loadState == AudioDataLoadState.Loaded) {
            GetComponent<AudioSource>().clip = clip;
            GetComponent<AudioSource>().Play();
        }
    }
}
