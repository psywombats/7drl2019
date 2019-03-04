using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnimationPlayer), true)]
public class AnimationPlayerDrawer : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        AnimationPlayer player = (AnimationPlayer)target;
        if (player.anim != null) {
            if (Application.IsPlaying(player)) {
                if (!player.isPlayingAnimation) {
                    if (GUILayout.Button("Play animation")) {
                        if (player.GetComponent<BattleEvent>()) {
                            player.GetComponent<CharaEvent>().doll.GetComponent<CharaAnimationTarget>()
                                .ConfigureToBattler(player.GetComponent<BattleEvent>());
                        }
                        player.StartCoroutine(CoUtils.RunWithCallback(player.PlayAnimationRoutine(), () => {
                            Repaint();
                        }));
                    }
                } else {
                    GUILayout.Label("Running...");
                    if (GUILayout.Button("(cancel)")) {
                        player.EditorReset();
                    }
                }
            }
        }
    }
}