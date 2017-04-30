using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Assertions;

[RequireComponent(typeof(CanvasGroup))]
public class Textbox : MonoBehaviour, InputListener {
    
    private const float CharacterDelay = (1.0f / 100.0f);
    
    public Image backer;
    public Text textbox;
    
    public bool Visible { get { return GetComponent<CanvasGroup>().alpha == 1.0f; } }

    private static Textbox instance;

    private string fullText;
    private bool hurried;

    public void OnEnable() {
        Clear();
    }

    public void Clear() {
        textbox.text = "";
    }

    public static Textbox GetInstance() {
        if (instance == null) {
            instance = FindObjectOfType<Textbox>();
        }
        return instance;
    }

    public IEnumerator ShowText(string text) {
        Clear();
        hurried = false;
        Global.Instance().Input.PushListener(this);
        if (!Visible) {
            yield return TransitionIn();
        }
        fullText = text;
        for (int i = 0; i <= fullText.Length; i += 1) {
            if (hurried) {
                break;
            }
            textbox.text = fullText.Substring(0, i);
            textbox.text += "<color=#00000000>";
            textbox.text += fullText.Substring(i);
            textbox.text += "</color>";
            yield return new WaitForSeconds(CharacterDelay);
        }
        textbox.text = fullText;
        yield return Global.Instance().Input.AwaitConfirm();
        Global.Instance().Input.RemoveListener(this);
    }

    public IEnumerator TransitionIn() {
        Clear();

        // placeholder
        while (GetComponent<CanvasGroup>().alpha < 1.0f) {
            GetComponent<CanvasGroup>().alpha += Time.deltaTime / 0.25f;
            if (GetComponent<CanvasGroup>().alpha > 1.0f) {
                GetComponent<CanvasGroup>().alpha = 1.0f;
            }
            yield return null;
        }
    }

    public IEnumerator TransitionOut() {
        // placeholder
        while (GetComponent<CanvasGroup>().alpha > 0.0f) {
            GetComponent<CanvasGroup>().alpha -= Time.deltaTime / 0.25f;
            if (GetComponent<CanvasGroup>().alpha < 0.0f) {
                GetComponent<CanvasGroup>().alpha = 0.0f;
            }
            yield return null;
        }
    }

    public bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        if (command == InputManager.Command.Confirm && eventType == InputManager.Event.Down) {
            hurried = true;
        }
        return true;
    }
}
