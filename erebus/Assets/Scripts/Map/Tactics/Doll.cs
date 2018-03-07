using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Doll : MonoBehaviour {

    // Editor properties
    public SpriteRenderer Sprite;
    public bool BillboardedX;

    private Unit unit;
    public Unit Unit {
        get {
            return unit;
        }
        set {
            unit = value;
        }
    }

    public void Update() {
        if (BillboardedX) {
            Vector3 angles = Sprite.transform.eulerAngles;
            Sprite.transform.eulerAngles = new Vector3(TacticsCam.Instance().transform.eulerAngles.x, angles.y, angles.z);
        }
    }
}
