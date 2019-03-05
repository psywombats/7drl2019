using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class SkillContainer : MonoBehaviour {

    private readonly Color MPColor = new Color(0x7d, 0x65, 0xdd);
    private readonly Color CDColor = new Color(0x2B, 0x6D, 0x65);

    public Text label;
    public Image icon;
    public Text costLabel;

    private Skill skill;
    private BattleUnit unit;
    private bool usable;

    public void Populate(BattleUnit unit, int ordinal) {
        this.unit = unit;
        skill = unit.unit.knownSkills[ordinal];
        icon.sprite = skill.icon;
        label.text = "F" + (ordinal + 1);
        if (skill.costMP > 0) {
            costLabel.color = MPColor;
            costLabel.text = skill.costMP + " MP";
        } else {
            costLabel.color = CDColor;
            costLabel.text = skill.costCD + " CD";
        }
        

        usable = unit.CanUse(skill);
        GetComponent<CanvasGroup>().alpha = usable ? 1.0f : 0.5f;
    }

    public void UpdateUsability(bool instant = false) {
        bool canUse = unit.CanUse(skill);
        if (canUse != usable || instant) {
            usable = canUse;
            if (instant) {
                GetComponent<CanvasGroup>().alpha = 0.5f;
            } else {
                StartCoroutine(UpdateAction());
            }
        }
    }

    public IEnumerator UpdateAction() {
        bool canUse = unit.CanUse(skill);
        if (canUse != usable) {
            usable = canUse;
            Tweener tween = GetComponent<CanvasGroup>().DOFade(usable ? 1.0f : 0.5f, 0.125f);
            return CoUtils.RunTween(tween);
        } else {
            return CoUtils.Wait(0.0f);
        }
    }
}
