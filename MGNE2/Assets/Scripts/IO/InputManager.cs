using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputManager : MonoBehaviour {

    public enum Command {
        Left,
        Right,
        Up,
        Down,
        Confirm,
        Cancel,
    };

    public enum Event {
        Down,
        Up,
        Hold,
        Repeat,
    };

    private static readonly float KeyRepeatSeconds = 0.6f;

    private Dictionary<Command, List<KeyCode>> keybinds;
    private List<InputListener> listeners;
    private Dictionary<Command, float> holdStartTimes;

    public void Awake() {
        keybinds = new Dictionary<Command, List<KeyCode>>();
        keybinds[Command.Left] = new List<KeyCode>(new[] { KeyCode.LeftArrow, KeyCode.A, KeyCode.Keypad4 });
        keybinds[Command.Right] = new List<KeyCode>(new[] { KeyCode.RightArrow, KeyCode.D, KeyCode.Keypad6 });
        keybinds[Command.Up] = new List<KeyCode>(new[] { KeyCode.UpArrow, KeyCode.D, KeyCode.Keypad8 });
        keybinds[Command.Down] = new List<KeyCode>(new[] { KeyCode.DownArrow, KeyCode.S, KeyCode.Keypad2 });
        keybinds[Command.Confirm] = new List<KeyCode>(new[] { KeyCode.Space, KeyCode.Z, KeyCode.Return });
        keybinds[Command.Cancel] = new List<KeyCode>(new[] { KeyCode.Escape, KeyCode.B, KeyCode.X });

        listeners = new List<InputListener>();
        holdStartTimes = new Dictionary<Command, float>();
    }

    public void Update() {
        List<InputListener> listeners = new List<InputListener>();
        listeners.AddRange(this.listeners);

        foreach (InputListener listener in listeners) {
            bool endProcessing = false; // ew.
            foreach (Command command in System.Enum.GetValues(typeof(Command))) {
                foreach (KeyCode code in keybinds[command]) {
                    if (Input.GetKeyDown(code)) {
                        endProcessing |= listener.OnCommand(command, Event.Down);
                    }
                    if (Input.GetKeyUp(code)) {
                        endProcessing |= listener.OnCommand(command, Event.Up);
                        holdStartTimes.Remove(command);
                    }
                    if (Input.GetKey(code)) {
                        if (!holdStartTimes.ContainsKey(command)) {
                            holdStartTimes[command] = Time.time;
                        }
                        endProcessing |= listener.OnCommand(command, Event.Hold);
                        if (Time.time - holdStartTimes[command] > KeyRepeatSeconds) {
                            endProcessing |= listener.OnCommand(command, Event.Repeat);
                        }
                    }
                    if (endProcessing) break;
                }
                if (endProcessing) break;
            }
            if (endProcessing) break;
        }
    }

    public void PushListener(InputListener listener) {
        listeners.Add(listener);
    }

    public void RemoveListener(InputListener listener) {
        listeners.Remove(listener);
    }
}
