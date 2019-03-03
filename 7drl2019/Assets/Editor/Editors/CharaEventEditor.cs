using UnityEngine;
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

        EightDir facing = (EightDir)EditorGUILayout.EnumPopup("Facing", chara.facing);
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
                    chara.parent.StepMultiRoutine(EightDir.NE, 2),
                    chara.parent.StepMultiRoutine(EightDir.SE, 2),
                    chara.parent.StepMultiRoutine(EightDir.SW, 2),
                    chara.parent.StepMultiRoutine(EightDir.NW, 2)
                });
        }
    }
}
