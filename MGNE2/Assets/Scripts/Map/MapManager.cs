using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;
using UnityEngine;
using UnityEngine.Assertions;

public class MapManager : MonoBehaviour {

    private Map activeMap;
    public Map ActiveMap {
        get {
            if (activeMap == null) {
                activeMap = GameObject.FindObjectOfType<Map>();
            }
            return activeMap;
        }
        private set {
            activeMap = value;
        }
    }

    private AvatarEvent avatar;
    public AvatarEvent Avatar {
        get {
            if (avatar == null) {
                avatar = ActiveMap.GetComponentInChildren<AvatarEvent>();
            }
            return avatar;
        }
    }
    
    // map path is accepted either as a relative map name "Testmap01" or full path "Test/Testmap01"
    // the .tmx or whatever extension is not needed
    public void Teleport(string mapName, IntVector2 location) {
        Assert.IsNotNull(ActiveMap);
        string localPath = ActiveMap.ResourcePath + "/" + mapName;
        GameObject newMapObject = Resources.Load<GameObject>(localPath);
        if (newMapObject == null) {
            newMapObject = Resources.Load<GameObject>(mapName);
        }
        Assert.IsNotNull(newMapObject);
        GameObject newMapInstace = Instantiate(newMapObject);

        Teleport(newMapInstace.GetComponent<Map>(), location);
    }

    public void Teleport(Map map, IntVector2 location) {
        Assert.IsNotNull(ActiveMap);
        Assert.IsNotNull(Avatar);

        int layerIndex = Avatar.GetComponent<MapEvent>().LayerIndex;
        Avatar.transform.parent = null;
        Layer parentLayer = map.LayerAtIndex(layerIndex);
        Avatar.transform.parent = parentLayer.gameObject.transform;

        GameObject.DestroyObject(ActiveMap.gameObject);
        ActiveMap = map;
        Avatar.GetComponent<MapEvent>().SetLocation(location);
    }
}
