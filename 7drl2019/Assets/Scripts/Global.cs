using MoonSharp.Interpreter;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Global : MonoBehaviour {

    private static Global instance;
    private static bool destructing;
    
    public InputManager Input { get; private set; }
    public MapManager Maps { get; private set; }
    public MemoryManager Memory { get; private set; }
    public AudioManager Audio { get; private set; }
    public SettingsCollection Settings { get; private set; }

    private IndexDatabase database;
    public IndexDatabase Database {
        get {
            if (database == null && !destructing) {
                database = IndexDatabase.Instance();
            }
            return database;
        }
    }

    public static Global Instance() {
        if (instance == null) {
            Dictionary<string, string> scripts = new Dictionary<string, string>();

            object[] result = Resources.LoadAll("Lua", typeof(TextAsset));

            foreach (TextAsset ta in result.OfType<TextAsset>()) {
                scripts.Add(ta.name, ta.text);
            }

            Script.DefaultOptions.ScriptLoader = new MoonSharp.Interpreter.Loaders.UnityAssetsScriptLoader(scripts);


            GameObject globalObject = new GameObject("Globals");
            // debug-ish and we don't serialize scenes
            // globalObject.hideFlags = HideFlags.HideAndDontSave;
            instance = globalObject.AddComponent<Global>();
            instance.InstantiateManagers();
        }

        return instance;
    }

    public void Update() {
        SetFullscreenMode();
    }

    public void Awake() {
        DontDestroyOnLoad(gameObject);
        MoonSharp.Interpreter.UserData.RegisterAssembly();
    }

    public void OnDestroy() {
        destructing = true;
    }

    private void InstantiateManagers() {
        Settings = gameObject.AddComponent<SettingsCollection>();
        Input = gameObject.AddComponent<InputManager>();
        Maps = gameObject.AddComponent<MapManager>();
        Memory = gameObject.AddComponent<MemoryManager>();
        Audio = gameObject.AddComponent<AudioManager>();
    }

    private void SetFullscreenMode() {
        // not sure if this "check" is necessary
        // actually performing this here is kind of a hack
        if (Settings != null && Screen.fullScreen != Settings.GetBoolSetting(SettingsConstants.Fullscreen).Value) {
            Screen.fullScreen = Settings.GetBoolSetting(SettingsConstants.Fullscreen).Value;
        }
    }
}
