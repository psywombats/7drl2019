using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharaAnimator : MonoBehaviour {

    public bool AlwaysAnimates = false;

    private Animator Appearance {
        get { return GetComponent<Animator>(); }
    }

    private Vector2 lastPosition;

    public void Start() {
        lastPosition = new Vector2();
        if (AlwaysAnimates) {
            StartAnimation();
        }
    }

    public void Update() {
        if (!AlwaysAnimates) {
            Vector3 position = gameObject.transform.position;
            if (Appearance.enabled) {
                if (position.x == lastPosition.x && position.y == lastPosition.y) {
                    StopAnimation();
                }
            } else {
                if (position.x != lastPosition.x || position.y != lastPosition.y) {
                    StartAnimation();
                }
            }
        }
    }

    private void StartAnimation() {
        Appearance.enabled = true;
    }

    private void StopAnimation() {
        // it strikes me that there should really be one animation controller per facing but welp
        Appearance.Play(Appearance.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0.0f);
        Appearance.enabled = false;
    }

    private void UpdatePositionMemory() {
        lastPosition.x = gameObject.transform.position.x;
        lastPosition.y = gameObject.transform.position.y;
    }
}
