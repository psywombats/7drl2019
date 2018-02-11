using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TransitionIndexData", menuName = "Data/TransitionIndexData")]
public class TransitionIndexData : GenericIndex<TransitionData> {

}

public class TransitionData : GenericDataObject {

    public string fadeOut;
    public string fadeIn;

    public FadeData GetFadeOut() {
        return Global.Instance().ScenePlayer.fades.GetData(fadeOut);
    }

    public FadeData GetFadeIn() {
        return Global.Instance().ScenePlayer.fades.GetData(fadeIn);
    }
}