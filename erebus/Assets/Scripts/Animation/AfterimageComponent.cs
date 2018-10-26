using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class AfterimageComponent : MonoBehaviour {

    public int afterimageCount = 0;
    public float afterimageDuration = 0.05f;

    private List<GameObject> renderChildren;
    private List<Afterimage> images;
    private float lastLength;

    public void OnEnable() {
        images = new List<Afterimage>();
        renderChildren = new List<GameObject>();
        lastLength = afterimageCount * afterimageDuration;
    }

    public void OnDisable() {
        Clear();
    }

    public void Update() {
        if (lastLength != afterimageDuration * afterimageCount) {
            Clear();
            lastLength = afterimageDuration * afterimageCount;
        }

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        images.Add(new Afterimage(Time.time, transform.position, renderer.sprite));
        float lastRelevantTime = Time.time - afterimageCount * afterimageDuration;
        if (images.Count > 1 && images[1].time < lastRelevantTime) {
            images.RemoveAt(0);
        }
        
        for (int i = 0; i < afterimageCount; i += 1) {
            float time = Time.time - afterimageDuration * (i + 1);
            if (!HasTimeFor(time)) {
                break;
            }

            GameObject renderChild;
            if (renderChildren.Count <= i) {
                renderChild = new GameObject("Afterimage");
                renderChild.transform.parent = transform;
                renderChild.layer = gameObject.layer;
                renderChildren.Add(renderChild);
                SpriteRenderer childRenderer = renderChild.AddComponent<SpriteRenderer>();
                childRenderer.material = renderer.material;
                FadeoutBehavior fade = renderChild.AddComponent<FadeoutBehavior>();
                fade.alpha = 0.5f;
            } else {
                renderChild = renderChildren[i];
            }

            Afterimage[] greaterLesser = ImagesForTime(time);
            renderChild.transform.position = Vector3.Lerp(greaterLesser[0].pos, greaterLesser[1].pos,
                    (greaterLesser[0].time - time) / (greaterLesser[0].time - greaterLesser[1].time));
            renderChild.GetComponent<SpriteRenderer>().sprite = greaterLesser[0].sprite;
        }
    }

    private bool HasTimeFor(float time) {
        return images[0].time < time;
    }

    private Afterimage[] ImagesForTime(float time) {
        if (images.Count < 2) {
            return new Afterimage[] { images[0], images[0] };
        }

        Afterimage greater, lesser;
        int offset = 0;
        do {
            greater = images[images.Count - 1 - offset];
            lesser = images[images.Count - 2 - offset];
            offset += 1;
        } while (time < lesser.time);
        return new Afterimage[] { greater, lesser };
    }

    private void Clear() {
        foreach (GameObject renderer in renderChildren) {
            Destroy(renderer);
        }
        renderChildren.Clear();
        images.Clear();
    }

    private struct Afterimage {
        public float time;
        public Sprite sprite;
        public Vector3 pos;

        public Afterimage(float time, Vector3 pos, Sprite sprite) {
            this.time = time;
            this.pos = pos;
            this.sprite = sprite;
        }
    }
}
