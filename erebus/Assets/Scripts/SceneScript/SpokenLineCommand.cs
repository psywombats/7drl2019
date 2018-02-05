using UnityEngine;
using System.Collections;
using System;

public class SpokenLineCommand : TextCommand {

    private CharaData chara;

    // we expect text in the format MAX: "Some stuff!"
    // or else for narration lines, just Some stuff happened. is fine
    // breaking it down into character tag and stuff is done internally
    public SpokenLineCommand(ScenePlayer player, string text) : base(text) {
        if (SceneScript.StartsWithName(text)) {
            string tag = text.Substring(0, text.IndexOf(':'));
            this.text = text.Substring(text.IndexOf(':') + 2);
            chara = player.portraits.charas.GetDataOrNull(tag);
        }
    }

    public override IEnumerator PerformAction(ScenePlayer player) {
        if (player.textbox.speaker != null) {
            if (player.textbox.gameObject.activeInHierarchy) {
                player.textbox.speaker.TransitionToChara(chara);
            } else {
                player.textbox.speaker.SetChara(chara);
            }
        }
        
        yield return player.StartCoroutine(base.PerformAction(player));

        if (chara != null) {
            Global.Instance().Memory.AppendLogItem(new LogItem(chara.tag.ToUpper() + ": " + text));
        } else {
            Global.Instance().Memory.AppendLogItem(new LogItem(text));
        }
    }

    protected override TextboxComponent PrimaryBox(ScenePlayer player) {
        return player.textbox;
    }

    protected override TextboxComponent SecondaryBox(ScenePlayer player) {
        return player.paragraphBox;
    }
}
