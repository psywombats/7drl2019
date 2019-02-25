using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharaEvent))]
public class CharaEventEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        CharaEvent chara = (CharaEvent)target;
        Texture2D tex = (Texture2D)EditorGUILayout.ObjectField("Appearance", chara.spritesheet, typeof(Texture2D), false);
        if (tex != chara.spritesheet) {
            chara.spritesheet = tex;
            chara.UpdateAppearance();
        }

        OrthoDir facing = (OrthoDir)EditorGUILayout.EnumPopup("Facing", chara.facing);
        if (facing != chara.facing) {
            chara.facing = facing;
            chara.UpdateAppearance();
        }
    }
}
