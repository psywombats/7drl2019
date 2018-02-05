using UnityEngine;
using System.Collections;
using System;

public abstract class TextCommand : SceneCommand {
    
    protected string text;

    public TextCommand(string text) {
        this.text = text;
    }

    public override IEnumerator PerformAction(ScenePlayer player) {
        TextboxComponent primaryBox = PrimaryBox(player);
        TextboxComponent secondaryBox = SecondaryBox(player);
        
        // fade the textboxes in or out
        if (!primaryBox.gameObject.activeInHierarchy) {
            yield return player.StartCoroutine(CoUtils.RunParallel(new[] {
                primaryBox.Activate(player),
                secondaryBox.Deactivate(player)
            }, player));
        }

        // type the text, this will take care of awaiting for input
        yield return player.StartCoroutine(primaryBox.ShowText(player, text, true));
    }

    protected abstract TextboxComponent PrimaryBox(ScenePlayer parser);

    protected abstract TextboxComponent SecondaryBox(ScenePlayer parser);
}
