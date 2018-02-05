using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChoiceButtonComponent : MonoBehaviour {

    public Text text;

    public float Alpha {
        get { return GetComponent<CanvasGroup>().alpha; }
        set { GetComponent<CanvasGroup>().alpha = value; }
    }

}
