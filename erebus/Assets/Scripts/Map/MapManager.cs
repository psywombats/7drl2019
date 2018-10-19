using System;
using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;
using UnityEngine;
using UnityEngine.Assertions;

public class MapManager : MonoBehaviour, MemoryPopulater {

    public Map ActiveMap { get; private set; }
    public AvatarEvent Avatar { get; private set; }

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
            ActiveMap = FindObjectOfType<Map>();
            Avatar = ActiveMap.GetComponentInChildren<AvatarEvent>();
        }
    }

    public void SetUpInitialMap(string mapName) {
        ActiveMap = InstantiateMap(mapName);
        AddInitialAvatar();
    }

    public void PopulateMemory(Memory memory) {
        if (ActiveMap != null) {
            Avatar.PopulateMemory(memory);
            memory.mapName = ActiveMap.fullName;
        }
    }

    public void PopulateFromMemory(Memory memory) {
        if (memory.mapName != null) {
            AddInitialAvatar();
            ActiveMap = InstantiateMap(memory.mapName);
            Avatar.PopulateFromMemory(memory);
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
    
    // map path is accepted either as a relative map name "Testmap01" or full path "Test/Testmap01"
    // the .tmx or whatever extension is not needed
    private void RawTeleport(string mapName, IntVector2 location) {
        Assert.IsNotNull(ActiveMap);
        Map newMapInstance = InstantiateMap(mapName);
        RawTeleport(newMapInstance, location);
    }

    private void RawTeleport(string mapName, string targetEventName) {
        Assert.IsNotNull(ActiveMap);
        Map newMapInstance = InstantiateMap(mapName);
        MapEvent target = newMapInstance.GetEventNamed(targetEventName);
        RawTeleport(newMapInstance, target.Position);
    }

    private void RawTeleport(Map map, IntVector2 location) {
        Assert.IsNotNull(ActiveMap);
        Assert.IsNotNull(Avatar);

        int layerIndex = Avatar.GetComponent<MapEvent>().LayerIndex;
        Avatar.transform.parent = null;
        Layer parentLayer = map.LayerAtIndex(layerIndex);
        Avatar.transform.parent = parentLayer.gameObject.transform;

        ActiveMap.OnTeleportAway();
        GameObject.DestroyObject(ActiveMap.gameObject);
        ActiveMap = map;
        ActiveMap.OnTeleportTo();
        Avatar.GetComponent<MapEvent>().SetLocation(location);
    }

    private Map InstantiateMap(string mapName) {
        GameObject newMapObject = null;
        if (ActiveMap != null) {
            string localPath = ActiveMap.resourcePath + "/" + mapName;
            newMapObject = Resources.Load<GameObject>(localPath);
        }
        if (newMapObject == null) {
            newMapObject = Resources.Load<GameObject>(mapName);
        }
        Assert.IsNotNull(newMapObject);
        return Instantiate(newMapObject).GetComponent<Map>();
    }

    private void AddInitialAvatar(Memory memory = null) {
        Avatar = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/Map3D/Avatar3D")).GetComponent<AvatarEvent>();
        if (memory != null) {
            Avatar.PopulateFromMemory(memory);
        }
        Avatar.transform.parent = ActiveMap.LowestObjectLayer().transform;
        ActiveMap.OnTeleportTo();
        Camera.target = Avatar.GetComponent<MapEvent>();
        Camera.ManualUpdate();
    }

    private IEnumerator TeleportOutRoutine() {
        Avatar.PauseInput();
        yield return StartCoroutine(Camera.GetComponent<ColorEffect>().FadeRoutine(Color.black, 0.3f));
    }

    private IEnumerator TeleportInRoutine() {
        yield return StartCoroutine(Camera.GetComponent<ColorEffect>().FadeRoutine(Color.white, 0.3f));
        Avatar.UnpauseInput();
    }
}
