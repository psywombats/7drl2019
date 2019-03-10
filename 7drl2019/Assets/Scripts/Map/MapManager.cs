using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class MapManager : MonoBehaviour {

    private static readonly string DefaultTransitionTag = "default";

    public static bool seenTutorial;

    public Map activeMap { get; set; }
    public PCEvent pc { get; set; }
    public SceneBlendController blendController { get; set; }

    private MapCamera _camera;
    public new MapCamera camera {
        get {
            if (_camera == null) _camera = FindObjectOfType<MapCamera>();
            return _camera;
        }
    }

    public void Start() {
        activeMap = FindObjectOfType<Map>();
        pc = activeMap.GetComponentInChildren<PCEvent>();
    }

    public IEnumerator NextMapRoutine() {
        EightDir facing = pc.GetComponent<CharaEvent>().facing;
        yield return pc.GetComponent<MapEvent>().StepMultiRoutine(facing, 3);

        TransitionData data = Global.Instance().Database.Transitions.GetData(DefaultTransitionTag);
        yield return camera.cam.GetComponent<FadeImageEffect>().TransitionRoutine(data, RawNextMapRoutine());
        facing = pc.GetComponent<CharaEvent>().facing;
        yield return pc.GetComponent<MapEvent>().StepMultiRoutine(facing, 3);
    }

    public IEnumerator RawNextMapRoutine() {
        bool swapped = false;
        pc.GetComponent<MapEvent>().SetLocation(new Vector2Int(0, 0));
        activeMap.GetComponent<BattleController>().Clear();

        MapGenerator oldMap = activeMap.GetComponent<MapGenerator>();
        int level = oldMap.level;
        MapGenerator newGen = activeMap.gameObject.AddComponent<MapGenerator>();
        newGen.GenerateMesh(oldMap);
        Destroy(oldMap);
        activeMap.GetComponent<LineOfSightEffect>().Erase();

        if (activeMap != oldMap) {
            swapped = true;
            pc.GetComponent<CharaEvent>().facing = EightDir.N;
            Destroy(oldMap.gameObject);
            activeMap.AddEvent(pc.GetComponent<MapEvent>());
        }

        MapCamera cam = FindObjectOfType<MapCamera>();
        if (cam.target != pc.GetComponent<MapEvent3D>()) {
            cam.target = activeMap.GetEventNamed("ZoomTarget").GetComponent<MapEvent3D>();
        }

        activeMap.GetComponent<LineOfSightEffect>().RegenSitemap(activeMap.GetComponent<TacticsTerrainMesh>());

        RogueUI ui = FindObjectOfType<RogueUI>();
        ui.narrator.Clear();
        if (level == 0 && !seenTutorial) {
            seenTutorial = true;
            yield return ui.TutorialRoutine();
        }
        yield return ui.EditSpellsRoutine();

        Vector2Int loc = activeMap.GetEventNamed("TeleStart").location;
        EightDir dir = pc.GetComponent<CharaEvent>().facing;
        loc += dir.XY() * -2;
        pc.GetComponent<MapEvent>().SetLocation(loc);
        activeMap.GetComponent<LineOfSightEffect>().RecalculateVisibilityMap();

        if (swapped) {
            StartCoroutine(activeMap.GetComponent<BattleController>().BattleRoutine());
        }
    }
    
    private void RawTeleport(string mapName, Vector2Int location) {
        Assert.IsNotNull(activeMap);
        Map newMapInstance = InstantiateMap(mapName);
        RawTeleport(newMapInstance, location);
    }

    private void RawTeleport(string mapName, string targetEventName) {
        Assert.IsNotNull(activeMap);
        Map newMapInstance = InstantiateMap(mapName);
        MapEvent target = newMapInstance.GetEventNamed(targetEventName);
        RawTeleport(newMapInstance, target.location);
    }

    private void RawTeleport(Map map, Vector2Int location) {
        Assert.IsNotNull(activeMap);
        Assert.IsNotNull(pc);

        pc.transform.SetParent(map.objectLayer.transform, false);

        activeMap.OnTeleportAway();
        Destroy(activeMap.gameObject);
        activeMap = map;
        activeMap.OnTeleportTo();
        pc.GetComponent<MapEvent>().SetLocation(location);
    }

    private Map InstantiateMap(string mapName) {
        GameObject newMapObject = null;
        if (activeMap != null) {
            string localPath = Map.ResourcePath + mapName;
            newMapObject = Resources.Load<GameObject>(localPath);
        }
        if (newMapObject == null) {
            newMapObject = Resources.Load<GameObject>(mapName);
        }
        Assert.IsNotNull(newMapObject);
        return Instantiate(newMapObject).GetComponent<Map>();
    }
}
