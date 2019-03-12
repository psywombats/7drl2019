using UnityEngine;
using System.Collections;
using System;
using MoonSharp.Interpreter;

public class LuaCutsceneContext : LuaContext {

    private static readonly string DefinesPath = "Assets/Resources/Lua/cutscene_defines.lua";

    public override IEnumerator RunRoutine(LuaScript script) {
        yield return base.RunRoutine(script);
        if (MapOverlayUI.Instance().textbox.isDisplaying) {
            yield return MapOverlayUI.Instance().textbox.DisableRoutine();
        }
    }

    public void Start() {
        //lua.Globals["pc"] = Global.Instance().Maps.pc.GetComponent<MapEvent>().luaObject;
    }

    public override void Awake() {
        base.Awake();
        lua.DoString("function teleportCoords(mapName, x, y)\n" +
"    cs_teleportCoords(mapName, x, y)\n" +
"    await()\n" +
"end\n" +
"function teleport(mapName, eventName)\n" +
"    cs_teleport(mapName, eventName)\n" +
"    await()\n" +
"end\n" +
"function fadeOutBGM(seconds)\n" +
"    cs_fadeOutBGM(seconds)\n" +
"    await()\n" +
"end\n" +
"function speak(speaker, line)\n" +
"    cs_speak(speaker, line)\n" +
"    await()\n" +
"end\n" +
"function speak2(speaker, faceNo, line)\n" +
"    cs_speak2(speaker, faceNo, line)\n" +
"    await()\n" +
"end\n" +
"function nextMap()\n" +
"    cs_nextMap()\n" +
"    await()\n" +
"end\n");
    }

    public override void RunRoutineFromLua(IEnumerator routine) {
        if (MapOverlayUI.Instance().textbox.isDisplaying) {
            routine = CoUtils.RunSequence(new IEnumerator[] {
                MapOverlayUI.Instance().textbox.DisableRoutine(),
                routine,
            });
        }

        base.RunRoutineFromLua(routine);
    }

    public void RunTextboxRoutineFromLua(IEnumerator routine) {
        base.RunRoutineFromLua(routine);
    }

    protected override void AssignGlobals() {
        base.AssignGlobals();
        lua.Globals["playBGM"] = (Action<DynValue>)PlayBGM;
        lua.Globals["cs_fadeOutBGM"] = (Action<DynValue>)FadeOutBGM;
        lua.Globals["cs_speak"] = (Action<DynValue, DynValue>)Speak;
        lua.Globals["cs_speak2"] = (Action<DynValue, DynValue, DynValue>)Speak2;
        lua.Globals["cs_nextMap"] = (Action)NextMap;
        lua.Globals["winGame"] = (Action)WinGame;
        lua.Globals["reallyWinGame"] = (Action)ReallyWinGame;
    }

    // === LUA CALLABLE ============================================================================

    private void PlayBGM(DynValue bgmKey) {
        Global.Instance().Audio.PlayBGM(bgmKey.String);
    }

    private void FadeOutBGM(DynValue seconds) {
        RunRoutineFromLua(Global.Instance().Audio.FadeOutRoutine((float)seconds.Number));
    }

    private void Walk(DynValue path) {
        RunRoutineFromLua(WalkRoutine(path));
    }

    private void Speak(DynValue speaker, DynValue text) {
        RunTextboxRoutineFromLua(MapOverlayUI.Instance().textbox.SpeakRoutine(speaker.String, text.String));
    }

    private void Speak2(DynValue speaker, DynValue face, DynValue text) {
        RunTextboxRoutineFromLua(MapOverlayUI.Instance().textbox.SpeakRoutine(speaker.String, text.String, (int)face.Number));
    }

    private void NextMap() {
        RunRoutineFromLua(Global.Instance().Maps.NextMapRoutine());
    }

    private void WinGame() {
        RunRoutineFromLua(WinGameRoutine());
    }

    private void ReallyWinGame() {
        RunRoutineFromLua(ReallyWinGameRoutine());
    }

    // === OUR ROUTINES ============================================================================

    private IEnumerator WalkRoutine(DynValue path) {
        PCEvent pc = Global.Instance().Maps.pc;
        foreach (DynValue dynDir in path.Tuple) {
            EightDir dir;
            if (dynDir.String.Equals("forward")) {
                dir = pc.GetComponent<CharaEvent>().facing;
            } else {
                dir = EightDirExtensions.Parse(dynDir.String);
            }
            yield return pc.GetComponent<MapEvent>().StepRoutine(
                pc.GetComponent<MapEvent>().location, 
                pc.GetComponent<MapEvent>().location + dir.XY());
        }
    }

    private IEnumerator WinGameRoutine() {
        // oh god
        Unit unitx = Resources.Load<Unit>("Database/Units/Pascir");
        BattleUnit unit = new BattleUnit(unitx, null);
        var ui = FindObjectOfType<RogueUI>();
        yield return ui.PrepareTalkRoutine(unit);
        ui.face2.Populate(unit);

        Global.Instance().Maps.pc.GetComponent<CharaEvent>().FaceToward(GetComponent<MapEvent>());
        
        ui.GetComponent<LuaContext>().SetGlobal("name", unitx.unitName);
        ui.rightDisplayEnabled = true;
    }

    private IEnumerator ReallyWinGameRoutine() {
        var ui = FindObjectOfType<RogueUI>();
        yield return ui.WinRoutine();
    }
}
