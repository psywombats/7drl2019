﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DG.Tweening;

/**
 * For our purposes, a CharaEvent is anything that's going to be moving around the map
 * or has a physical appearance. For parallel process or whatevers, they won't have this.
 */
[RequireComponent(typeof(MapEvent))]
[DisallowMultipleComponent]
public class CharaEvent : MonoBehaviour {

    private const float Gravity = -20.0f;
    private const float JumpHeightUpMult = 2.0f;
    private const float JumpHeightDownMult = 1.6f;
    private const string DefaultMaterial2DPath = "Materials/Sprite2D";
    private const string DefaultMaterial3DPath = "Materials/Sprite3D";
    private const float DesaturationDuration = 0.5f;
    private const float StepsPerSecond = 4.0f;
    private const float JumpStepsPerSecond = 8.0f;

    [Space]
    public GameObject doll;
    public SpriteRenderer mainLayer;
    public SpriteRenderer armsLayer;
    public SpriteRenderer itemLayer;
    public float desaturation = 0.0f;
    public bool alwaysAnimates = false;

    private Dictionary<string, Sprite> sprites;
    private Vector2 lastPosition;
    private bool wasSteppingLastFrame;
    private List<KeyValuePair<float, Vector3>> afterimageHistory;
    private Vector3 targetPx;
    private float moveTime;
    private bool stepping;
    private bool visible = true;
    private bool footOffset;

    public MapEvent parent { get { return GetComponent<MapEvent>(); } }
    public Map map { get { return parent.map; } }
    public Sprite overrideBodySprite { get; set; }
    public Sprite itemSprite { get; set; }
    public ArmMode armMode { get; set; }
    public ItemMode itemMode { get; set; }
    public bool jumping { get; set; }

    [SerializeField]
    [HideInInspector]
    private Texture2D _spritesheet;
    public Texture2D spritesheet {
        get { return _spritesheet; }
        set {
            _spritesheet = value;
            LoadSpritesheetData();
            UpdateAppearance();
        }
    }

    [SerializeField]
    [HideInInspector]
    private EightDir _facing = EightDir.S;
    public EightDir facing {
        get { return _facing; }
        set {
            _facing = value;
            if (facing == EightDir.N) {
                armsLayer.sortingOrder = -1;
            } else {
                armsLayer.sortingOrder = 1;
            }
            UpdateAppearance();
        }
    }

    private SpriteRenderer[] renderers {
        get { return new SpriteRenderer[] { mainLayer, armsLayer, itemLayer }; }
    }

    public static string NameForFrame(string sheetName, int x, int y) {
        return sheetName + "_" + x + "_" + y;
    }

    public void Start() {
        CopyShaderValues();
        GetComponent<Dispatch>().RegisterListener(MapEvent.EventMove, (object payload) => {
            facing = (EightDir)payload;
        });
        GetComponent<Dispatch>().RegisterListener(MapEvent.EventEnabled, (object payload) => {
            bool enabled = (bool)payload;
            foreach (SpriteRenderer renderer in renderers) {
                renderer.enabled = enabled;
            }
        });
    }

    public void Update() {
        CopyShaderValues();
        
        bool steppingThisFrame = IsSteppingThisFrame();
        stepping = steppingThisFrame; // || wasSteppingLastFrame;
        if (steppingThisFrame != wasSteppingLastFrame) {
            moveTime = 0.0f;
            footOffset = !footOffset;
        }
        if (stepping) {
            moveTime += Time.deltaTime;
        }
        wasSteppingLastFrame = steppingThisFrame;
        lastPosition = transform.position;

        UpdateAppearance();
    }

    public void UpdateAppearance() {
        if (spritesheet != null) {
            if (sprites == null || sprites.Count == 0) {
                LoadSpritesheetData();
            }
            mainLayer.sprite = SpriteForMain();
            armsLayer.sprite = SpriteForArms();
            itemLayer.sprite = SpriteForItem();

            if (itemLayer.sprite != null) {
                itemLayer.transform.localPosition = new Vector3(
                    (float)armMode.ItemAnchor().x / Map.TileSizePx,
                    (float)armMode.ItemAnchor().y / Map.TileSizePx, 
                    itemLayer.transform.localPosition.z);
                itemLayer.transform.localEulerAngles = new Vector3(0.0f, 0.0f, itemMode.Rotation());
            }
        }
    }

    public void FaceToward(MapEvent other) {
        facing = parent.DirectionTo(other);
    }

    public Sprite FrameBySlot(int x) {
        return sprites[NameForFrame(spritesheet.name, x, DirectionRelativeToCamera().Ordinal())];
    }
    public Sprite FrameBySlot(int x, int y) {
        string name = NameForFrame(spritesheet.name, x, y);
        if (!sprites.ContainsKey(name)) {
            Debug.LogError(this + " doesn't contain frame " + name);
        }
        return sprites[name];
    }

    private void CopyShaderValues() {
        foreach (SpriteRenderer renderer in renderers) {
            Material material = Application.isPlaying ? renderer.material : renderer.sharedMaterial;
            if (material != null) {
                material.SetFloat("_Desaturation", desaturation);
            } 
        }
    }

    public void SetVisibleByPC(bool visible) {
        if (this.visible != visible) {
            StartCoroutine(GetComponent<CharaEvent>().FadeRoutine(visible));
            this.visible = visible;
        }
    }

    public IEnumerator FadeRoutine(bool visible) {
        Tweener tween = DOTween.To(() => mainLayer.color.a, x => {
            foreach (SpriteRenderer renderer in renderers) {
                renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, x);
            }
        }, visible ? 1.0f : 0.0f, 0.125f);
        yield return CoUtils.RunTween(tween);
    }

    public IEnumerator StepRoutine(EightDir dir, bool faceTo = true) {
        if (faceTo) {
            facing = dir;
        }
        parent.tracking = true;
        Vector2Int offset = dir.XY();
        Vector3 startPx = parent.positionPx;
        targetPx = parent.TileToWorldCoords(parent.location);
        if (targetPx.y == startPx.y || GetComponent<MapEvent3D>() == null) {
            yield return parent.LinearStepRoutine(dir);
        } else if (targetPx.y > startPx.y) {
            // jump up routine routine
            float duration = (targetPx - startPx).magnitude / parent.CalcTilesPerSecond() * JumpHeightUpMult;
            yield return JumpRoutine(startPx, targetPx, duration);
            overrideBodySprite = FrameBySlot(0, DirectionRelativeToCamera().Ordinal()); // "prone" frame
            yield return CoUtils.Wait(1.0f / parent.CalcTilesPerSecond() / 2.0f);
            overrideBodySprite = null;
        } else {
            // jump down routine
            float elapsed = 0.0f;
            float walkRatio = 0.65f;
            float walkDuration = walkRatio / parent.CalcTilesPerSecond();
            while (true) {
                float t = elapsed / walkDuration;
                elapsed += Time.deltaTime;
                parent.transform.position = new Vector3(
                    startPx.x + t * (targetPx.x - startPx.x) * walkRatio,
                    startPx.y,
                    startPx.z + t * (targetPx.z - startPx.z) * walkRatio);
                parent.SetCameraTrackerLocation(parent.transform.position);
                if (elapsed >= walkDuration) {
                    break;
                }
                yield return null;
            }
            float dy = targetPx.y - startPx.y;
            float jumpDuration = Mathf.Sqrt(dy / Gravity) * JumpHeightDownMult;
            bool isBigDrop = dy <= -1.0f;
            yield return JumpRoutine(parent.transform.position, targetPx, jumpDuration, isBigDrop);
            if (isBigDrop) {
                overrideBodySprite = FrameBySlot(2, DirectionRelativeToCamera().Ordinal()); // "prone" frame
                yield return CoUtils.Wait(JumpHeightDownMult / parent.CalcTilesPerSecond() / 2.0f);
                overrideBodySprite = null;
            }
        }
        parent.tracking = false;
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

    private IEnumerator JumpRoutine(Vector3 startPx, Vector3 targetPx, float duration, bool useJumpFrames = true) {
        jumping = useJumpFrames;
        float elapsed = 0.0f;
        
        float dy = (targetPx.y - startPx.y);
        float b = (dy - Gravity * (duration * duration)) / duration;
        while (true) {
            float t = elapsed / duration;
            elapsed += Time.deltaTime;
            parent.transform.position = new Vector3(
                startPx.x + t * (targetPx.x - startPx.x),
                startPx.y + Gravity * (elapsed * elapsed) + b * elapsed,
                startPx.z + t * (targetPx.z - startPx.z));
            parent.SetCameraTrackerLocation(startPx + (targetPx - startPx) * t);
            if (elapsed >= duration) {
                break;
            }
            yield return null;
        }
        jumping = false;
        parent.SetScreenPositionToMatchTilePosition();
    }

    private bool IsSteppingThisFrame() {
        Vector2 position = transform.position;
        Vector2 delta = position - lastPosition;
        return alwaysAnimates || (delta.sqrMagnitude > 0 && delta.sqrMagnitude < Map.TileSizePx) || parent.tracking;
    }

    private void LoadSpritesheetData() {
        string path = GetComponent<MapEvent3D>() == null ? DefaultMaterial2DPath : DefaultMaterial3DPath;
        foreach (SpriteRenderer renderer in renderers) {
            if (Application.isPlaying) {
                if (renderer.material == null) {
                    renderer.material = Resources.Load<Material>(path);
                }
            } else {
                if (renderer.sharedMaterial == null) {
                    renderer.sharedMaterial = Resources.Load<Material>(path);
                }
            }
        }

        sprites = new Dictionary<string, Sprite>();
        path = AssetDatabase.GetAssetPath(spritesheet);
        if (path.StartsWith("Assets/Resources/")) {
            path = path.Substring("Assets/Resources/".Length);
        }
        if (path.EndsWith(".png")) {
            path = path.Substring(0, path.Length - ".png".Length);
        }
        foreach (Sprite sprite in Resources.LoadAll<Sprite>(path)) {
            sprites[sprite.name] = sprite;
        }
    }

    private OrthoDir DirectionRelativeToCamera() {
        MapCamera cam = Application.isPlaying ? Global.Instance().Maps.camera : FindObjectOfType<MapCamera>();

        Vector3 ourScreen = cam.cam.WorldToScreenPoint(transform.position);
        Vector3 targetWorld = ((MapEvent3D)parent).TileToWorldCoords(parent.location + facing.XY());
        targetWorld.y = parent.transform.position.y;
        Vector3 targetScreen = cam.cam.WorldToScreenPoint(targetWorld);
        Vector3 delta = targetScreen - ourScreen;
        return OrthoDirExtensions.DirectionOf2D(new Vector2(delta.x, -delta.y));
    }

    private Sprite SpriteForMain() {
        if (overrideBodySprite != null) {
            return overrideBodySprite;
        }

        int x;
        int y = DirectionRelativeToCamera().Ordinal();
        if (jumping && HasJumpFrames()) {
            x = Mathf.FloorToInt(moveTime * JumpStepsPerSecond + (footOffset ? 1 : 0)) % 2 + 3;
        } else {
            x = Mathf.FloorToInt(moveTime * StepsPerSecond + (footOffset ? 2 : 0)) % 4;
            if (x == 3) x = 1;
            if (!stepping) x = 1;
        }
        return FrameBySlot(x, y);
    }

    private Sprite SpriteForArms() {
        if (!HasJumpFrames()) {
            return null;
        }
        if (armMode == ArmMode.Disabled && jumping) {
            return FrameBySlot(ArmMode.Overhead.FrameIndex());
        }
        if (armMode.Show()) {
            return FrameBySlot(armMode.FrameIndex());
        } else {
            return null;
        }
    }

    private Sprite SpriteForItem() {
        if (itemMode.Show()) {
            return itemSprite;
        } else {
            return null;
        }
    }

    private bool HasJumpFrames() {
        return sprites.ContainsKey(NameForFrame(spritesheet.name, 4, 0));
    }
}
