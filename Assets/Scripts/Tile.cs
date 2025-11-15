using UnityEngine;

public class Tile
{
    public Vector2Int GridPosition { get; private set; }
    public TerrainType Terrain { get; set; }
    public bool IsPassable => Terrain != TerrainType.Water && Terrain != TerrainType.HighMountains;
    public GameObject SettlementObject { get; set; }
    public GameObject ArmyObject { get; set; }
    public Settlement Settlement { get; set; }
    public Army Army { get; set; }

    public Tile(int x, int y, TerrainType terrain)
    {
        GridPosition = new Vector2Int(x, y);
        Terrain = terrain;
    }

    public bool HasSettlement => Settlement != null;
    public bool HasArmy => Army != null;
    public TileVisual Visual { get; set; }

}