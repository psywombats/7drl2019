using DG.Tweening;
using UnityEngine;
using UnityEngine.Profiling;

[RequireComponent(typeof(TacticsTerrainMesh))]
[ExecuteInEditMode]
public class LineOfSightEffect : MonoBehaviour {

    public Texture2D oldLosTexture;
    public Texture2D losTexture;
    public float visBlend = 1.0f;
    public Vector3 heroPos, oldHeroPos;

    private Color[] map;
    private bool[,] seenMap;
    private PCEvent pc;
    private TacticsTerrainMesh mesh;

    public void OnDestroy() {
        if (losTexture != null) {
            Destroy(losTexture);
        }
    }

    public void Update() {
        AssignCommonShaderVariables();
    }

    public void Erase() {
        Destroy(losTexture);
        if (oldLosTexture != null) {
            Destroy(oldLosTexture);
        }
        losTexture = null;
        oldLosTexture = null;
        seenMap = null;
    }

    public void TransitionFromOldLos(float duration) {
        visBlend = 0.0f;
        Tweener tween = DOTween.To(() => visBlend, x => visBlend = x, 1.0f, duration);
        tween.SetEase(Ease.Linear);
        StartCoroutine(CoUtils.RunTween(tween));
    }

    public bool CachedIsVisible(Vector2Int loc) {
        return map[loc.y * mesh.size.x + loc.x].r > 0.5f;
    }

    public void RecalculateVisibilityMap() {
        Profiler.BeginSample("los");
        mesh = GetComponent<TacticsTerrainMesh>();

        if (seenMap == null) {
            seenMap = new bool[mesh.size.x, mesh.size.y];
            for (int y = 0; y < mesh.size.y; y += 1) {
                for (int x = 0; x < mesh.size.x; x += 1) {
                    seenMap[x, y] = false;
                }
            }
        if (Application.isPlaying) {
                pc = Global.Instance().Maps.pc;
            }
            if (pc == null) {
                pc = FindObjectOfType<PCEvent>();
            }
        }

        if (oldLosTexture != null) {
            Destroy(oldLosTexture);
        }
        oldLosTexture = losTexture;
        oldHeroPos = heroPos;
        losTexture = new Texture2D(mesh.size.x, mesh.size.y, TextureFormat.ARGB32, false);
        losTexture.filterMode = FilterMode.Point;
        losTexture.wrapMode = TextureWrapMode.Clamp;
        heroPos = pc.GetComponent<MapEvent3D>().TileToWorldCoords(pc.GetComponent<MapEvent>().location);
        heroPos += new Vector3(0.5f, 1.5f, 0.5f);

        map = new Color[mesh.size.x * mesh.size.y];
        BattleEvent battler = pc.GetComponent<BattleEvent>();
        Vector2Int v = new Vector2Int();
        for (int y = 0; y < mesh.size.y; y += 1) {
            for (int x = 0; x < mesh.size.x; x += 1) {
                float r = 1.0f;
                float b = 1.0f;
                v.x = x;
                v.y = y;
                if (battler.CanSeeLocation(mesh, v)) {
                    seenMap[x, y] = true;
                } else {
                    r = 0.0f;
                    b = seenMap[x, y] ? 1.0f : 0.0f;
                }
                map[y * mesh.size.x + x] = new Color(r, 0.0f, b);
            }
        }
        losTexture.SetPixels(0, 0, mesh.size.x, mesh.size.y, map);
        losTexture.Apply();

        foreach (PCVisibility vis in GetComponent<Map>().GetEvents<PCVisibility>()) {
            Vector2Int location = vis.GetComponent<MapEvent>().location;
            vis.SetVisibleByPC(CachedIsVisible(location));
        }

        Profiler.EndSample();
    }

    private void AssignCommonShaderVariables() {
        Material material = FindMaterial();
        if (Application.isPlaying) {
            if (pc == null || pc.GetComponent<BattleEvent>() == null) return;
            TacticsTerrainMesh mesh = GetComponent<TacticsTerrainMesh>();

            material.SetFloat("_CellResolutionX", mesh.size.x);
            material.SetFloat("_CellResolutionY", mesh.size.y);
            material.SetTexture("_VisibilityTex", losTexture);
            material.SetFloat("_VisibilityBlend", visBlend);
            material.SetVector("_HeroPos", heroPos);
            material.SetFloat("_SightRange", pc.GetComponent<BattleEvent>().unit.Get(StatTag.SIGHT));
            if (oldLosTexture != null) {
                material.SetTexture("_OldVisibilityTex", oldLosTexture);
            } else {
                material.SetTexture("_OldVisibilityTex", losTexture);
            }
            if (oldHeroPos == Vector3.zero) {
                material.SetVector("_OldHeroPos", heroPos);
            } else {
                material.SetVector("_OldHeroPos", oldHeroPos);
            }
        } else {
            material.SetFloat("_SightRange", 1000.0f);
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
