using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class MapManager : MonoBehaviour, MemoryPopulater {

    private static readonly string DefaultTransitionTag = "default";

    public Map activeMap { get; set; }
    public AvatarEvent avatar { get; set; }
    public DuelMap activeDuelMap { get; set; }
    public SceneBlendController blendController { get; set; }

    private MapCamera _camera;
    public new MapCamera camera {
        get {
            if (_camera == null) {
                _camera = FindObjectOfType<MapCamera>();
            }
            return _camera;
        }
        set {
            _camera = value;
        }
    }

    public void Start() {
        activeMap = FindObjectOfType<Map>();
        avatar = activeMap.GetComponentInChildren<AvatarEvent>();
    }

    public void SetUpInitialMap(string mapName) {
        activeMap = InstantiateMap(mapName);
        AddInitialAvatar();
    }

    public void PopulateMemory(Memory memory) {
        if (activeMap != null) {
            avatar.PopulateMemory(memory);
            memory.mapName = activeMap.fullName;
        }
    }

    public void PopulateFromMemory(Memory memory) {
        if (memory.mapName != null) {
            AddInitialAvatar();
            activeMap = InstantiateMap(memory.mapName);
            avatar.PopulateFromMemory(memory);
        }
    }

    public IEnumerator TeleportRoutine(string mapName, Vector2Int location) {
        avatar.PauseInput();
        TransitionData data = Global.Instance().Database.Transitions.GetData(DefaultTransitionTag);
        yield return camera.GetComponent<FadeImageEffect>().TransitionRoutine(data, () => {
            RawTeleport(mapName, location);
        });
        avatar.UnpauseInput();
    }

    public IEnumerator TeleportRoutine(string mapName, string targetEventName) {
        avatar.PauseInput();
        TransitionData data = Global.Instance().Database.Transitions.GetData(DefaultTransitionTag);
        yield return camera.GetComponent<FadeImageEffect>().TransitionRoutine(data, () => {
            RawTeleport(mapName, targetEventName);
        });
        avatar.UnpauseInput();
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
        RawTeleport(newMapInstance, target.position);
    }

    private void RawTeleport(Map map, Vector2Int location) {
        Assert.IsNotNull(activeMap);
        Assert.IsNotNull(avatar);

        avatar.transform.SetParent(map.objectLayer.transform, false);

        activeMap.OnTeleportAway();
        Destroy(activeMap.gameObject);
        activeMap = map;
        activeMap.OnTeleportTo();
        avatar.GetComponent<MapEvent>().SetLocation(location);
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

    private void AddInitialAvatar(Memory memory = null) {
        // TODO:
        avatar = Instantiate(Resources.Load<GameObject>("Prefabs/Map3D/Avatar3D")).GetComponent<AvatarEvent>();
        if (memory != null) {
            avatar.PopulateFromMemory(memory);
        }
        avatar.transform.parent = activeMap.objectLayer.transform;
        activeMap.OnTeleportTo();
        camera.target = avatar.GetComponent<MapEvent>();
        camera.ManualUpdate();
    }
}
