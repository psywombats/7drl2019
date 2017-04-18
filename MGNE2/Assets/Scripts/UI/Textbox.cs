using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class Textbox : MonoBehaviour {
    
    private const float CharacterDelay = (1.0f / 100.0f);
    
    public Image backer;
    public Text textbox;
    public Image advancePrompt;
    
    private string fullText;

    public float Height {
        get { return GetComponent<RectTransform>().rect.height; }
    }

    private float AdvancePromptAlpha {
        get { return advancePrompt.GetComponent<CanvasRenderer>().GetAlpha(); }
        set { advancePrompt.GetComponent<CanvasRenderer>().SetAlpha(value); }
    }

    public void OnEnable() {
        Clear();
    }

    public void Clear() {
        textbox.text = "";
    }

    public static Textbox GetInstance() {
        return FindObjectOfType<Textbox>();
    }

    public void FadeAdvancePrompt(bool fadeIn) {
        if (advancePrompt != null) {
            // transitions?
            advancePrompt.CrossFadeAlpha(fadeIn ? 1.0f : 0.0f, 0, false);
        }
    }

    public IEnumerator ShowText(string text) {
        fullText = text;
        FadeAdvancePrompt(false);
        for (int i = 0; i <= fullText.Length; i += 1) {
            textbox.text = fullText.Substring(0, i);
            textbox.text += "<color=#00000000>";
            textbox.text += fullText.Substring(i);
            textbox.text += "</color>";
            yield return new WaitForSeconds(CharacterDelay);
        }
        textbox.text = fullText;
        
        FadeAdvancePrompt(true);
    }
}
