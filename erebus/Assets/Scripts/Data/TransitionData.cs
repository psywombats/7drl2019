using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "TransitionData", menuName = "Data/TransitionData")]
public class TransitionData : GenericDataObject {

    public FadeData fadeOut;
    public FadeData fadeIn;

}
