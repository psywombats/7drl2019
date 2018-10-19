using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Global : MonoBehaviour {

    private static readonly string UIModulePath = "Prefabs/UI/UIModule";

    private static Global instance;
    private bool destructing;
    
    public InputManager Input { get; private set; }
    public LuaInterpreter Lua { get; private set; }
    public MapManager Maps { get; private set; }
    public MemoryManager Memory { get; private set; }
    public AudioManager Audio { get; private set; }
    public SettingsCollection Settings { get; private set; }
    public ScenePlayer ScenePlayer { get; private set; }
    public UIEngine UIEngine { get; private set; }
    public PartyManager Party { get; private set; }

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
            GameObject globalObject = new GameObject();
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
    }

    public void OnDestroy() {
        destructing = true;
    }

    private void InstantiateManagers() {
        Settings = gameObject.AddComponent<SettingsCollection>();
        Input = gameObject.AddComponent<InputManager>();
        Lua = gameObject.AddComponent<LuaInterpreter>();
        Maps = gameObject.AddComponent<MapManager>();
        Memory = gameObject.AddComponent<MemoryManager>();
        Audio = gameObject.AddComponent<AudioManager>();
        Party = gameObject.AddComponent<PartyManager>();

        GameObject module = Instantiate(Resources.Load<GameObject>(UIModulePath));
        module.transform.parent = transform;
        UIEngine = module.GetComponentInChildren<UIEngine>();
    }

    private void SetFullscreenMode() {
        // not sure if this "check" is necessary
        // actually performing this here is kind of a hack
        if (Settings != null && Screen.fullScreen != Settings.GetBoolSetting(SettingsConstants.Fullscreen).Value) {
            Screen.fullScreen = Settings.GetBoolSetting(SettingsConstants.Fullscreen).Value;
        }
    }
}
