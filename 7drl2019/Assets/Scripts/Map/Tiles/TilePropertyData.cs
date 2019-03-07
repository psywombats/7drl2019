[System.Serializable]
public class TilePropertyData {

    public bool impassable = true;

    public TilePropertyData(bool passable) {
        this.impassable = passable;
    }
}
