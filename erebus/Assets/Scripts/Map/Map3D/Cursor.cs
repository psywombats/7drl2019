using UnityEngine;
using System.Collections;
using System;

public class Cursor : MonoBehaviour, InputListener {

    public static readonly IntVector2 CanceledLocation = new IntVector2(-1, -1);

    private const string InstancePath = "Prefabs/Map3D/Cursor";

    public float minTimeBetweenMoves = 0.1f;
    
    private Action<IntVector2> onSelect;
    private float lastStepTime;
    private bool awaitingSelect;

    public static Cursor GetInstance() {
        GameObject prefab = Resources.Load<GameObject>(InstancePath);
        return UnityEngine.Object.Instantiate<GameObject>(prefab).GetComponent<Cursor>();
    }

    public void OnEnable() {
        Global.Instance().Input.PushListener(this);
        TacticsCam.Instance().target = GetComponent<MapEvent>();
    }

    public void OnDisable() {
        Global.Instance().Input.RemoveListener(this);
        if (TacticsCam.Instance() != null && TacticsCam.Instance().target == GetComponent<MapEvent>()) {
            TacticsCam.Instance().target = null;
        }
    }

    // configures the cursor behavior
    public void Configure(Action<IntVector2> onSelect) {
        this.onSelect = onSelect;
    }

    // waits for the cursor to select
    public IEnumerator AwaitSelectionRoutine() {
        awaitingSelect = true;
        while (awaitingSelect) {
            yield return null;
        }
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
            case InputManager.Event.Down:
                switch (command) {
                    case InputManager.Command.Confirm:
                        onSelect(GetComponent<MapEvent>().Position);
                        awaitingSelect = false;
                        return true;
                    case InputManager.Command.Cancel:
                        onSelect(CanceledLocation);
                        return true;
                    default:
                        return false;
                }
            default:
                return false;
        }
    }

    private bool TryStep(OrthoDir dir) {
        if (Time.fixedTime - lastStepTime < minTimeBetweenMoves) {
            return true;
        }
        IntVector2 target = GetComponent<MapEvent>().Position + dir.XY();
        if (GetComponent<MapEvent>().CanPassAt(target)) {
            StartCoroutine(GetComponent<MapEvent>().StepRoutine(dir));
            lastStepTime = Time.fixedTime;
        }

        return true;
    }
}
