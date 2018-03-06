using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TacticsCam : MonoBehaviour {

    private static TacticsCam instance;
    public static TacticsCam Instance() {
        if (instance == null) {
            instance = FindObjectOfType<TacticsCam>();
        }
        return instance;
    }

}
