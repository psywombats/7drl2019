using System;
using UnityEngine;
using System.Collections;

public class UIEngine : MonoBehaviour, InputListener {

    public Canvas Canvas;
    public UnityEngine.UI.Text DebugBox;
    public ScenePlayer ScenePlayer;

    public void Start() {
        Global.Instance().Input.PushListener(this);
    }

    public bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        if (eventType != InputManager.Event.Down) {
            return false;
        }
        if (command == InputManager.Command.Menu) {
            StartCoroutine(PauseRoutine());
            return true;
        }
        return false;
    }

    public IEnumerator DisplayMenu(GameObject menuObject) {
        MenuComponent menuComponent = menuObject.GetComponent<MenuComponent>();
        menuComponent.Alpha = 0.0f;

        yield return menuComponent.FadeInRoutine();
    }

    private IEnumerator PauseRoutine() {
        Global.Instance().Memory.RememberScreenshot();
        yield return StartCoroutine(DisplayMenu(PauseMenuComponent.Spawn(Global.Instance().UIEngine.Canvas.gameObject, () => {} )));
    }
}
