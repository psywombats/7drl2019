using UnityEngine;
using System.Collections;
using System;

public class ParagraphCommand : TextCommand {

    public ParagraphCommand(string text) : base(text) {

    }

    public override IEnumerator PerformAction() {
        yield return Global.Instance().ScenePlayer.StartCoroutine(base.PerformAction());
        Global.Instance().Memory.AppendLogItem(new LogItem("\n" + text + "\n"));
    }

    protected override TextboxComponent PrimaryBox() {
        return Global.Instance().ScenePlayer.paragraphBox;
    }

    protected override TextboxComponent SecondaryBox() {
        return Global.Instance().ScenePlayer.textbox;
    }
}
