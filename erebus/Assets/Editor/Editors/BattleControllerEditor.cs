using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BattleController))]
public class BattleControllerEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        BattleController controller = (BattleController)target;
        if (GUILayout.Button("Start Battle")) {
            controller.StartCoroutine(controller.battle.BattleRoutine(controller));
        }

        GUILayout.Label("Battle status:");
        foreach (BattleFaction faction in controller.battle.GetFactions()) {
            GUILayout.Label("  " + faction);
        }
    }
}
