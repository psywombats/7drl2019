using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class LogComponent : MenuComponent {

    private const string PrefabName = "Prefabs/UI/Log";

    public Text textbox;
    public ScrollRect scroll;

    public override void Start() {
        base.Start();
        PopulateLog(Global.Instance().Memory.GetMessageHistory());
    }

    public static GameObject Spawn(GameObject parent, Action onFinish) {
        return Spawn(parent, PrefabName, onFinish);
    }

    public void PopulateLog(List<LogItem> log) {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        foreach (LogItem logItem in log) {
            builder.AppendLine(logItem.FormatLogLine());
        }
        textbox.text = builder.ToString();
        scroll.normalizedPosition = new Vector2(0, 0);
    }
}
