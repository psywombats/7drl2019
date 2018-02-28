using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using MoonSharp.Interpreter;
using System;
using System.IO;

public class LuaInterpreter : MonoBehaviour {

    private static readonly string DefinesPath = "Assets/Resources/Scenes/Defines.lua";

    public Script GlobalContext { get; private set; }

    private MoonSharp.Interpreter.Coroutine activeScript;
    private int blockingRoutines;
    
    public void Awake() {
        GlobalContext = new Script();
        UserData.RegisterAssembly();

        // immediate functions
        GlobalContext.Globals["debugLog"] = (Action<DynValue>)DebugLog;
        GlobalContext.Globals["getSwitch"] = (Func<DynValue, DynValue>)GetSwitch;
        GlobalContext.Globals["setSwitch"] = (Action<DynValue, DynValue>)SetSwitch;
        GlobalContext.Globals["eventNamed"] = (Func<DynValue, LuaMapEvent>)EventNamed;
        GlobalContext.Globals["playSFX"] = (Action<DynValue>)PlaySFX;
        GlobalContext.Globals["playBGM"] = (Action<DynValue>)PlayBGM;

        // routines
        GlobalContext.Globals["cs_teleportCoords"] = (Action<DynValue, DynValue, DynValue>)Teleport;
        GlobalContext.Globals["cs_teleport"] = (Action<DynValue, DynValue>)Teleport;
        GlobalContext.Globals["cs_wait"] = (Action<DynValue>)Wait;
        GlobalContext.Globals["cs_fadeOutBGM"] = (Action<DynValue>)FadeOutBGM;
        GlobalContext.Globals["cs_playScene"] = (Action<DynValue>)PlayScene;
        GlobalContext.Globals["cs_speakLine"] = (Action<DynValue>)Speak;

        // global defines lua-side
        LoadDefines(DefinesPath);
    }

    public void LoadDefines(string path) {
        StreamReader reader = new StreamReader(DefinesPath);
        GlobalContext.DoStream(reader.BaseStream);
        reader.Close();
    }

    public DynValue Marshal(object toMarshal) {
        return DynValue.FromObject(GlobalContext, toMarshal);
    }

    public void RegisterAvatar(AvatarEvent avatar) {
       SetGlobal("avatar", avatar.GetComponent<MapEvent>().LuaObject);
    }

    // generates a lua script from a lua file
    public LuaScript CreateScriptFromFile(string fileName) {
        TextAsset luaText = Resources.Load<TextAsset>("Scenes/" + fileName + ".lua");
        return CreateScript(luaText.text);
    }

    // generates a lua script object from the specified lua guts, can be run as a process
    public LuaScript CreateScript(string luaScript) {
        luaScript = "return function()\n" + luaScript;
        luaScript = luaScript + "\nend";
        return new LuaScript(GlobalContext.DoString(luaScript));
    }

    // meant to be evaluated synchronously
    public LuaCondition CreateCondition(string luaScript) {
        return new LuaCondition(GlobalContext.LoadString(luaScript));
    }

    // creates an empty table as the lua representation of some c# object
    public LuaMapEvent CreateEvent(MapEvent mapEvent) {
        return new LuaMapEvent(GlobalContext.DoString("return {}"), mapEvent);
    }

    // evaluates a lua function in the global context
    public DynValue Evaluate(DynValue function) {
        return GlobalContext.Call(function);
    }

    // make sure the luaobject has been registered via [MoonSharpUserData]
    public void SetGlobal(string key, object luaObject) {
        GlobalContext.Globals[key] = luaObject;
    }

    // executes asynchronously, for cutscenes
    public void RunScript(DynValue function, Action callback = null) {
        Global.Instance().Maps.Avatar.PauseInput();
        StartCoroutine(CoUtils.RunWithCallback(ScriptRoutine(function), this, () => {
            Global.Instance().Maps.Avatar.UnpauseInput();
            activeScript = null;
            if (callback != null) {
                callback();
            }
        }));
    }

    // hang on to a chunk of lua to run later
    public DynValue Load(string luaChunk) {
        return GlobalContext.LoadString(luaChunk);
    }

    // call a coroutine from lua code
    // any coroutines invoked by proxy objects need to run here
    public void RunRoutineFromLua(IEnumerator routine) {
        Assert.IsNotNull(activeScript);
        blockingRoutines += 1;
        StartCoroutine(CoUtils.RunWithCallback(routine, Global.Instance().Lua, () => {
            blockingRoutines -= 1;
            if (blockingRoutines == 0 && activeScript != null) {
                activeScript.Resume();
            }
        }));
    }

    private IEnumerator ScriptRoutine(DynValue function) {
        Assert.IsNull(activeScript);
        activeScript = GlobalContext.CreateCoroutine(function).Coroutine;
        activeScript.Resume();
        while (activeScript.State != CoroutineState.Dead) {
            yield return null;
        }
    }

    private static void DebugLog(DynValue message) {
        Debug.Log(message.CastToString());
    }

    private static DynValue GetSwitch(DynValue switchName) {
        bool value = Global.Instance().Memory.GetSwitch(switchName.String);
        return Global.Instance().Lua.Marshal(value);
    }

    private static void SetSwitch(DynValue switchName, DynValue value) {
        Global.Instance().Memory.SetSwitch(switchName.String, value.Boolean);
    }

    private static LuaMapEvent EventNamed(DynValue eventName) {
        MapEvent mapEvent = Global.Instance().Maps.ActiveMap.GetEventNamed(eventName.String);
        if (mapEvent == null) {
            return null;
        } else {
            return mapEvent.LuaObject;
        }
    }

    private static void PlaySFX(DynValue sfxKey) {
        Global.Instance().Audio.PlaySFX(sfxKey.String);
    }

    private static void PlayBGM(DynValue bgmKey) {
        Global.Instance().Audio.PlayBGM(bgmKey.String);
    }

    // Routines

    private static void RunStaticRoutineFromLua(IEnumerator routine) {
        Global.Instance().Lua.RunRoutineFromLua(routine);
    }

    private static void Teleport(DynValue mapName, DynValue x, DynValue y) {
        RunStaticRoutineFromLua(Global.Instance().Maps.TeleportRoutine(mapName.String, new IntVector2((int)x.Number, (int)y.Number)));
    }

    private static void Teleport(DynValue mapName, DynValue targetEventName) {
        RunStaticRoutineFromLua(Global.Instance().Maps.TeleportRoutine(mapName.String, targetEventName.String));
    }

    private static void Wait(DynValue seconds) {
        RunStaticRoutineFromLua(CoUtils.Wait((float)seconds.Number));
    }

    private static void FadeOutBGM(DynValue seconds) {
        RunStaticRoutineFromLua(Global.Instance().Audio.FadeOutRoutine((float)seconds.Number));
    }

    private static void PlayScene(DynValue sceneName) {
        RunStaticRoutineFromLua(Global.Instance().ScenePlayer.PlaySceneFromLua(sceneName.String));
    }

    // Scene interaction routines

    private static void RunSceneCommand(SceneCommand command) {
        // TODO: get the real title in here
        SceneScript script = new SceneScript(command, "anonymous command");
        Global.Instance().ScenePlayer.gameObject.SetActive(true);
        RunStaticRoutineFromLua(Global.Instance().ScenePlayer.PlayCommandFromLua(script));
    }

    private static void Speak(DynValue line) {
        // line is interpreted as either a system command or tagged dialog
        RunSceneCommand(new SpokenLineCommand(line.String));
    }
}
