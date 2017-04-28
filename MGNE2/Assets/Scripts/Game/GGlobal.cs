using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GGlobal : MonoBehaviour {

    private static GGlobal instance;

    // RPG
    public HeroParty Party { get; set; }

    // Globals
    public GameLuaInterpretor Lua { get; private set; }

    public static GGlobal Instance() {
        if (instance == null) {
            GameObject globalObject = new GameObject();
            globalObject.hideFlags = HideFlags.HideAndDontSave;
            instance = globalObject.AddComponent<GGlobal>();
            instance.InstantiateManagers();
            instance.InitializeRPG();
        }
        return instance;
    }

    public void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    private void InstantiateManagers() {
        Lua = gameObject.AddComponent<GameLuaInterpretor>();
    }

    private void InitializeRPG() {
        Party = new HeroParty();
    }
}
