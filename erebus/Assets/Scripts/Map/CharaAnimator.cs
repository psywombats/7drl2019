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
    private const string AlwaysAnimatesProperty = "step";
    private const float DesaturationDuration = 0.5f;

    public MapEvent parentEvent = null;
    public float desaturation = 0.0f;
    public bool alwaysAnimates = false;
    public bool dynamicFacing = false;
    public string spriteName = "";

    private Vector2 lastPosition;

    private Vector3 preAnimLocalPosition;
    private OrthoDir preAnimFacing;
    private string preAnimSprite;

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

            bool stepping = alwaysAnimates || delta.sqrMagnitude > 0 || Parent().GetComponent<MapEvent>().tracking;
            GetComponent<Animator>().SetBool("stepping", stepping);
            GetComponent<Animator>().SetInteger("dir", CalculateDirection().Ordinal());

            lastPosition = position;
        } else {
            GetComponent<Animator>().SetBool("stepping", alwaysAnimates);
            GetComponent<Animator>().SetInteger("dir", OrthoDir.South.Ordinal());
        }

        CopyShaderValues();
    }

    public void OnValidate() {
        CopyShaderValues();
    }

    public void Populate(IDictionary<string, string> properties) {
        if (properties.ContainsKey(AlwaysAnimatesProperty)) {
            alwaysAnimates = true;
        }
    }

    public void SetSpriteByKey(string spriteName) {
        this.spriteName = spriteName;
        string controllerPath = "Animations/Charas/Instances/" + spriteName;
        RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>(controllerPath);
        GetComponent<Animator>().runtimeAnimatorController = controller;

        GetComponent<SpriteRenderer>().material = Resources.Load<Material>(DefaultMaterialPath);

        string spritePath = "Sprites/Charas/" + spriteName;
        Sprite[] sprites = Resources.LoadAll<Sprite>(spritePath);
        foreach (Sprite sprite in sprites) {
            if (sprite.name == spriteName + parentEvent.GetComponent<CharaEvent>().facing.DirectionName() + "Center") {
                GetComponent<SpriteRenderer>().sprite = sprite;
                break;
            }
        }
    }

    public void PrepareForAnimation() {
        preAnimLocalPosition = transform.localPosition;
        preAnimFacing = parentEvent.GetComponent<CharaEvent>().facing;
        preAnimSprite = spriteName;
    }

    public void ResetAfterAnimation() {
        transform.localPosition = preAnimLocalPosition;
        parentEvent.GetComponent<CharaEvent>().facing = preAnimFacing;
        SetSpriteByKey(preAnimSprite);
    }

    public IEnumerator DesaturateRoutine(float targetDesat) {
        float oldDesat = desaturation;
        float elapsed = 0.0f;
        while (desaturation != targetDesat) {
            elapsed += Time.deltaTime;
            desaturation = Mathf.Lerp(oldDesat, targetDesat, elapsed / DesaturationDuration);
            yield return null;
        }
    }

    private GameObject Parent() {
        return parentEvent == null ? transform.parent.gameObject : parentEvent.gameObject;
    }

    private void CopyShaderValues() {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Material material = Application.isPlaying ? sprite.material : sprite.sharedMaterial;
        material.SetFloat("_Desaturation", desaturation);
    }

    private void UpdatePositionMemory() {
        lastPosition.x = gameObject.transform.position.x;
        lastPosition.y = gameObject.transform.position.y;
    }

    private OrthoDir CalculateDirection() {
        OrthoDir normalDir = Parent().GetComponent<CharaEvent>().facing;
        MapCamera cam = Application.isPlaying ? Global.Instance().Maps.Camera : FindObjectOfType<MapCamera>();
        if (!cam || !dynamicFacing) {
            return normalDir;
        }

        Vector3 ourScreen = cam.GetCameraComponent().WorldToScreenPoint(parentEvent.transform.position);
        Vector3 targetWorld = MapEvent3D.TileToWorldCoords(parentEvent.Position + normalDir.XY());
        Vector3 targetScreen = cam.GetCameraComponent().WorldToScreenPoint(targetWorld);
        Vector3 delta = targetScreen - ourScreen;
        return OrthoDirExtensions.DirectionOf(new Vector2(delta.x, -delta.y));
    }
}
