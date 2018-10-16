using UnityEngine;
using System.Collections;

/**
 * Renders the attached sprite as fixed-x at the camera.
 */
[RequireComponent(typeof(SpriteRenderer))]
[ExecuteInEditMode]
[DisallowMultipleComponent]
public class BillboardingSpriteComponent : MonoBehaviour {

    public void Update() {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Vector3 angles = sprite.transform.eulerAngles;
        sprite.transform.eulerAngles = new Vector3(
                TacticsCam.Instance().transform.eulerAngles.x,
                angles.y,
                angles.z);
    }
}
