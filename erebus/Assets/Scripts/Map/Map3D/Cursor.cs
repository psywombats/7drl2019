using UnityEngine;
using System.Collections;

public class Cursor : MonoBehaviour, InputListener {

    public void Start() {
        Global.Instance().Input.PushListener(this);
    }

    public bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        if (GetComponent<MapEvent>().tracking) {
            return true;
        }
        switch (eventType) {
            case InputManager.Event.Hold:
                switch (command) {
                    case InputManager.Command.Up:
                        TryStep(OrthoDir.North);
                        return true;
                    case InputManager.Command.Down:
                        TryStep(OrthoDir.South);
                        return true;
                    case InputManager.Command.Right:
                        TryStep(OrthoDir.East);
                        return true;
                    case InputManager.Command.Left:
                        TryStep(OrthoDir.West);
                        return true;
                    default:
                        return false;
                }
            default:
                return false;
        }
    }

    private bool TryStep(OrthoDir dir) {
        IntVector2 target = GetComponent<MapEvent>().Position + dir.XY();
        if (GetComponent<MapEvent>().CanPassAt(target)) {
            StartCoroutine(GetComponent<MapEvent>().StepRoutine(dir));
        }

        return true;
    }
}
