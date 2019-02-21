using UnityEngine;
using System.Collections;

/// <summary>
/// A wrapper class to get results out of coroutines
/// </summary>
/// <typeparam name="T">The expted result type from a coroutine</typeparam>
public class Result<T> {

    public bool finished { get; private set; }
    public bool canceled { get; private set; }

    private T _value;
    public T value {
        get {
            Debug.Assert(!canceled, "Accessing canceled result");
            Debug.Assert(finished, "Accessing unset result");
            return _value;
        }
        set {
            Debug.Assert(!finished, "Can only set result once");
            _value = value;
            finished = true;
        }
    }

    public void Cancel() {
        Debug.Assert(!finished, "Can only set result once");
        finished = true;
        canceled = true;
    }

    public void Reset() {
        finished = false;
        canceled = false;
        _value = default(T);
    }
}
