using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LuaCutsceneContext))]
public class RogueUI : MonoBehaviour, InputListener {

    public Facebox face1, face2;
    public Narrator narrator;
    public SkillsetUI skills;
    public Textbox box;
    public GameObject rightDisplay;
    public SpellEditorUI spellEditor;
    public FadeImageEffect fader;
    public Text postMortem;
    [TextArea(3, 6)] public string luaTutorial;

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
            case InputManager.Command.Zoom:
                MapCamera cam = FindObjectOfType<MapCamera>();
                if (cam.target == pc.GetComponent<MapEvent3D>()) {
                    cam.target = pc.battle.map.GetEventNamed("ZoomTarget").GetComponent<MapEvent3D>();
                    cam.targetDistance = 100.0f;
                } else {
                    cam.targetDistance = 25.0f;
                    cam.target = pc.GetComponent<MapEvent3D>();
                }
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

    public IEnumerator PostMortemRoutine(BattleUnit killer) {
        yield return PrepareTalkRoutine(unit);
        face2.Populate(killer);

        rightDisplayEnabled = true;
        narrator.Clear();
        yield return fader.FadeRoutine(fader.startFade, false);

        LuaScript script = new LuaScript(GetComponent<LuaContext>(), killer.unit.luaOnDefeat);
        GetComponent<LuaContext>().SetGlobal("name", unit.ToString());
        yield return script.RunRoutine();
        rightDisplayEnabled = false;
        postMortem.text = "Made it to floor " + pc.battle.map.GetComponent<MapGenerator>().level +
            " before a disastrous encounter with " + killer;
        postMortem.text += "\n\nGot " + pc.gold + " gold out of it though!";
        postMortem.text = postMortem.text.Replace("\\n", "\n");
        yield return CoUtils.RunTween(GetComponent<CanvasGroup>().DOFade(0.0f, 3.0f));
        yield return CoUtils.RunTween(postMortem.GetComponent<CanvasGroup>().DOFade(1.0f, 1.0f));
        yield return CoUtils.Wait(3.0f);
        yield return CoUtils.RunTween(postMortem.GetComponent<CanvasGroup>().DOFade(0.0f, 3.0f));
        SceneManager.LoadScene("Title", LoadSceneMode.Single);
    }

    public IEnumerator EditSpellsRoutine() {
        yield return spellEditor.ActivateRoutine(this, pc, 
            pc.GetComponent<MapEvent>().map.GetComponent<MapGenerator>().level);
        Populate();
    }

    public IEnumerator PlayNextCommandRoutine(Result<bool> executeResult) {
        this.executeResult = executeResult;
        while (pc.GetComponent<MapEvent>().IsAnimating()) {
            yield return null;
        }
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

                pc.GetComponent<CharaEvent>().FaceToward(unit.battler.GetComponent<MapEvent>());
                unit.battler.GetComponent<CharaEvent>().FaceToward(pc.GetComponent<MapEvent>());

                LuaScript script = new LuaScript(GetComponent<LuaContext>(), unit.unit.luaOnExamine);
                GetComponent<LuaContext>().SetGlobal("name", unit.ToString());
                yield return script.RunRoutine();
            }
        }
        rightDisplayEnabled = false;
        unit.battle.DespawnCursor();
        executeResult.Cancel();
        narrator.Log(unit.StatusString(), true);
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

    public IEnumerator PrepareTalkRoutine(BattleUnit other) {
        box.ConfigureSpeakers(unit, other);
        yield return null;
    }

    public IEnumerator TutorialRoutine() {
        LuaScript script = new LuaScript(GetComponent<LuaContext>(), luaTutorial);
        yield return script.RunRoutine();
    }
}
