using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "TransitionIndexData", menuName = "Data/TransitionIndexData")]
public class TransitionIndexData : GenericIndex<TransitionData> {

}

[Serializable]
public class TransitionData : GenericDataObject {

    public string FadeOutTag;
    public string FadeInTag;

    public FadeData GetFadeOut() {
        return Global.Instance().Database.Fades.GetData(FadeOutTag);
    }

    public FadeData GetFadeIn() {
        return Global.Instance().Database.Fades.GetData(FadeInTag);
    }
}