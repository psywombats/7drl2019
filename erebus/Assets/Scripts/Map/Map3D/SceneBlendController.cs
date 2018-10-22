using UnityEngine;
using System.Collections;

public class SceneBlendController : MonoBehaviour {

    public GameObject tacticsRender;
    public GameObject duelRender;
    public GameObject tacticsZone;
    public GameObject duelZone;

    public void Start() {
        // TODO: how do we keep track of this thing?
        Global.Instance().Maps.BlendController = this;
    }

    public IEnumerator BlendInDuelRoutine(float duration) {
        duelRender.GetComponent<FadeoutBehavior>().alpha = 1.0f;
        yield return tacticsRender.GetComponent<FadeoutBehavior>().FadeRoutine(0.0f, duration);
    }
}
