using UnityEngine;
using System.Collections;

public class LogItem {

    public CharaData chara;
    public string text;

    public LogItem(CharaData chara, string text) {
        this.chara = chara;
        this.text = text;
    }
    
    public LogItem(string text) : this(null, text) {

    }

    public string FormatLogLine() {
        if (chara == null) {
            return text;
        } else {
            return chara.displayName + ": " + text;
        }
    }
}
