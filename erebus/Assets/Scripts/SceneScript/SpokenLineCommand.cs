using UnityEngine;
using System.Collections;
using System;

public class SpokenLineCommand : TextCommand {

    private CharaData chara;

    // we expect text in the format MAX: "Some stuff!"
    // or else for narration lines, just Some stuff happened. is fine
    // breaking it down into character tag and stuff is done internally
    public SpokenLineCommand(string text) : base(text) {
        ScenePlayer player = Global.Instance().ScenePlayer;
        if (SceneScript.StartsWithName(text)) {
            string tag = text.Substring(0, text.IndexOf(':'));
            this.text = text.Substring(text.IndexOf(':') + 2);
            chara = Global.Instance().Database.Charas.GetDataOrNull(tag);
        }
    }

    public override IEnumerator PerformAction() {
        ScenePlayer player = Global.Instance().ScenePlayer;
        if (player.textbox.speaker != null) {
            if (player.textbox.gameObject.activeInHierarchy) {
                player.textbox.speaker.TransitionToChara(chara);
            } else {
                player.textbox.speaker.SetChara(chara);
            }
        }
        
        yield return player.StartCoroutine(base.PerformAction());

        if (chara != null) {
            Global.Instance().Memory.AppendLogItem(new LogItem(chara.tag.ToUpper() + ": " + text));
        } else {
            Global.Instance().Memory.AppendLogItem(new LogItem(text));
        }
    }

    protected override TextboxComponent PrimaryBox() {
        return Global.Instance().ScenePlayer.textbox;
    }

    protected override TextboxComponent SecondaryBox() {
        return Global.Instance().ScenePlayer.paragraphBox;
    }
}
