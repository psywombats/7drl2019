using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(TacticsTerrainMesh))]
public class LineOfSightEffect : MonoBehaviour {

    public Texture2D oldLosTexture;
    public Texture2D losTexture;
    public float visBlend = 1.0f;

    public void OnDestroy() {
        if (losTexture != null) {
            Destroy(losTexture);
        }
    }

    public void Update() {
        AssignCommonShaderVariables();
    }

    public void OnValidate() {
        AssignCommonShaderVariables();
    }

    public void TransitionFromOldLos(float duration) {
        visBlend = 0.0f;
        Tweener tween = DOTween.To(() => visBlend, x => visBlend = x, 1.0f, duration);
        tween.SetEase(Ease.Linear);
        StartCoroutine(CoUtils.RunTween(tween));
    }

    public void RecalculateVisibilityMap() {
        AvatarEvent hero;
        if (Application.isPlaying) {
            hero = Global.Instance().Maps.avatar;
        } else {
            hero = FindObjectOfType<AvatarEvent>();
        }
        TacticsTerrainMesh mesh = GetComponent<TacticsTerrainMesh>();

        if (oldLosTexture != null) {
            Destroy(oldLosTexture);
        }
        oldLosTexture = losTexture;
        losTexture = new Texture2D(mesh.size.x, mesh.size.y, TextureFormat.ARGB32, false);
        losTexture.filterMode = FilterMode.Point;
        losTexture.wrapMode = TextureWrapMode.Clamp;

        Color[] map = new Color[mesh.size.x * mesh.size.y];
        for (int y = 0; y < mesh.size.y; y += 1) {
            for (int x = 0; x < mesh.size.x; x += 1) {
                bool visible = hero.GetComponent<BattleEvent>().CanSeeLocation(new Vector2Int(x, y));
                map[y * mesh.size.x + x] = new Color(visible ? 1.0f : 0.0f, 1.0f, 1.0f);
            }
        }
        losTexture.SetPixels(0, 0, mesh.size.x, mesh.size.y, map);
        losTexture.Apply();
    }

    private void AssignCommonShaderVariables() {
        Material material = FindMaterial();
        TacticsTerrainMesh mesh = GetComponent<TacticsTerrainMesh>();
        material.SetFloat("_CellResolutionX", mesh.size.x);
        material.SetFloat("_CellResolutionY", mesh.size.y);
        material.SetTexture("_VisibilityTex", losTexture);
        material.SetFloat("_VisibilityBlend", visBlend);
        if (oldLosTexture != null) {
            material.SetTexture("_OldVisibilityTex", oldLosTexture);
        } else {
            material.SetTexture("_OldVisibilityTex", losTexture);
        }
    }

    private Material FindMaterial() {
        if (Application.isPlaying) {
            return GetComponent<MeshRenderer>().material;
        } else {
            return GetComponent<MeshRenderer>().sharedMaterial;
        }
    }
}
