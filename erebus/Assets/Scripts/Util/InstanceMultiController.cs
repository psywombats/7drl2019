using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

[ExecuteInEditMode]
public class InstanceMultiController : MonoBehaviour {

    public bool AutoApply;

    public void Update() {
        if (AutoApply) {
            CopyAllComponentsInScene();
        }
    }

    public void CopyAllComponentsInScene() {
       foreach (InstanceMultiController controller in FindObjectsOfType<InstanceMultiController>()) {
            if (controller.gameObject.name.Equals(gameObject.name)) {
                CopyAllComponents(controller.gameObject);
            }
       }
    }

    private void CopyAllComponents(GameObject destination) {
        foreach (Component component in GetComponents<Component>()) {
            if (component != this && component.GetType() != typeof(Transform)) {
                CopyComponent(component, destination);
            }
        }
    }

    private void CopyComponent(Component original, GameObject destination) {
        System.Type type = original.GetType();
        Component destinationComponent = destination.GetComponent(type);
        FieldInfo[] fields = type.GetFields();
        foreach (FieldInfo field in fields) {
            if (!field.IsStatic) {
                field.SetValue(destinationComponent, field.GetValue(original));
            }
        }
        PropertyInfo[] properties = type.GetProperties();
        foreach (PropertyInfo property in properties) {
            if (property.CanWrite && property.CanWrite && property.Name != "name") {
                property.SetValue(destinationComponent, property.GetValue(original, null), null);
            }
        }
    }
}
