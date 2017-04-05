using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharaEvent))]
public class CharaAnimator : MonoBehaviour {

    public bool AlwaysAnimates = false;

    private Animator Appearance { get { return GetComponent<Animator>(); } }

    private Vector2 lastPosition;

    public void Start() {
        lastPosition = new Vector2();
        if (AlwaysAnimates) {
            StartAnimation();
        } else {
            StopAnimation();
        }

        GetComponent<Dispatch>().RegisterListener(CharaEvent.FaceEvent, (object payload) => {
            foreach (OrthoDir otherDir in System.Enum.GetValues(typeof(OrthoDir))) {
                Appearance.ResetTrigger(otherDir.TriggerName());
            }
            OrthoDir dir = (OrthoDir)payload;
            Appearance.SetTrigger(dir.TriggerName());
            Appearance.Play(Appearance.GetCurrentAnimatorStateInfo(0).fullPathHash, -1, 0.0f);
        });
    }

    public void Update() {
        Vector2 position = gameObject.transform.position;
        Vector2 delta = position - lastPosition;

        if (!AlwaysAnimates) {
            if (AnimationPlaying()) {
                if (delta.sqrMagnitude == 0) {
                    StopAnimation();
                }
            } else {
                if (delta.sqrMagnitude > 0) {
                    StartAnimation();
                }
            }
        }

        lastPosition = position;
    }

    private void StartAnimation() {
        Appearance.speed = 1.0f;
    }

    private void StopAnimation() {
        // it strikes me that there should really be one animation controller per facing but welp
        Appearance.Play(Appearance.GetCurrentAnimatorStateInfo(0).fullPathHash, -1, 0.3f);
        Appearance.speed = 0.0f;
    }

    private bool AnimationPlaying() {
        return Appearance.speed > 0.0f;
    }

    private void UpdatePositionMemory() {
        lastPosition.x = gameObject.transform.position.x;
        lastPosition.y = gameObject.transform.position.y;
    }
}
