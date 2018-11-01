using UnityEngine;
using System.Collections;

/// <summary>
/// A wrapper class to get results out of coroutines
/// </summary>
/// <typeparam name="T">The expted result type from a coroutine</typeparam>
public class Result<T> {

    public bool finished { get; private set; }
    public bool canceled { get; private set; }

    private T value;
    public T Value {
        get {
            Debug.Assert(!canceled, "Accessing canceled result");
            Debug.Assert(finished, "Accessing unset result");
            return value;
        }
        set {
            Debug.Assert(!finished, "Can only set result once");
            this.value = value;
            this.finished = true;
        }
    }

    public void Cancel() {
        Debug.Assert(!finished, "Can only set result once");
        this.finished = true;
        this.canceled = true;
    }
}
