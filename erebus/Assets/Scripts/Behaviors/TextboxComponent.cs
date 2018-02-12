using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class TextboxComponent : MonoBehaviour {

    private const float CharacterDelayMax = (1.0f / 20.0f);
    private const float CharacterDelayMin = (1.0f / 200.0f);
    private const float AutoMinDelayMult = 0.0f;
    private const float AutoMaxDelayMult = .05f;
    private const float AutoBaseDelay = 0.8f;
    private const float AdvancePromptFadeOutSeconds = 0.15f;
    private const float AdvancePromptFadeInSeconds = 0.3f;
    private const float FastModeHiccupSeconds = 0.05f;

    public Shader shader;
    public TextboxBackerComponent backer;
    public Text textbox;
    public Image advancePrompt;
    public SpeakerComponent speaker;
    public QuickMenuComponent quickMenu;

    private Setting<float> characterSpeedSetting;
    private Setting<float> autoSpeedSetting;
    private string fullText;
    private bool paused;

    public float Height {
        get { return GetComponent<RectTransform>().rect.height; }
    }

    public bool Paused {
        get { return paused; }
    }

    private float AdvancePromptAlpha {
        get { return advancePrompt.GetComponent<CanvasRenderer>().GetAlpha(); }
        set { advancePrompt.GetComponent<CanvasRenderer>().SetAlpha(value); }
    }

    public void Awake() {
        characterSpeedSetting = Global.Instance().Settings.GetFloatSetting(SettingsConstants.TextSpeed);
        autoSpeedSetting = Global.Instance().Settings.GetFloatSetting(SettingsConstants.AutoSpeed);
    }

    public void OnEnable() {
        Clear();
        if (speaker != null) speaker.GetComponent<FadingUIComponent>().SetAlpha(0.0f);
        if (backer != null) backer.GetComponent<FadingUIComponent>().SetAlpha(0.0f);
        if (quickMenu != null) quickMenu.GetComponent<FadingUIComponent>().SetAlpha(0.0f);
        if (advancePrompt != null) AdvancePromptAlpha = 0.0f;
    }
    
    public void Clear() {
        textbox.text = "";
    }

    public void FadeAdvancePrompt(bool fadeIn, float seconds = AdvancePromptFadeOutSeconds) {
        if (advancePrompt != null) {
            advancePrompt.CrossFadeAlpha(fadeIn ? 1.0f : 0.0f, seconds, false);
        }
    }

    public IEnumerator ShowText(ScenePlayer player, string text, bool waitUntilAcknowledged) {
        fullText = text;
        FadeAdvancePrompt(false);
        for (int i = 0; i <= fullText.Length; i += 1) {
            if (Paused) {
                yield return null;
            }
            if (player.WasHurried()) {
                player.AcknowledgeHurried();
                textbox.text = fullText;
                break;
            }
            if (player.ShouldUseFastMode()) {
                break;
            }
            textbox.text = fullText.Substring(0, i);
            textbox.text += "<color=#00000000>";
            textbox.text += fullText.Substring(i);
            textbox.text += "</color>";
            yield return new WaitForSeconds(GetCharacterDelay());
        }
        textbox.text = fullText;
        if (player.ShouldUseFastMode()) {
            yield return new WaitForSeconds(FastModeHiccupSeconds);
        }

        if (!Paused && advancePrompt != null) {
            advancePrompt.gameObject.SetActive(true);
            AdvancePromptAlpha = 0.0f;
            FadeAdvancePrompt(true);
        }

        if (waitUntilAcknowledged && !player.ShouldUseFastMode()) {
            if (player.AutoMode) {
                float delay = AutoBaseDelay + fullText.Length * GetAutoDelayMult();
                yield return new WaitForSeconds(delay);
            } else {
                yield return player.AwaitHurry();
            }
        }
    }

    public IEnumerator FadeInRoutine(ScenePlayer player, float durationSeconds) {
        if (!gameObject.activeInHierarchy) {
            yield break;
        }
        FadeAdvancePrompt(true, durationSeconds);
        List<IEnumerator> toRun = new List<IEnumerator>();
        if (speaker != null) toRun.Add(speaker.FadeInRoutine(durationSeconds));
        if (backer != null) toRun.Add(backer.FadeInRoutine(durationSeconds));
        if (quickMenu != null) toRun.Add(quickMenu.FadeInRoutine(durationSeconds));
        yield return player.StartCoroutine(CoUtils.RunParallel(toRun.ToArray(), player));
    }

    public IEnumerator FadeOutRoutine(ScenePlayer player, float durationSeconds) {
        if (!gameObject.activeInHierarchy) {
            yield break;
        }
        paused = true;
        FadeAdvancePrompt(false, durationSeconds);
        List<IEnumerator> toRun = new List<IEnumerator>();
        if (speaker != null) toRun.Add(speaker.FadeOutRoutine(durationSeconds));
        if (backer != null) toRun.Add(backer.FadeOutRoutine(durationSeconds));
        if (quickMenu != null) toRun.Add(quickMenu.FadeOutRoutine(durationSeconds));
        yield return player.StartCoroutine(CoUtils.RunParallel(toRun.ToArray(), player));
    }

    public IEnumerator Activate(ScenePlayer player) {
        if (gameObject.activeInHierarchy) {
            yield break;
        }
        paused = false;
        gameObject.SetActive(true);
        List<IEnumerator> toRun = new List<IEnumerator>();
        if (speaker != null) toRun.Add(speaker.Activate(player));
        if (backer != null) toRun.Add(backer.Activate(player));
        if (quickMenu != null) toRun.Add(quickMenu.Activate(player));
        yield return player.StartCoroutine(CoUtils.RunParallel(toRun.ToArray(), player));
    }

    public IEnumerator Deactivate(ScenePlayer player) {
        if (!gameObject.activeInHierarchy) {
            yield break;
        }

        FadeAdvancePrompt(false);
        List<IEnumerator> toRun = new List<IEnumerator>();
        if (speaker != null) toRun.Add(speaker.Deactivate(player));
        if (backer != null) toRun.Add(backer.Deactivate(player));
        if (quickMenu != null) toRun.Add(quickMenu.Deactivate(player));
        yield return player.StartCoroutine(CoUtils.RunParallel(toRun.ToArray(), player));
        gameObject.SetActive(false);
    }

    private float GetCharacterDelay() {
        return CharacterDelayMax + ((CharacterDelayMin - CharacterDelayMax) * characterSpeedSetting.Value);
    }

    // in seconds/character
    private float GetAutoDelayMult() {
        return AutoMaxDelayMult + ((AutoMinDelayMult - AutoMaxDelayMult) * autoSpeedSetting.Value);
    }
}
