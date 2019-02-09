using UnityEngine;
using System.Collections;
using System;

public class Cursor : MonoBehaviour, InputListener {

    private const string InstancePath = "Prefabs/Map3D/Cursor";
    private const float ScrollSnapTime = 0.2f;

    public float minTimeBetweenMoves = 0.1f;
    public GameObject reticules;
    
    private float lastStepTime;
    private Result<IntVector2> awaitingSelect;

    public static Cursor GetInstance() {
        GameObject prefab = Resources.Load<GameObject>(InstancePath);
        return Instantiate(prefab).GetComponent<Cursor>();
    }

    public void OnEnable() {
        Global.Instance().Input.PushListener(this);
        TacticsCam.Instance().target = GetComponent<MapEvent>();
        TacticsCam.Instance().snapTime = ScrollSnapTime;
    }

    public void OnDisable() {
        Global.Instance().Input.RemoveListener(this);
        if (TacticsCam.Instance() != null && TacticsCam.Instance().target == GetComponent<MapEvent>()) {
            TacticsCam.Instance().target = null;
        }
    }

    // waits for the cursor to select
    public IEnumerator AwaitSelectionRoutine(Result<IntVector2> result) {
        awaitingSelect = result;
        while (!result.finished) {
            yield return null;
        }
        awaitingSelect = null;
    }

    public void EnableReticules() {
        reticules.SetActive(true);
    }
    public void DisableReticules() {
        reticules.SetActive(false);
    }

    public bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        if (GetComponent<MapEvent>().tracking) {
            return true;
        }
        switch (eventType) {
            case InputManager.Event.Down:
                switch (command) {
                    case InputManager.Command.Up:
                        TryStep(OrthoDir.North);
                        break;
                    case InputManager.Command.Down:
                        TryStep(OrthoDir.South);
                        break;
                    case InputManager.Command.Right:
                        TryStep(OrthoDir.East);
                        break;
                    case InputManager.Command.Left:
                        TryStep(OrthoDir.West);
                        break;
                    case InputManager.Command.Confirm:
                        if (awaitingSelect != null) {
                            awaitingSelect.value = GetComponent<MapEvent>().Position;
                        }
                        break;
                    case InputManager.Command.Cancel:
                        if (awaitingSelect != null) {
                            awaitingSelect.Cancel();
                        }
                        break;
                }
                break;
        }
        return true;
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
