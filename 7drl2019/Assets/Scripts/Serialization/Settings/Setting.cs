using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Setting<T> {

    private string tag;
    private UnityEvent onModify;
    private T settingValue;

    public string Tag {
        get { return tag; }
    }
    public T Value {
        get { return settingValue; }
        set { settingValue = value; onModify.Invoke(); }
    }
    public UnityEvent OnModify {
        get { return onModify; }
    }

    public Setting(string tag, T defaultValue) {
        this.onModify = new UnityEvent();
        this.tag = tag;
        this.Value = defaultValue;
    }
}
