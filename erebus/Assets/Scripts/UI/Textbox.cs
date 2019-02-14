using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Textbox : MonoBehaviour {

    [Header("Config")]
    public float animationSeconds = 0.2f;

    [Space]
    [Header("Hookups")]
    public Text namebox;
    public Text text;
    public RectTransform backer;

    public IEnumerator EnableRoutine() {

    }

    public IEnumerator DisableRoutine() {

    }
}
