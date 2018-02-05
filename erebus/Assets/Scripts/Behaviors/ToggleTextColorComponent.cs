using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Text))]
public class ToggleTextColorComponent : MonoBehaviour {

    public Toggle toggle;
    public Color onColor = new Color(0.88f, 0.88f, 0.88f, 1.0f);
    public Color offColor = new Color(0.48f, 0.48f, 0.48f, 1.0f);

    public void Update() {
        GetComponent<Text>().color = toggle.interactable ? onColor : offColor;
    }
}
