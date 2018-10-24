using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(BattleAnimationPlayer))]
public class BattleAnimationDrawer : Editor {

    public override void OnInspectorGUI() { 
        base.OnInspectorGUI();
        BattleAnimationPlayer player = (BattleAnimationPlayer)target;
        if (player.anim != null) {
            Editor.CreateEditor(player.anim).DrawDefaultInspector();
        }
    }
}