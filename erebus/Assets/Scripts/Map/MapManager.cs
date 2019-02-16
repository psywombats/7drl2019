using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class MapManager : MonoBehaviour, MemoryPopulater {

    public Map activeMap { get; set; }
    public AvatarEvent avatar { get; set; }
    public DuelMap activeDuelMap { get; set; }
    public SceneBlendController blendController { get; set; }

    private MapCamera mapCamera;
    public MapCamera Camera {
        get {
            if (mapCamera == null) {
                mapCamera = FindObjectOfType<MapCamera>();
            }
            return mapCamera;
        }
    }
    public void SetCamera(MapCamera cam) {
        Debug.Assert(mapCamera == null);
        mapCamera = cam;
    }

    public void Start() {
        Global.Instance().Memory.RegisterMemoryPopulater(this);
        DebugMapMarker debugMap = FindObjectOfType<DebugMapMarker>();
        if (debugMap != null) {
            activeMap = FindObjectOfType<Map>();
            avatar = activeMap.GetComponentInChildren<AvatarEvent>();
        }
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

    public IEnumerator TeleportRoutine(string mapName, IntVector2 location) {
        yield return StartCoroutine(TeleportOutRoutine());
        RawTeleport(mapName, location);
        yield return StartCoroutine(TeleportInRoutine());
    }

    public IEnumerator TeleportRoutine(string mapName, string targetEventName) {
        yield return StartCoroutine(TeleportOutRoutine());
        RawTeleport(mapName, targetEventName);
        yield return StartCoroutine(TeleportInRoutine());
    }
    
    private void RawTeleport(string mapName, IntVector2 location) {
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

    private void RawTeleport(Map map, IntVector2 location) {
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
        avatar = Instantiate(Resources.Load<GameObject>("Prefabs/Map3D/Avatar3D")).GetComponent<AvatarEvent>();
        if (memory != null) {
            avatar.PopulateFromMemory(memory);
        }
        avatar.transform.parent = activeMap.objectLayer.transform;
        activeMap.OnTeleportTo();
        Camera.target = avatar.GetComponent<MapEvent>();
        Camera.ManualUpdate();
    }

    private IEnumerator TeleportOutRoutine() {
        avatar.PauseInput();
        yield return null;
    }

    private IEnumerator TeleportInRoutine() {
        avatar.UnpauseInput();
        yield return null;
    }
}
