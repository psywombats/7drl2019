using System;
using UnityEngine;
using System.Collections;

public class UIEngine : MonoBehaviour, InputListener {

    public Canvas InteractiveCanvas;
    public UnityEngine.UI.Text DebugBox;
    public GameObject MenuAttachmentPoint;
    public FadingUIComponent Tint;

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
        InteractiveCanvas.enabled = true;
        yield return StartCoroutine(DisplayMenu(PauseMenuComponent.Spawn(InteractiveCanvas.gameObject, () => {
            InteractiveCanvas.enabled = false;
        })));
    }
}
