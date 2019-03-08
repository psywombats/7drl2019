using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(LuaCutsceneContext))]
public class RogueUI : MonoBehaviour, InputListener {

    public Facebox face1, face2;
    public Narrator narrator;
    public SkillsetUI skills;
    public Textbox box;
    public GameObject rightDisplay;
    public SpellEditorUI spellEditor;

    public PCEvent pc { get; private set; }
    public BattleUnit unit {
        get {
            return pc.GetComponent<BattleEvent>().unit;
        }
    }

    public bool rightDisplayEnabled { get; set; }
    private Result<bool> executeResult;

    public void Populate() {
        if (pc == null) {
            pc = Global.Instance().Maps.pc;
        }
        skills.Populate(pc);
        face1.Populate(unit);
    }

    public void Update() {
        float off = 178.0f;
        float targetOffsetRight = rightDisplayEnabled ? 0.0f : off;
        float at = rightDisplay.GetComponent<RectTransform>().anchoredPosition.x;
        if (at != targetOffsetRight) {
            float to = at + Mathf.Sign(targetOffsetRight - at) * Time.deltaTime * 3.0f * off;
            to = Mathf.Clamp(to, 0.0f, off);
            rightDisplay.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                to,
                rightDisplay.GetComponent<RectTransform>().anchoredPosition.y);
        }
    }

    public bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        if (eventType != InputManager.Event.Up) {
            return true;
        }
        switch (command) {
            case InputManager.Command.Up:
            case InputManager.Command.Down:
            case InputManager.Command.Right:
            case InputManager.Command.Left:
            case InputManager.Command.UpLeft:
            case InputManager.Command.DownLeft:
            case InputManager.Command.DownRight:
            case InputManager.Command.UpRight:
                EightDir dir = EightDirExtensions.FromCommand(command);
                Global.Instance().Input.RemoveListener(this);
                StartCoroutine(unit.battler.StepOrAttackRoutine(dir, executeResult));
                break;
            case InputManager.Command.Wait:
                Global.Instance().Input.RemoveListener(this);
                executeResult.value = true;
                break;
            case InputManager.Command.Skill1:
            case InputManager.Command.Skill2:
            case InputManager.Command.Skill3:
            case InputManager.Command.Skill4:
            case InputManager.Command.Skill5:
            case InputManager.Command.Skill6:
                int skillNumber = Global.Instance().Input.CommandToNumber(command) - 1;
                if (skillNumber < pc.activeBook.spells.Count) {
                    Skill skill = pc.activeBook.spells[skillNumber];
                    if (unit.CanUse(skill)) {
                        Global.Instance().Input.RemoveListener(this);
                        StartCoroutine(skill.PlaySkillRoutine(unit, executeResult));
                    } else {
                        if (skill.costMP > 0) {
                            narrator.Log(skill + " costs too much MP to cast.", true);
                        } else {
                            narrator.Log(skill + " is on cooldown.", true);
                        }
                    }
                }
                break;
            case InputManager.Command.Examine:
            case InputManager.Command.Confirm:
                Global.Instance().Input.RemoveListener(this);
                StartCoroutine(ScanRoutine());
                rightDisplayEnabled = false;
                break;
            case InputManager.Command.Debug:
                StartCoroutine(spellEditor.ActivateRoutine(this, pc,
                    pc.GetComponent<MapEvent>().map.GetComponent<MapGenerator>().level));
                break;
        }
        return true;
    }

    public void OnTurn() {
        face1.OnTurn();
        face2.OnTurn();
        narrator.OnTurn();
        skills.OnTurn();
    }

    public IEnumerator PlayNextCommandRoutine(Result<bool> executeResult) {
        this.executeResult = executeResult;
        Global.Instance().Input.PushListener(this);
        while (!executeResult.finished) {
            yield return null;
        }
        Global.Instance().Input.RemoveListener(this);
    }

    private IEnumerator ScanRoutine() {
        narrator.Clear();
        Cursor cursor = unit.battle.SpawnCursor(unit.location, true);
        Result<Vector2Int> result = new Result<Vector2Int>();
        yield return cursor.AwaitSelectionRoutine(result, _ => true, ScanAtRoutine);
        if (!result.canceled) {
            BattleEvent ev = unit.battle.map.GetEventAt<BattleEvent>(result.value);
            if (ev != null) {
                BattleUnit unit = ev.unit;
                yield return PrepareTalkRoutine(unit);
                LuaScript script = new LuaScript(GetComponent<LuaContext>(), unit.unit.luaOnExamine);
                GetComponent<LuaContext>().SetGlobal("name", unit.ToString());
                yield return script.RunRoutine();
            }
        }
        rightDisplayEnabled = false;
        unit.battle.DespawnCursor();
        executeResult.Cancel();
    }

    private IEnumerator ScanAtRoutine(Vector2Int loc) {
        BattleEvent ev = unit.battle.map.GetEventAt<BattleEvent>(loc);
        if (ev != null && !ev.GetComponent<PCEvent>()) {
            rightDisplayEnabled = true;
            face2.Populate(ev.unit);
        } else {
            rightDisplayEnabled = false;
        }
        yield return null;
    }

    private IEnumerator PrepareTalkRoutine(BattleUnit other) {
        box.ConfigureSpeakers(unit, other);
        yield return null;
    }
}
