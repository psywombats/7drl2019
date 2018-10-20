using UnityEngine;
using System.Collections;
using System;

public class AnonymousListener : InputListener {

    private Func<InputManager.Command, InputManager.Event, bool> eventResponder;

    public AnonymousListener(Func<InputManager.Command, InputManager.Event, bool> eventResponder) {
        this.eventResponder = eventResponder;
    }

    public bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        return eventResponder(command, eventType);
    }
}
