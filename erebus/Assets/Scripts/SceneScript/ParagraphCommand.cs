using UnityEngine;
using System.Collections;
using System;

public class ParagraphCommand : TextCommand {

    public ParagraphCommand(string text) : base(text) {

    }

    protected override TextboxComponent PrimaryBox(ScenePlayer parser) {
        return parser.paragraphBox;
    }

    public override IEnumerator PerformAction(ScenePlayer player) {
        yield return player.StartCoroutine(base.PerformAction(player));
        Global.Instance().memory.AppendLogItem(new LogItem("\n" + text + "\n"));
    }

    protected override TextboxComponent SecondaryBox(ScenePlayer parser) {
        return parser.textbox;
    }
}
