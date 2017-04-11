using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharaEvent))]
public class CharaAnimator : MonoBehaviour {

    public bool AlwaysAnimates = false;

    private Animator Appearance { get { return GetComponent<Animator>(); } }

    private Vector2 lastPosition;

    public void Start() {
        lastPosition = gameObject.transform.position;
    }

    public void Update() {
        Vector2 position = gameObject.transform.position;
        Vector2 delta = position - lastPosition;

        bool stepping = AlwaysAnimates || delta.sqrMagnitude > 0 || GetComponent<CharaEvent>().Tracking;
        Appearance.SetBool("stepping", stepping);
        Appearance.SetInteger("dir", GetComponent<CharaEvent>().Facing.Ordinal());

        lastPosition = position;
    }

    private void UpdatePositionMemory() {
        lastPosition.x = gameObject.transform.position.x;
        lastPosition.y = gameObject.transform.position.y;
    }
}
