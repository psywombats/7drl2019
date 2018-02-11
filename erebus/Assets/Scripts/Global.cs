using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Global : MonoBehaviour {

    private static readonly string VNModulePath = "Prefabs/VNModule";

    private static Global instance;
    private bool destructing;
    
    public InputManager Input { get; private set; }
    public LuaInterpreter Lua { get; private set; }
    public MapManager Maps { get; private set; }
    public MemoryManager Memory { get; private set; }
    public AudioManager Audio { get; private set; }
    public SettingsCollection Settings { get; private set; }

    private GlobalConfig config;
    public GlobalConfig Config {
        get {
            if (config == null) {
                config = GlobalConfig.GetInstance();
            }
            return config;
        }
    }

    private ScenePlayer scenePlayer;
    public ScenePlayer ScenePlayer {
        get {
            if (destructing) {
                return null;
            }
            if (scenePlayer != null) {
                if (scenePlayer.gameObject.scene != SceneManager.GetActiveScene()) {
                    scenePlayer = null;
                }
            }
            if (scenePlayer == null) {
                scenePlayer = FindObjectOfType<ScenePlayer>();
            }
            if (scenePlayer == null) {
                scenePlayer = Instantiate(Resources.Load<GameObject>(VNModulePath)).GetComponentInChildren<ScenePlayer>();
            }
            return scenePlayer;
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

        // this should be the only game/engine binding
        GGlobal.Instance();

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
    }

    private void SetFullscreenMode() {
        // not sure if this "check" is necessary
        // actually performing this here is kind of a hack
        if (Settings != null && Screen.fullScreen != Settings.GetBoolSetting(SettingsConstants.Fullscreen).Value) {
            Screen.fullScreen = Settings.GetBoolSetting(SettingsConstants.Fullscreen).Value;
        }
    }
}
