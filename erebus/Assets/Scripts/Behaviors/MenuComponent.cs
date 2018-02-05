using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public abstract class MenuComponent : MonoBehaviour, InputListener {

    private const float FadeoutSeconds = 0.4f;

    protected Action onFinish;

    public float Alpha {
        get { return gameObject.GetComponent<CanvasGroup>().alpha; }
        set { gameObject.GetComponent<CanvasGroup>().alpha = value; }
    }

    public virtual void Start() {
        Global.Instance().Input.PushListener(this);
        SetInputEnabled(false);
    }

    protected static GameObject Spawn(GameObject parent, string prefabName, Action onFinish) {
        GameObject menuObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>(prefabName));
        menuObject.GetComponent<MenuComponent>().onFinish = onFinish;
        UIUtils.AttachAndCenter(parent, menuObject);
        return menuObject;
    }

    public virtual bool OnCommand(InputManager.Command command, InputManager.Event eventType) {
        if (eventType != InputManager.Event.Up) {
            return true;
        }
        switch (command) {
            case InputManager.Command.Menu:
            case InputManager.Command.Rightclick:
                StartCoroutine(ResumeRoutine());
                return true;
            default:
                return false;
        }
    }

    public IEnumerator FadeInRoutine() {
        while (Alpha < 1.0f) {
            Alpha += Time.deltaTime / FadeoutSeconds;
            yield return null;
        }
        Alpha = 1.0f;
        SetInputEnabled(true);
    }

    public IEnumerator FadeOutRoutine() {
        SetInputEnabled(false);
        while (Alpha > 0.0f) {
            Alpha -= Time.deltaTime / FadeoutSeconds;
            yield return null;
        }
        Alpha = 0.0f;
    }

    protected virtual float GetFadeoutSeconds() {
        return FadeoutSeconds;
    }

    protected virtual void SetInputEnabled(bool enabled) {
        if (enabled) {
            Global.Instance().Input.EnableListener(this);
        } else {
            Global.Instance().Input.DisableListener(this);
        }
        
    }

    protected IEnumerator ResumeRoutine() {
        yield return StartCoroutine(FadeOutRoutine());
        Global.Instance().Input.RemoveListener(this);
        if (onFinish != null) {
            onFinish();
        }
        Destroy(gameObject);
    }
}
