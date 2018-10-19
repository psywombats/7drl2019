using System;
using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[DisallowMultipleComponent]
public class CharaAnimator : MonoBehaviour {

    private const string DefaultMaterialPath = "Materials/SpriteDefault";

    public MapEvent ParentEvent;
    public bool AlwaysAnimates = false;
    public bool DynamicFacing = false;

    private Vector2 lastPosition;

    public void Start() {
        lastPosition = gameObject.transform.position;

        if (Parent().GetComponent<CharaEvent>() != null) {
            Parent().GetComponent<Dispatch>().RegisterListener(MapEvent.EventEnabled, (object payload) => {
                bool enabled = (bool)payload;
                GetComponent<SpriteRenderer>().enabled = enabled;
            });
        }
    }

    public void Update() {
        if (Parent().GetComponent<CharaEvent>() != null) {
            Vector2 position = Parent().transform.position;
            Vector2 delta = position - lastPosition;

            bool stepping = AlwaysAnimates || delta.sqrMagnitude > 0 || Parent().GetComponent<MapEvent>().tracking;
            GetComponent<Animator>().SetBool("stepping", stepping);
            GetComponent<Animator>().SetInteger("dir", CalculateDirection().Ordinal());

            lastPosition = position;
        } else {
            GetComponent<Animator>().SetBool("stepping", AlwaysAnimates);
            GetComponent<Animator>().SetInteger("dir", OrthoDir.South.Ordinal());
        }
    }

    public void Populate(string spriteName) {
        string controllerPath = "Animations/Charas/Instances/" + spriteName;
        RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>(controllerPath);
        GetComponent<Animator>().runtimeAnimatorController = controller;

        GetComponent<SpriteRenderer>().material = Resources.Load<Material>(DefaultMaterialPath);

        string spritePath = "Sprites/Charas/" + spriteName;
        Sprite[] sprites = Resources.LoadAll<Sprite>(spritePath);
        foreach (Sprite sprite in sprites) {
            if (sprite.name == spriteName + ParentEvent.GetComponent<CharaEvent>().facing.DirectionName() + "Center") {
                GetComponent<SpriteRenderer>().sprite = sprite;
                break;
            }
        }
    }

    private GameObject Parent() {
        return ParentEvent == null ? transform.parent.gameObject : ParentEvent.gameObject;
    }

    private void UpdatePositionMemory() {
        lastPosition.x = gameObject.transform.position.x;
        lastPosition.y = gameObject.transform.position.y;
    }

    private OrthoDir CalculateDirection() {
        OrthoDir normalDir = Parent().GetComponent<CharaEvent>().facing;
        MapCamera cam = Application.isPlaying ? Global.Instance().Maps.Camera : FindObjectOfType<MapCamera>();
        if (!DynamicFacing && !cam.dynamicFacing) {
            return normalDir;
        }

        float rotation = cam.GetCameraComponent().transform.localEulerAngles.y;
        rotation += 45.0f;
        int rotationOrdinal = (int)Math.Floor(rotation / 90.0f);
        return (OrthoDir)(normalDir.Ordinal() + rotationOrdinal);
    }
}
