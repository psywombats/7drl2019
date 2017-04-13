using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/**
 * Listener system for quick messaging within the same game object (or not).
 * Heavily inspired by iOS GSD, provide event name and callback, and then at later date
 * that callback can be triggered by passing the event name. Make sure to unsubscribe
 * when the object is destroyed.
 */
public class Dispatch : MonoBehaviour {

    private Dictionary<string, HashSet<Action<object>>> listeners;

    public void Awake() {
        listeners = new Dictionary<string, HashSet<Action<object>>>();
    }

    public void RegisterListener(string eventName, Action<object> callback) {
        if (!listeners.ContainsKey(eventName)) {
            listeners[eventName] = new HashSet<Action<object>>();
        }
        listeners[eventName].Add(callback);
    }

    public void UnregisterListener(string eventName, Action<object> callback) {
        HashSet<Action<object>> eventListeners = listeners[eventName];
        if (eventListeners != null) {
            eventListeners.Remove(callback);
        }
    }

    public void Signal(string eventName, object payload) {
        if (listeners == null) {
            // can happen in the editor, not an error case
            return;
        }
        if (listeners.ContainsKey(eventName)) {
            HashSet<Action<object>> eventListeners = listeners[eventName];
            foreach (Action<object> listener in eventListeners) {
                listener(payload);
            }
        }
    }
}
