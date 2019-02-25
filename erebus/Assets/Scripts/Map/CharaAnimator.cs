using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[ExecuteInEditMode]
[DisallowMultipleComponent]
public class CharaAnimator : MonoBehaviour {

    private const string DefaultMaterial2DPath = "Materials/Sprite2D";
    private const string DefaultMaterial3DPath = "Materials/Sprite3D";
    private const string AlwaysAnimatesProperty = "step";
    private const float DesaturationDuration = 0.5f;
    
    public float desaturation = 0.0f;
    public bool alwaysAnimates = false;
    public bool dynamicFacing = false;
    public string spriteName = "";

    private Vector2 lastPosition;
    private bool wasSteppingLastFrame;
    private List<KeyValuePair<float, Vector3>> afterimageHistory;
    private Vector3 preAnimLocalPosition;
    private OrthoDir preAnimFacing;
    private string preAnimSprite;

    public MapEvent parentEvent { get { return transform.parent.GetComponent<MapEvent>(); } }

    private Animator _animator;
    private Animator animator {
        get {
            if (_animator == null) _animator = GetComponent<Animator>();
            return _animator;
        }
    }

    public void Start() {
        if (!Application.isEditor) {
            lastPosition = gameObject.transform.position;

            if (Parent().GetComponent<CharaEvent>() != null) {
                Parent().GetComponent<Dispatch>().RegisterListener(MapEvent.EventEnabled, (object payload) => {
                    bool enabled = (bool)payload;
                    GetComponent<SpriteRenderer>().enabled = enabled;
                });
            }
        }
    }

    public void Update() {
        CopyShaderValues();

        if (Application.isPlaying) {
            if (Parent().GetComponent<CharaEvent>() != null) {
                bool steppingThisFrame = IsStepping();
                bool stepping = steppingThisFrame || wasSteppingLastFrame;
                wasSteppingLastFrame = steppingThisFrame;
                GetComponent<Animator>().SetBool("stepping", stepping);
                GetComponent<Animator>().SetInteger("dir", CalculateDirection().Ordinal());

                lastPosition = Parent().transform.position;
            } else {
                animator.SetBool("stepping", alwaysAnimates);
                animator.SetInteger("dir", OrthoDir.South.Ordinal());
            }
        } else {
            if (animator.runtimeAnimatorController != null) {
                SetSpriteByKey(animator.runtimeAnimatorController.name);
            }
        }
    }

    public void SetSpriteByKey(string spriteName) {
        this.spriteName = spriteName;
        string controllerPath = "Animations/Charas/Instances/" + spriteName;
        RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>(controllerPath);
        animator.runtimeAnimatorController = controller;

        string path = parentEvent.GetComponent<MapEvent3D>() == null ? DefaultMaterial2DPath : DefaultMaterial3DPath;
        GetComponent<SpriteRenderer>().material = Resources.Load<Material>(path);

        string spritePath = "Sprites/Charas/" + spriteName;
        Sprite[] sprites = Resources.LoadAll<Sprite>(spritePath);
        if (sprites == null) {
            Debug.LogError("Unknown sprite " + spriteName);
        } else {
            foreach (Sprite sprite in sprites) {
                if (sprite.name == spriteName + parentEvent.GetComponent<CharaEvent>().initialFacing.DirectionName() + "Center") {
                    GetComponent<SpriteRenderer>().sprite = sprite;
                    break;
                }
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
        ClearOverrideSprite();
    }

    public void SetOverrideSprite(Sprite sprite) {
        ClearOverrideSprite();
        animator.enabled = false;
        GetComponent<SpriteRenderer>().sprite = sprite;
    }

    public void SetOverrideAnim(List<Sprite> frames, float frameDuration) {
        this.animator.enabled = false;
        SimpleSpriteAnimator animator = GetComponent<SimpleSpriteAnimator>();
        if (animator == null) {
            animator = gameObject.AddComponent<SimpleSpriteAnimator>();
        }
        animator.frames = frames;
        animator.frameDuration = frameDuration;
        animator.enabled = true;
        animator.Update();
    }

    public void ClearOverrideSprite() {
        animator.enabled = true;
        if (GetComponent<SimpleSpriteAnimator>() != null) {
            GetComponent<SimpleSpriteAnimator>().enabled = false;
        }
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
        if (material != null) {
            material.SetFloat("_Desaturation", desaturation);
        }
    }

    private void UpdatePositionMemory() {
        lastPosition.x = gameObject.transform.position.x;
        lastPosition.y = gameObject.transform.position.y;
    }

    private OrthoDir CalculateDirection() {
        OrthoDir normalDir = Parent().GetComponent<CharaEvent>().facing;
        MapCamera cam = Application.isPlaying ? Global.Instance().Maps.camera : FindObjectOfType<MapCamera>();
        if (!cam || !dynamicFacing) {
            return normalDir;
        }

        Vector3 ourScreen = cam.GetCameraComponent().WorldToScreenPoint(parentEvent.transform.position);
        Vector3 targetWorld = ((MapEvent3D)parentEvent).TileToWorldCoords(parentEvent.position + normalDir.XY3D());
        targetWorld.y = parentEvent.transform.position.y;
        Vector3 targetScreen = cam.GetCameraComponent().WorldToScreenPoint(targetWorld);
        Vector3 delta = targetScreen - ourScreen;
        return OrthoDirExtensions.DirectionOf2D(new Vector2(delta.x, -delta.y));
    }

    private bool IsStepping() {
        Vector2 position = Parent().transform.position;
        Vector2 delta = position - lastPosition;
        return alwaysAnimates || delta.sqrMagnitude > 0 || Parent().GetComponent<MapEvent>().tracking;
    }
}
