using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DuelMap))]
public class BattleAnimationPlayer : MonoBehaviour {

    public DollTargetEvent attacker = null;
    public DollTargetEvent defender = null;
    public BattleAnimation anim = null;
}
