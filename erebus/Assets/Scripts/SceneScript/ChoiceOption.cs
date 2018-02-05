using UnityEngine;
using System.Collections;

public class ChoiceOption {

    public string caption;
    public string sceneName;

    // choice string is in the format "caption :: scenename" eg "Click here :: s0001"
    public ChoiceOption(string choiceString) {
        this.caption = choiceString.Substring(0, choiceString.IndexOf(':') - 1);
        this.sceneName = choiceString.Substring(caption.Length + 4, choiceString.Length - (caption.Length + 4));
    }

}
