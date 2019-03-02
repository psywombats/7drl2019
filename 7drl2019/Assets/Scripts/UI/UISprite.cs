using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(SpriteRenderer))]
public class UISprite : MonoBehaviour {
    
	public void Start() {
        HideSpriteRenderer();
        MatchImageToSprite();
    }

    public void OnValidate() {
        HideSpriteRenderer();
        MatchImageToSprite();
    }

    public void Update() {
        MatchImageToSprite();
    }

    private void HideSpriteRenderer() {
        GetComponent<SpriteRenderer>().color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    }

    private void MatchImageToSprite() {
        GetComponent<Image>().sprite = GetComponent<SpriteRenderer>().sprite;
    }
}
