using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[RequireComponent(typeof(MapEvent))]
public class DirectionCursor : MonoBehaviour, InputListener {

    private const string InstancePath = "Prefabs/Map3D/DirectionCursor";

    public OrthoDir currentDir;
    private CharaEvent actor;
    private Result<OrthoDir> awaitingSelect;

    public static DirectionCursor GetInstance() {
        GameObject prefab = Resources.Load<GameObject>(InstancePath);
        return Instantiate(prefab).GetComponent<DirectionCursor>();
    }

    public void OnEnable() {
        currentDir = OrthoDir.North;
        Global.Instance().Input.PushListener(this);
        TacticsCam.Instance().target = GetComponent<MapEvent>();
    }

    public void OnDisable() {
        Global.Instance().Input.RemoveListener(this);
        if (TacticsCam.Instance() != null && TacticsCam.Instance().target == GetComponent<MapEvent>()) {
            TacticsCam.Instance().target = null;
        }
    }

    public IEnumerator AwaitSelectionRoutine(Result<OrthoDir> result) {
        awaitingSelect = result;
        while (!awaitingSelect.finished) {
            yield return null;
        }
        awaitingSelect = null;
    }

    public bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        if (eventType == InputManager.Event.Down) {
            switch (command) {
                case InputManager.Command.Down:
                    AttemptSetDirection(OrthoDir.South);
                    break;
                case InputManager.Command.Left:
                    AttemptSetDirection(OrthoDir.West);
                    break;
                case InputManager.Command.Right:
                    AttemptSetDirection(OrthoDir.East);
                    break;
                case InputManager.Command.Up:
                    AttemptSetDirection(OrthoDir.North);
                    break;
                case InputManager.Command.Confirm:
                    awaitingSelect.Value = currentDir;
                    break;
                case InputManager.Command.Cancel:
                    awaitingSelect.Cancel();
                    break;
            }
        }
        return true;
    }

    private void AttemptSetDirection(OrthoDir dir) {
        SetDirection(dir);
    }

    private void SetDirection(OrthoDir dir) {
        currentDir = dir;
        actor.facing = dir;
        GetComponent<MapEvent>().Position = actor.GetComponent<MapEvent>().Position + dir.XY();
        GetComponent<MapEvent>().SetScreenPositionToMatchTilePosition();
        TacticsCam.Instance().ManualUpdate();
    }
}
