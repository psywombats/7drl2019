using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class SimpleSpriteAnimator : MonoBehaviour {

    public List<Sprite> frames;
    public float frameDuration = 0.5f;
    public bool autoplays = true;

    private float elapsed;

    public void Update() {
        elapsed += Time.deltaTime;
        UpdateSprite();
    }

    public void OnEnable() {
        UpdateSprite();
    }

    public void ResetFrame() {
        elapsed = 0.0f;
        UpdateSprite();
    }

    public void OnValidate() {
        if (frames != null && frames.Count > 0 && GetComponent<SpriteRenderer>().sprite == null) {
            GetComponent<SpriteRenderer>().sprite = frames[0];
        }
    }

    public IEnumerator PlayOnceRoutine() {
        foreach (Sprite sprite in frames) {
            GetComponent<SpriteRenderer>().sprite = sprite;
            yield return CoUtils.Wait(frameDuration);
        }
    }

    private void UpdateSprite() {
        if (frames != null && autoplays) {
            float frameFloat = elapsed * frames.Count / frameDuration;
            int frame = ((int)Mathf.Floor(frameFloat)) % frames.Count;
            GetComponent<SpriteRenderer>().sprite = frames[frame];
        }
    }
}
