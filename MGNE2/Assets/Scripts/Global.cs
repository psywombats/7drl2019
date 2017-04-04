using UnityEngine;
using System.Collections;

public class Global : MonoBehaviour {

    private static Global instance;
    
    public InputManager input;

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
        input = gameObject.AddComponent<InputManager>();
    }
}
