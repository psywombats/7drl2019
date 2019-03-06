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

    public PCEvent pc { get; private set; }
    public BattleUnit unit {
        get {
            return pc.GetComponent<BattleEvent>().unit;
        }
    }

    private bool rightDisplayEnabled;
    private Result<IEnumerator> executeResult;

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
                IEnumerator result = unit.battler.StepOrAttackAction(dir, true);
                if (result != null) {
                    Global.Instance().Input.RemoveListener(this);
                    executeResult.value = result;
                }
                break;
            case InputManager.Command.Wait:
                Global.Instance().Input.RemoveListener(this);
                executeResult.value = CoUtils.Wait(0.0f);
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
                        StartCoroutine(PlaySkillRoutine(skill));
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
                StartCoroutine(ScanRoutine());
                rightDisplayEnabled = false;
                break;
        }
        return true;
    }

    public IEnumerator OnTurnAction() {
        return CoUtils.RunParallel(new IEnumerator[] {
            face1.OnTurnAction(),
            face2.OnTurnAction(),
            narrator.OnTurnAction(),
            skills.OnTurnAction(),
        }, this);
    }

    public IEnumerator PlayNextCommand(Result<IEnumerator> executeResult) {
        this.executeResult = executeResult;
        Global.Instance().Input.PushListener(this);
        while (!executeResult.finished) {
            yield return null;
        }
        Global.Instance().Input.RemoveListener(this);
    }

    private IEnumerator PlaySkillRoutine(Skill skill) {
        Global.Instance().Input.DisableListener(this);
        Result<IEnumerator> effectResult = new Result<IEnumerator>();
        yield return skill.PlaySkillRoutine(unit, effectResult);
        if (!effectResult.canceled) {
            executeResult.value = effectResult.value;
        }
        Global.Instance().Input.EnableListener(this);
    }

    private IEnumerator ScanRoutine() {
        Cursor cursor = unit.battle.SpawnCursor(unit.location, true);
        Result<Vector2Int> result = new Result<Vector2Int>();
        yield return cursor.AwaitSelectionRoutine(result, _ => true, ScanAtRoutine);
        if (!result.canceled) {
            MapEvent ev = unit.battle.map.GetEventAt<MapEvent>(result.value);
            if (ev.GetComponent<BattleEvent>()) {
                BattleUnit unit = ev.GetComponent<BattleEvent>().unit;
                yield return PrepareTalkRoutine(unit);
                LuaScript script = new LuaScript(GetComponent<LuaContext>(), unit.unit.luaOnExamine);
                GetComponent<LuaContext>().SetGlobal("name", unit.ToString());
                yield return script.RunRoutine();
            }
        }
        rightDisplayEnabled = false;
        unit.battle.DespawnCursor();
    }

    private IEnumerator ScanAtRoutine(Vector2Int loc) {
        MapEvent ev = unit.battle.map.GetEventAt<MapEvent>(loc);
        if (ev.GetComponent<BattleEvent>() && !ev.GetComponent<PCEvent>()) {
            rightDisplayEnabled = true;
            face2.Populate(ev.GetComponent<BattleEvent>().unit);
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
