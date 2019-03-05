using UnityEngine;
using System.Collections;
using System;

public class Cursor : MonoBehaviour, InputListener {

    private const string InstancePath = "Prefabs/Tactics/Cursor";
    private const float ScrollSnapTime = 0.2f;
    
    public GameObject reticules;

    public bool cameraFollows { get; set; }
    
    private float lastStepTime;
    private Result<Vector2Int> awaitingSelect;
    private Func<Vector2Int, IEnumerator> scanner;

    public static Cursor GetInstance() {
        GameObject prefab = Resources.Load<GameObject>(InstancePath);
        return Instantiate(prefab).GetComponent<Cursor>();
    }

    public void Enable() {
        if (gameObject.activeSelf) {
            return;
        }
        gameObject.SetActive(true);

        Global.Instance().Input.PushListener(this);
        if (cameraFollows) {
            Global.Instance().Maps.camera.target = GetComponent<MapEvent3D>();
            Global.Instance().Maps.camera.snapTime = ScrollSnapTime;
        }
    }

    public void Disable() {
        if (!gameObject.activeSelf) {
            return;
        }

        Global.Instance().Input.RemoveListener(this);
        if (Global.Instance().Maps.camera.target == GetComponent<MapEvent3D>()) {
            // 7drl hack alert
            Global.Instance().Maps.camera.target = 
                FindObjectOfType<BattleController>().pcEvent.GetComponent<MapEvent3D>();
        }

        gameObject.SetActive(false);
    }

    // waits for the cursor to select
    public IEnumerator AwaitSelectionRoutine(Result<Vector2Int> result, Func<Vector2Int, IEnumerator> scanner = null) {
        this.scanner = scanner;
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
        if (GetComponent<MapEvent>().tracking || (awaitingSelect != null && awaitingSelect.finished)) {
            return true;
        }
        switch (eventType) {
            case InputManager.Event.Down:
                switch (command) {
                    case InputManager.Command.Up:
                    case InputManager.Command.Down:
                    case InputManager.Command.Right:
                    case InputManager.Command.Left:
                    case InputManager.Command.UpLeft:
                    case InputManager.Command.DownLeft:
                    case InputManager.Command.DownRight:
                    case InputManager.Command.UpRight:
                        TryStep(EightDirExtensions.FromCommand(command));
                        break;
                    case InputManager.Command.Confirm:
                        if (awaitingSelect != null) {
                            awaitingSelect.value = GetComponent<MapEvent>().location;
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

    private bool TryStep(EightDir dir) {
        //if (Time.fixedTime - lastStepTime < minTimeBetweenMoves) {
        //    return true;
        //}
        Vector2Int target = GetComponent<MapEvent>().location + dir.XY();
        if (GetComponent<MapEvent>().CanPassAt(target)) {
            StartCoroutine(GetComponent<MapEvent>().StepRoutine(dir));
            lastStepTime = Time.fixedTime;
        }
        if (scanner != null) {
            StartCoroutine(scanner(GetComponent<MapEvent>().location));
        }

        return true;
    }
}
