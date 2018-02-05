using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ClickInterceptorComponent : MonoBehaviour {

    public void Awake() {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            Global.Instance().Input.SimulateCommand(InputManager.Command.Click);
        });
    }
}
