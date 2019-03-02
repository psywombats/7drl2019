using UnityEngine;
using System.Collections;

public class RogueUI : MonoBehaviour, InputListener {

    private Result<bool> executeResult;

    public IEnumerator PlayNextCommand(Result<bool> executeResult) {
        this.executeResult = executeResult;
        Global.Instance().Input.PushListener(this);
        while (!executeResult.finished) {
            yield return null;
        }
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
}
