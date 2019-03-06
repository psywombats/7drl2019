using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using System.Text;

[RequireComponent(typeof(Text))]
public class Narrator : MonoBehaviour {

    private const float OlderMessageAlpha = 0.5f;

    private List<LogEntry> messages = new List<LogEntry>();
    private float oldMessageAlpha = 0.5f;

    public void Start() {
        Clear();
    }

    public void Log(string text, bool outOfTurn) {
        if (!char.IsUpper(text[0])) {
            text = char.ToUpper(text[0]) + text.Substring(1);
        }
        messages.Add(new LogEntry(text));
        if (outOfTurn) {
            OnTurn();
        }
    }

    public void Clear() {
        messages = new List<LogEntry>();
        GetComponent<Text>().text = "";
    }

    public void IncrementTurn() {
        foreach (LogEntry entry in messages) {
            entry.turnsOld += 1;
        }
    }

    public void OnTurn() {
        oldMessageAlpha = 1.0f;
        IncrementTurn();
        Tweener tween = DOTween.To(() => { return oldMessageAlpha; }, (float x) => {
            oldMessageAlpha = x;
            UpdateText();
        }, OlderMessageAlpha, 0.125f);

        while (messages.Count > 7) {
            if (messages[0].turnsOld <= 1) {
                break;
            }
            messages.RemoveAt(0);
        }

        StartCoroutine(CoUtils.RunTween(tween));
    }

    private void UpdateText() {
        StringBuilder builder = new StringBuilder(512);
        string oldAlpha = Mathf.RoundToInt(oldMessageAlpha * 255).ToString("x");
        string olderAlpha = Mathf.RoundToInt(OlderMessageAlpha * 255).ToString("x");
        string oldAlphaStart = "<color=#ffffff" + oldAlpha + ">";
        string olderAlphaStart = "<color=#ffffff" + olderAlpha + ">";
        string end = "</color>";
        foreach (LogEntry entry in messages) {
            if (entry != messages[0]) {
                builder.Append("\n");
            }
            if (entry.turnsOld > 2) {
                builder.Append(olderAlphaStart);
                builder.Append(entry.message);
                builder.Append(end);
            } else if (entry.turnsOld > 1) {
                builder.Append(oldAlphaStart);
                builder.Append(entry.message);
                builder.Append(end);
            } else {
                builder.Append(entry.message);
            }
        }
        GetComponent<Text>().text = builder.ToString();
    }

    private class LogEntry {
        public string message;
        public int turnsOld;
        public LogEntry(string message) {
            this.message = message;
            turnsOld = 0;
        }
    }
}
