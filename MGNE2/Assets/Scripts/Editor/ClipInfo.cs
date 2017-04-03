using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Editor window for listing all object reference curves in an animation clip
// straight up copied from the unity iste
public class ClipInfo : EditorWindow {
    private AnimationClip clip;

    [MenuItem("Window/Clip Info")]
    static void Init() {
        GetWindow(typeof(ClipInfo));
    }

    public void OnGUI() {
        clip = EditorGUILayout.ObjectField("Clip", clip, typeof(AnimationClip), false) as AnimationClip;

        EditorGUILayout.LabelField("Object reference curves:");
        if (clip != null) {
            foreach (EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(clip)) {
                ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                EditorGUILayout.LabelField(binding.path + "/" + binding.propertyName + ", Keys: " + keyframes.Length + " Type: " + binding.type);
                foreach(ObjectReferenceKeyframe keyframe in keyframes) {
                    EditorGUILayout.LabelField("Time: " + keyframe.time);
                    EditorGUILayout.LabelField("Value: " + keyframe.value);
                }
            }
        }
    }
}
