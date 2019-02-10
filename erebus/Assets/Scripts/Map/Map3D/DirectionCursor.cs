using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[RequireComponent(typeof(MapEvent))]
public class DirectionCursor : MonoBehaviour, InputListener {

    private const string InstancePath = "Prefabs/Map3D/DirectionCursor";

    public OrthoDir currentDir;
    private BattleEvent actor;
    private Result<OrthoDir> awaitingSelect;

    public static DirectionCursor GetInstance() {
        GameObject prefab = Resources.Load<GameObject>(InstancePath);
        return Instantiate(prefab).GetComponent<DirectionCursor>();
    }

    // selects an adjacent unit to the actor (provided they meet the rule), cancelable
    public IEnumerator SelectAdjacentUnitRoutine(Result<BattleUnit> result,
                BattleUnit actingUnit,
                Func<BattleUnit, bool> rule,
                bool canCancel = true) {
        List<OrthoDir> dirs = new List<OrthoDir>();
        Map map = actingUnit.battle.controller.map;
        foreach (OrthoDir dir in Enum.GetValues(typeof(OrthoDir))) {
            IntVector2 loc = actingUnit.location + dir.XY();
            BattleEvent doll = map.GetEventAt<BattleEvent>(map.LowestObjectLayer(), loc);
            if (doll != null && rule(doll.unit)) {
                dirs.Add(dir);
            }
        }
        if (dirs.Count > 0) {
            Result<OrthoDir> dirResult = new Result<OrthoDir>();
            yield return SelectTargetDirRoutine(dirResult, actingUnit, dirs, canCancel);
            IntVector2 loc = actingUnit.location + dirResult.value.XY();
            result.value = map.GetEventAt<BattleEvent>(map.LowestObjectLayer(), loc).unit;
        } else {
            Debug.Assert(false, "No valid directions");
            result.Cancel();
        }
    }

    // selects a square to be targeted by the acting unit, might be canceled
    public IEnumerator SelectTargetDirRoutine(Result<OrthoDir> result,
            BattleUnit actingUnit,
            List<OrthoDir> allowedDirs,
            bool canCancel = true) {

        gameObject.SetActive(true);
        currentDir = allowedDirs[0];
        actingUnit.controller.cursor.DisableReticules();

        SelectionGrid grid = actingUnit.controller.SpawnSelectionGrid();
        grid.ConfigureNewGrid(new IntVector2(3, 3), (IntVector2 loc) => {
            return (loc.x + loc.y) % 2 == 1;
        });
        grid.GetComponent<MapEvent>().position = actingUnit.location - new IntVector2(1, 1);
        grid.GetComponent<MapEvent>().SetScreenPositionToMatchTilePosition();
        GetComponent<MapEvent>().position = actingUnit.location;
        GetComponent<MapEvent>().SetScreenPositionToMatchTilePosition();

        while (!result.finished) {
            Result<OrthoDir> dirResult = new Result<OrthoDir>();
            yield return AwaitSelectionRoutine(actingUnit.doll, dirResult);
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
        actingUnit.controller.cursor.EnableReticules();
        gameObject.SetActive(false);
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

    public IEnumerator AwaitSelectionRoutine(BattleEvent actor, Result<OrthoDir> result) {
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
                    awaitingSelect.value = currentDir;
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
        actor.GetComponent<CharaEvent>().facing = dir;
        GetComponent<MapEvent>().position = actor.GetComponent<MapEvent>().position + dir.XY();
        GetComponent<MapEvent>().SetScreenPositionToMatchTilePosition();
        TacticsCam.Instance().ManualUpdate();
    }
}
