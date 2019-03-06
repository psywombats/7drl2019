using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(MapEvent))]
public class DirectionCursor : MonoBehaviour, InputListener {

    private const string InstancePath = "Prefabs/Tactics/DirectionCursor";

    public bool cameraFollows { get; set; }

    public EightDir currentDir;
    private BattleEvent actor;
    private Result<EightDir> awaitingSelect;

    public static DirectionCursor GetInstance() {
        GameObject prefab = Resources.Load<GameObject>(InstancePath);
        return Instantiate(prefab).GetComponent<DirectionCursor>();
    }

    // selects an adjacent unit to the actor (provided they meet the rule), cancelable
    public IEnumerator SelectAdjacentUnitRoutine(Result<EightDir> result,
                BattleUnit actingUnit,
                Func<BattleUnit, bool> rule,
                bool canCancel = true) {
        List<EightDir> dirs = new List<EightDir>();
        Map map = actingUnit.battle.map;
        foreach (EightDir dir in Enum.GetValues(typeof(EightDir))) {
            Vector2Int loc = actingUnit.location + dir.XY();
            BattleEvent doll = map.GetEventAt<BattleEvent>(loc);
            if (doll != null && rule(doll.unit)) {
                dirs.Add(dir);
            }
        }

        yield return SelectTargetDirRoutine(result, actingUnit, dirs, canCancel);
    }

    public IEnumerator SelectTargetDirRoutine(Result<EightDir> result,
            BattleUnit actingUnit,
            Func<Vector2Int, bool> rule,
            bool canCancel = true) {
        List<EightDir> dirs = new List<EightDir>();
        Map map = actingUnit.battle.map;
        foreach (EightDir dir in Enum.GetValues(typeof(EightDir))) {
            Vector2Int loc = actingUnit.location + dir.XY();
            if (rule(loc)) {
                dirs.Add(dir);
            }
        }

        yield return SelectTargetDirRoutine(result, actingUnit, dirs, canCancel);
    }

        // selects a square to be targeted by the acting unit, might be canceled
        public IEnumerator SelectTargetDirRoutine(Result<EightDir> result,
            BattleUnit actingUnit,
            List<EightDir> allowedDirs,
            bool canCancel = true) {
        actor = actingUnit.battler;

        gameObject.SetActive(true);
        actingUnit.battle.cursor.DisableReticules();

        SelectionGrid grid = actingUnit.battle.SpawnSelectionGrid();
        TacticsTerrainMesh terrain = actingUnit.battle.map.terrain;
        grid.ConfigureNewGrid(actingUnit.location, 1, terrain, (Vector2Int loc) => {
            return (loc.x + loc.y + actingUnit.location.x + actingUnit.location.y) % 2 < 2;
        }, (Vector2Int loc) => {
            return allowedDirs.Contains(actingUnit.battler.GetComponent<MapEvent>().DirectionTo(loc));
        });
        if (allowedDirs.Count > 0) {
            AttemptSetDirection(allowedDirs[0]);
        } else {
            GetComponent<MapEvent>().SetLocation(actor.location);
        }

        while (!result.finished) {
            Result<EightDir> dirResult = new Result<EightDir>();
            yield return AwaitSelectionRoutine(actor, dirResult);
            if (dirResult.canceled) {
                if (canCancel) {
                    result.Cancel();
                    break;
                }
            } else {
                result.value = dirResult.value;
            }
        }

        Destroy(grid.gameObject);
        actingUnit.battle.cursor.EnableReticules();
        gameObject.SetActive(false);
    }

    public void Enable() {
        if (gameObject.activeSelf) {
            return;
        }
        gameObject.SetActive(true);
        currentDir = EightDir.N;
        Global.Instance().Input.PushListener(this);
        if (cameraFollows) {
            Global.Instance().Maps.camera.target = GetComponent<MapEvent3D>();
        }
    }

    public void Disable() {
        if (!gameObject.activeSelf) {
            return;
        }
        Global.Instance().Input.RemoveListener(this);
        if (Global.Instance().Maps.camera.target == GetComponent<MapEvent3D>()) {
            Global.Instance().Maps.camera.target = actor.GetComponent<MapEvent3D>();
        }
        gameObject.SetActive(false);
    }

    public IEnumerator AwaitSelectionRoutine(BattleEvent actor, Result<EightDir> result) {
        this.actor = actor;
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
                case InputManager.Command.Left:
                case InputManager.Command.Right:
                case InputManager.Command.Up:
                case InputManager.Command.DownLeft:
                case InputManager.Command.UpLeft:
                case InputManager.Command.DownRight:
                case InputManager.Command.UpRight:
                    AttemptSetDirection(EightDirExtensions.FromCommand(command));
                    break;
                case InputManager.Command.Confirm:
                    awaitingSelect.value = currentDir;
                    break;
                case InputManager.Command.Cancel:
                    awaitingSelect.Cancel();
                    break;
            }
        }
        return true;
    }

    private void AttemptSetDirection(EightDir dir) {
        SetDirection(dir);
    }

    private void SetDirection(EightDir dir) {
        currentDir = dir;
        actor.GetComponent<CharaEvent>().facing = dir;
        GetComponent<MapEvent>().SetLocation(actor.location + dir.XY());
    }
}
