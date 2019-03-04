using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class MapManager : MonoBehaviour {

    private static readonly string DefaultTransitionTag = "default";

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
        yield return camera.cam.GetComponent<FadeImageEffect>().TransitionRoutine(data, () => {
            RawNextMap();
        });
        facing = pc.GetComponent<CharaEvent>().facing;
        yield return pc.GetComponent<MapEvent>().StepMultiRoutine(facing, 3);
    }

    public void RawNextMap() {
        pc.GetComponent<MapEvent>().SetLocation(new Vector2Int(0, 0));
        activeMap.GetComponent<BattleController>().Clear();

        MapGenerator oldMap = activeMap.GetComponent<MapGenerator>();
        MapGenerator newGen = activeMap.gameObject.AddComponent<MapGenerator>();
        newGen.GenerateMesh(oldMap);
        Destroy(oldMap);
        activeMap.GetComponent<LineOfSightEffect>().Erase();

        Vector2Int loc = activeMap.GetEventNamed("TeleStart").location;
        EightDir dir = pc.GetComponent<CharaEvent>().facing;
        loc += dir.XY() * -2;
        pc.GetComponent<MapEvent>().SetLocation(loc);
        
    }

    public IEnumerator TeleportRoutine(string mapName, Vector2Int location) {
        pc.PauseInput();
        TransitionData data = Global.Instance().Database.Transitions.GetData(DefaultTransitionTag);
        yield return camera.GetComponent<FadeImageEffect>().TransitionRoutine(data, () => {
            RawTeleport(mapName, location);
        });
        pc.UnpauseInput();
    }

    public IEnumerator TeleportRoutine(string mapName, string targetEventName) {
        pc.PauseInput();
        TransitionData data = Global.Instance().Database.Transitions.GetData(DefaultTransitionTag);
        yield return camera.GetComponent<FadeImageEffect>().TransitionRoutine(data, () => {
            RawTeleport(mapName, targetEventName);
        });
        pc.UnpauseInput();
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
