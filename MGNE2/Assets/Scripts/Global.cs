using UnityEngine;
using System.Collections;

public class Global : MonoBehaviour {

    private static Global instance;
    
    public InputManager Input { get; set; }
    public LuaInterpreter Lua { get; set; }
    public MapManager Maps { get; set; }

    public static Global Instance() {
        if (instance == null) {
            GameObject globalObject = new GameObject();
            globalObject.hideFlags = HideFlags.HideAndDontSave;
            instance = globalObject.AddComponent<Global>();
            instance.InstantiateManagers();
        }
        return instance;
    }

    public void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    private void InstantiateManagers() {
        Input = gameObject.AddComponent<InputManager>();
        Lua = gameObject.AddComponent<LuaInterpreter>();
        Maps = gameObject.AddComponent<MapManager>();
    }
}
