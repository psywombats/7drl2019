using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[RequireComponent(typeof(MapEvent))]
public class DirectionCursor : MonoBehaviour, InputListener {

    private const string InstancePath = "Prefabs/Map3D/DirectionCursor";

    private OrthoDir _dir;
    public OrthoDir dir {
        get {
            return _dir;
        }
        set {
            SetDirection(value);
        }
    }

    private CharaEvent actor;
    private Action<IntVector2> onSelect;
    private bool awaitingSelect;

    public static DirectionCursor GetInstance() {
        GameObject prefab = Resources.Load<GameObject>(InstancePath);
        return UnityEngine.Object.Instantiate<GameObject>(prefab).GetComponent<DirectionCursor>();
    }

    public void Configure(CharaEvent actor, Action<IntVector2> onSelect) {
        this.actor = actor;
        this.onSelect = onSelect;
        this.dir = OrthoDir.North;
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

    public IEnumerator AwaitSelectionRoutine() {
        awaitingSelect = true;
        while (awaitingSelect) {
            yield return null;
        }
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
                    awaitingSelect = false;
                    onSelect(GetComponent<MapEvent>().Position);
                    break;
                case InputManager.Command.Cancel:
                    awaitingSelect = false;
                    onSelect(Cursor.CanceledLocation);
                    break;
            }
        }
        return true;
    }

    private void AttemptSetDirection(OrthoDir dir) {
        SetDirection(dir);
    }

    private void SetDirection(OrthoDir dir) {
        _dir = dir;
        actor.facing = dir;
        GetComponent<MapEvent>().Position = actor.GetComponent<MapEvent>().Position + dir.XY();
        GetComponent<MapEvent>().SetScreenPositionToMatchTilePosition();
    }
}
