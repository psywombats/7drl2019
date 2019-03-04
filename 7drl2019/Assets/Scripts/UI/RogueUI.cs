﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RogueUI : MonoBehaviour, InputListener {

    public NumericalBar hpBar;
    public NumericalBar mpBar;
    public Image face;
    public Narrator narrator;
    public SkillsetUI skills;

    public BattleUnit unit { get; private set; }
    
    private Result<IEnumerator> executeResult;

    public void Populate() {
        if (unit == null) {
            unit = Global.Instance().Maps.pc.GetComponent<BattleEvent>().unit;
        }
        skills.Populate(unit);
        Populate(unit);
    }

    public bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        if (eventType != InputManager.Event.Up) {
            return true;
        }
        switch (command) {
            case InputManager.Command.Up:
            case InputManager.Command.Down:
            case InputManager.Command.Right:
            case InputManager.Command.Left:
            case InputManager.Command.UpLeft:
            case InputManager.Command.DownLeft:
            case InputManager.Command.DownRight:
            case InputManager.Command.UpRight:
                Global.Instance().Input.RemoveListener(this);
                EightDir dir = EightDirExtensions.FromCommand(command);
                IEnumerator result = unit.battler.StepOrAttackAction(dir, true);
                if (result != null) {
                    executeResult.value = result;
                }
                break;
            case InputManager.Command.Wait:
                Global.Instance().Input.RemoveListener(this);
                executeResult.value = CoUtils.Wait(0.0f);
                break;
        }
        return true;
    }

    public IEnumerator OnTurnAction() {
        return CoUtils.RunParallel(new IEnumerator[] {
            hpBar.AnimateWithTimeRoutine(unit.Get(StatTag.MHP), unit.Get(StatTag.HP), 0.125f),
            mpBar.AnimateWithTimeRoutine(unit.Get(StatTag.MMP), unit.Get(StatTag.MP), 0.125f),
            narrator.OnTurnAction(),
            skills.OnTurnAction(),
        }, this);
    }

    public IEnumerator PlayNextCommand(Result<IEnumerator> executeResult) {
        this.executeResult = executeResult;
        Global.Instance().Input.PushListener(this);
        while (!executeResult.finished) {
            yield return null;
        }
    }

    private void Populate(BattleUnit unit) {
        hpBar.Populate(unit.Get(StatTag.MHP), unit.Get(StatTag.HP));
        mpBar.Populate(unit.Get(StatTag.MP), unit.Get(StatTag.MP));
        face.sprite = unit.unit.face;
    }
}
