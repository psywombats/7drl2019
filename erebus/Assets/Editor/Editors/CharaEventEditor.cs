﻿using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(CharaEvent))]
public class CharaEventEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        CharaEvent chara = (CharaEvent)target;
        Texture2D tex = (Texture2D)EditorGUILayout.ObjectField("Appearance", chara.spritesheet, typeof(Texture2D), false);
        if (tex != chara.spritesheet) {
            chara.spritesheet = tex;
            chara.UpdateAppearance();
            EditorUtility.SetDirty(target);
        }

        OrthoDir facing = (OrthoDir)EditorGUILayout.EnumPopup("Facing", chara.facing);
        if (facing != chara.facing) {
            chara.facing = facing;
            chara.UpdateAppearance();
            EditorUtility.SetDirty(target);
        }

        if (Application.isPlaying) {
            if (GUILayout.Button("Walk test")) {
                chara.StartCoroutine(WalkTestRoutine(chara));
            }
        }
    }

    private IEnumerator WalkTestRoutine(CharaEvent chara) {
        while (true) {
            yield return CoUtils.RunSequence(new IEnumerator[] {
                    chara.parent.StepMultiRoutine(OrthoDir.North, 4),
                    chara.parent.StepMultiRoutine(OrthoDir.East, 4),
                    chara.parent.StepMultiRoutine(OrthoDir.South, 4),
                    chara.parent.StepMultiRoutine(OrthoDir.West, 4)
                });
        }
    }
}
