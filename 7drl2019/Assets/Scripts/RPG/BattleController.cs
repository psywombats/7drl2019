using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Map))]
public class BattleController : MonoBehaviour {

    // all of these behaviors read + managed internally
    public Map map { get { return GetComponent<Map>(); } }
    public Cursor cursor { get; private set; }
    public DirectionCursor dirCursor { get; private set; }
    public RogueUI ui { get; private set; }

    public BattleEvent heroEvent;
    public BattleUnit hero { get; private set; }

    // internal state
    private List<BattleUnit> units;
    private Dictionary<BattleUnit, BattleEvent> dolls;

    // === INITIALIZATION ==========================================================================

    public void Start() {
        hero = new BattleUnit(heroEvent.unitData, this, heroEvent.GetComponent<MapEvent>().location);
        heroEvent.unit = hero;
        units = new List<BattleUnit>() { heroEvent.unit };
        dolls = new Dictionary<BattleUnit, BattleEvent>();
        dolls[heroEvent.unit] = heroEvent;

        cursor = Cursor.GetInstance();
        cursor.gameObject.transform.SetParent(GetComponent<Map>().objectLayer.transform);
        cursor.gameObject.SetActive(false);

        dirCursor = DirectionCursor.GetInstance();
        dirCursor.gameObject.transform.SetParent(GetComponent<Map>().objectLayer.transform);
        dirCursor.gameObject.SetActive(false);

        ui = FindObjectOfType<RogueUI>();
    }

    // === GETTERS AND BOOKKEEPING =================================================================

    public BattleEvent GetEventForBattler(BattleUnit unit) {
        return dolls[unit];
    }

    public BattleUnit GetUnitAt(Vector2Int position) {
        foreach (MapEvent mapEvent in map.GetEventsAt(position)) {
            if (mapEvent.GetComponent<BattleEvent>() != null) {
                return mapEvent.GetComponent<BattleEvent>().unit;
            }
        }
        return null;
    }

    // === STATE MACHINE ===========================================================================

    public IEnumerator BattleRoutine() {
        while (true) {
            yield return PlayNextHumanActionRoutine();
        }
    }

    private IEnumerator PlayNextHumanActionRoutine() {
        Result<bool> executeResult = new Result<bool>();
        yield return ui.PlayNextCommand(executeResult);
        if (executeResult.value) {
            // TODO: 7drl: ai and turn processing
        }

        //Result<Effector> effectResult = new Result<Effector>();
        //yield return hero.PlayNextActionRoutine(effectResult);
        //if (effectResult.canceled) {
        //    yield return PlayNextHumanActionRoutine();
        //}
    }

    // === GAMEBOARD AND GRAPHICAL INTERACTION =====================================================

    public Cursor SpawnCursor(Vector2Int location) {
        cursor.Enable();
        cursor.GetComponent<MapEvent>().SetLocation(location);
        return cursor;
    }

    public void DespawnCursor() {
        cursor.Disable();
    }

    public void TargetCameraToLocation(Vector2Int loc) {
        Global.Instance().Maps.camera.SetTargetLocation(loc, map.terrain.HeightAt(loc));
    }

    public SelectionGrid SpawnSelectionGrid() {
        SelectionGrid grid = SelectionGrid.GetInstance();
        grid.gameObject.transform.SetParent(GetComponent<Map>().transform);
        return grid;
    }

    public void MoveCursorToHero() {
        cursor.GetComponent<MapEvent>().SetLocation(hero.location);
    }
}
