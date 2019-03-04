using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RogueUI : MonoBehaviour, InputListener {

    public NumericalBar hpBar;
    public NumericalBar mpBar;
    public Image face;
    
    private Result<bool> executeResult;

    public void Start() {
        Populate(Global.Instance().Maps.avatar.GetComponent<BattleEvent>().unitData);
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
                StartCoroutine(Global.Instance().Maps.avatar.TryStepRoutine(dir, executeResult));
                break;
            case InputManager.Command.Wait:
                Global.Instance().Input.RemoveListener(this);
                executeResult.value = true;
                break;
        }
        return true;
    }

    public IEnumerator PlayNextCommand(Result<bool> executeResult) {
        this.executeResult = executeResult;
        Global.Instance().Input.PushListener(this);
        while (!executeResult.finished) {
            yield return null;
        }
    }

    private void Populate(Unit unit) {
        hpBar.Populate(unit.stats.Get(StatTag.MHP), unit.stats.Get(StatTag.HP));
        mpBar.Populate(unit.stats.Get(StatTag.MP), unit.stats.Get(StatTag.MP));
        face.sprite = unit.face;
    }
}
