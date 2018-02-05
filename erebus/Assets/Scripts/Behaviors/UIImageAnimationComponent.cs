using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class UIImageAnimationComponent : MonoBehaviour {

    public Sprite[] frames;
    public float secondsPerFrame = 1.0f;

    private int frameIndex;
    private float elapsed;

    public void Start() {
        Reset();
    }

    public void OnEnable() {
        Reset();
    }

    public void Update() {
        elapsed += Time.deltaTime;
        while (elapsed > secondsPerFrame) {
            elapsed -= secondsPerFrame;
            frameIndex += 1;
            if (frameIndex >= frames.Length) {
                frameIndex = 0;
            }

            Image image = GetComponent<Image>();
            image.sprite = frames[frameIndex];
        }
    }

    private void Reset() {
        frameIndex = 0;
        elapsed = 0.0f;
    }
}
