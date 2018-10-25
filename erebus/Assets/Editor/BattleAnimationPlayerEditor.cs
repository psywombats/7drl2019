using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(BattleAnimationPlayer))]
public class BattleAnimationDrawer : Editor {

    public override void OnInspectorGUI() { 
        base.OnInspectorGUI();
        BattleAnimationPlayer player = (BattleAnimationPlayer)target;
        EditorUtility.SetDirty(player);
        if (player.anim != null) {
            Editor.CreateEditor(player.anim).DrawDefaultInspector();
            if (Application.isPlaying) {
                if (!player.isPlayingAnimation) {
                    if (GUILayout.Button("Play animation")) {
                        player.StartCoroutine(player.PlayAnimationRoutine());
                    }
                } else {
                    GUILayout.Label("Running...");
                }
            }
        }
    }
}