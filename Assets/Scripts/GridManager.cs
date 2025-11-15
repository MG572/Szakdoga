using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public float tileSize = 1f;
    private Tile[,] grid;
    public Tile[,] Tiles => grid;
    public GameObject settlementPrefab;
    public GameObject armyPrefab;
    public Material tileMaterial;

    public static GridManager Instance { get; private set; }

    private Dictionary<char, TerrainType> terrainLookup = new Dictionary<char, TerrainType> {
        { 'G', TerrainType.Grassland },
        { 'A', TerrainType.Water },
        { 'D', TerrainType.Desert },
        { 'H', TerrainType.Hills },
        { 'M', TerrainType.Mountains },
        { 'W', TerrainType.Woodland },
        { 'T', TerrainType.HighMountains },
        { 'S', TerrainType.Snow }
    };

    void Awake()
    {
        settlementPrefab = Resources.Load<GameObject>("SettlementPrefab");
        armyPrefab = Resources.Load<GameObject>("ArmyPrefab");

        if (tileMaterial == null)
        {
            tileMaterial = Resources.Load<Material>("TileMaterial");
            if (tileMaterial == null)
                UnityEngine.Debug.LogError("Failed to load TileMaterial from Resources!");
            else
                UnityEngine.Debug.Log("Loaded TileMaterial successfully from Resources.");
        }

        if (settlementPrefab == null)
        {
            UnityEngine.Debug.LogError("Failed to load SettlementPrefab from Resources!");
        }

        if (armyPrefab == null) 
        {
            UnityEngine.Debug.LogError("Failed to load ArmyPrefab from Resources!");
        }

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        if (settlementPrefab == null)
            settlementPrefab = Resources.Load<GameObject>("SettlementPrefab");
        if (armyPrefab == null)
            armyPrefab = Resources.Load<GameObject>("ArmyPrefab");

        LoadMapFromFile("map01");
        PlaceInitialObjects();

        UnityEngine.Debug.Log($"Settlement prefab is null? {settlementPrefab == null}");
        UnityEngine.Debug.Log($"Army prefab is null? {armyPrefab == null}");

        if (grid != null && grid.GetLength(0) > 1 && grid.GetLength(1) > 1)
        {
            UnityEngine.Debug.Log($"Tile (1,1) has settlement? {grid[1, 1].HasSettlement}");
            UnityEngine.Debug.Log($"Tile (2,0) has army? {grid[2, 0].HasArmy}");
        }

        SpawnAllVisuals();

        StartCoroutine(AssignObjectsToPlayersWhenReady());
    }

    System.Collections.IEnumerator AssignObjectsToPlayersWhenReady()
    {
        int attempts = 0;
        while (TurnManager.Instance == null && attempts < 50)
        {
            yield return new WaitForSeconds(0.1f);
            attempts++;
        }

        if (TurnManager.Instance == null)
        {
            UnityEngine.Debug.LogError("TurnManager.Instance is still null after waiting");
            yield break;
        }

        UnityEngine.Debug.Log("TurnManager is now available");

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                if (grid[x, y].HasSettlement)
                {
                    string faction = grid[x, y].Settlement.Faction;

                    foreach (var player in TurnManager.Instance.Players)
                    {
                        if (player.Name == faction)
                        {
                            player.AddSettlement(grid[x, y].Settlement);
                            UnityEngine.Debug.Log($"Settlement {grid[x, y].Settlement.Name} added to player {player.Name}");
                            break;
                        }
                    }
                }

                if (grid[x, y].HasArmy)
                {
                    string faction = grid[x, y].Army.Faction;
                    foreach (var player in TurnManager.Instance.Players)
                    {
                        if (player.Name == faction)
                        {
                            player.AddArmy(grid[x, y].Army);
                            UnityEngine.Debug.Log($"Army added to player {player.Name} with {grid[x, y].Army.Units.Count} units and {grid[x, y].Army.TotalSoldiers()} total soldiers");
                            break;
                        }
                    }
                }
            }
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
        }
    }

    void LoadMapFromFile(string fileName)
    {
        TextAsset mapText = Resources.Load<TextAsset>(fileName);
        if (mapText == null)
        {
            UnityEngine.Debug.LogError($"Map file '{fileName}' not found");

            // Backup map
            int defaultSize = 10;
            grid = new Tile[defaultSize, defaultSize];
            for (int x = 0; x < defaultSize; x++)
            {
                for (int y = 0; y < defaultSize; y++)
                {
                    grid[x, y] = new Tile(x, y, TerrainType.Grassland);
                }
            }
            return;
        }

        string[] lines = mapText.text.Trim().Split('\n');
        int width = lines.Length;
        int height = lines[0].Trim().Split(' ').Length;

        grid = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            string[] tokens = lines[x].Trim().Split(' ');

            for (int y = 0; y < height; y++)
            {
                if (y < tokens.Length && tokens[y].Length > 0)
                {
                    char terrainCode = tokens[y][0];
                    if (!terrainLookup.TryGetValue(terrainCode, out TerrainType terrain))
                    {
                        UnityEngine.Debug.LogWarning($"Unrecognized terrain code '{terrainCode}' at ({x},{y})");
                        terrain = TerrainType.Grassland;
                    }

                    grid[x, y] = new Tile(x, y, terrain);
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"Map data missing at ({x},{y}), using Grassland");
                    grid[x, y] = new Tile(x, y, TerrainType.Grassland);
                }
            }
        }
    }

    void SpawnTileVisual(int x, int y, Tile tile)
    {
        GameObject tileObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        tileObj.transform.position = new Vector3(x * tileSize, y * tileSize, 0);
        tileObj.transform.localScale = Vector3.one * tileSize;
        Renderer renderer = tileObj.GetComponent<Renderer>();

        if (tileMaterial != null)
        {
            renderer.material = new Material(tileMaterial);
            renderer.material.color = TerrainColor(tile.Terrain);
        }
        else
        {
            UnityEngine.Debug.LogError("TileMaterial not assigned in GridManager!");
        }
        TileVisual visual = tileObj.AddComponent<TileVisual>();
        visual.tileData = tile;

        tile.Visual = visual;

        ClickableObject clickable = tileObj.AddComponent<ClickableObject>();
        clickable.linkedTile = tile;
    }

    void SpawnAllVisuals()
    {
        if (grid == null)
        {
            UnityEngine.Debug.LogError("Cannot spawn visuals: grid is null!");
            return;
        }

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                SpawnTileVisual(x, y, grid[x, y]);
            }
        }

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                if (grid[x, y].HasSettlement)
                {
                    UnityEngine.Debug.Log($"At spawn time, tile({x},{y}) has settlement: {grid[x, y].Settlement.Name}");
                }
                SpawnObjectsOnTile(x, y, grid[x, y]);
            }
        }
    }

    void SpawnObjectsOnTile(int x, int y, Tile tile)
    {
        Vector3 tilePos = new Vector3(x * tileSize, y * tileSize, 0);

        if (tile.HasSettlement && settlementPrefab != null)
        {
            Vector3 offset = new Vector3(0, 0, 0);
            UnityEngine.Debug.Log($"Spawning settlement at ({x},{y})");
            UnityEngine.Debug.Log($"Settlement data: {tile.Settlement?.Name}");

            GameObject s = Instantiate(settlementPrefab, tilePos + offset, Quaternion.identity);
            s.transform.localScale = Vector3.one * 0.2f;
            s.tag = "Settlement";
            tile.SettlementObject = s;

            ClickableObject clickable = s.GetComponent<ClickableObject>();
            if (clickable == null)
            {
                clickable = s.AddComponent<ClickableObject>();
            }

            clickable.linkedTile = tile;
            clickable.linkedSettlement = tile.Settlement;

            UnityEngine.Debug.Log($"Settlement GameObject created with name: {s.name}");
            UnityEngine.Debug.Log($"ClickableObject linked to tile: {clickable.linkedTile != null}");
            UnityEngine.Debug.Log($"ClickableObject linked to settlement: {clickable.linkedSettlement != null}");
            if (clickable.linkedSettlement != null)
            {
                UnityEngine.Debug.Log($"Settlement name in clickable: {clickable.linkedSettlement.Name}");
            }
        }

        if (tile.HasArmy && armyPrefab != null)
        {
            Vector3 offset = new Vector3(0, 0, 0);
            GameObject a = Instantiate(armyPrefab, tilePos + offset, Quaternion.identity);
            a.transform.localScale = Vector3.one * 0.2f;
            a.tag = "Army";
            tile.ArmyObject = a;

            ClickableObject clickable = a.GetComponent<ClickableObject>();
            if (clickable == null)
            {
                clickable = a.AddComponent<ClickableObject>();
            }

            clickable.linkedTile = tile;
            clickable.linkedArmy = tile.Army;
        }
    }

    void PlaceInitialObjects()
    {
        if (grid == null)
        {
            UnityEngine.Debug.LogError("Cannot place initial objects: grid is null!");
            return;
        }

        if (grid.GetLength(0) > 2 && grid.GetLength(1) > 5)
        {
            Settlement firstTown = new Settlement("Firsttown", "Player", new Vector2Int(1, 1));
            UnityEngine.Debug.Log($"Created new settlement: {firstTown.Name}");

            grid[1, 1].Settlement = firstTown;
            firstTown.Tile = grid[1, 1];
            UnityEngine.Debug.Log($"Assigned settlement to grid[1,1], now Settlement is: {grid[1, 1].Settlement?.Name}");

            firstTown.Garrison.AddUnit(new Unit(UnitDatabase.UnitTypes["Militia Spearman"], 80));
            firstTown.Garrison.AddUnit(new Unit(UnitDatabase.UnitTypes["Militia Archer"], 70));

            Army startingArmy = new Army("Player");
            startingArmy.AddUnit("Militia Spearman", 100);
            startingArmy.AddUnit("Militia Spearman", 100);
            startingArmy.AddUnit("Militia Archer", 100);
            grid[2, 0].Army = startingArmy;

            Army secondArmy = new Army("Player");
            secondArmy.AddUnit("Militia Spearman", 100);
            secondArmy.AddUnit("Militia Spearman", 100);
            secondArmy.AddUnit("Militia Archer", 100);
            grid[3, 6].Army = secondArmy;

            Settlement secondTown = new Settlement("Secondtown", "Player", new Vector2Int(2, 5));
            UnityEngine.Debug.Log($"Created new settlement: {secondTown.Name}");

            grid[2, 5].Settlement = secondTown;
            secondTown.Tile = grid[2, 5];
            UnityEngine.Debug.Log($"Assigned settlement to grid[2,5]");

            secondTown.Garrison.AddUnit(new Unit(UnitDatabase.UnitTypes["Militia Spearman"], 90));
            secondTown.Garrison.AddUnit(new Unit(UnitDatabase.UnitTypes["Militia Archer"], 60));

            //OPPONENT
            Settlement enemyTown = new Settlement("Enemytown", "AI", new Vector2Int(3, 10));
            UnityEngine.Debug.Log($"Created new settlement: {enemyTown.Name}");

            grid[7, 4].Settlement =enemyTown;
            enemyTown.Tile = grid[7, 4];
            UnityEngine.Debug.Log($"Assigned settlement to grid[2,10]");

            enemyTown.Garrison.AddUnit(new Unit(UnitDatabase.UnitTypes["Militia Spearman"], 90));
            enemyTown.Garrison.AddUnit(new Unit(UnitDatabase.UnitTypes["Militia Archer"], 60));

            //opponent settlement 2
            Settlement enemyTown2 = new Settlement("Enemytown2", "AI", new Vector2Int(6, 15));
            UnityEngine.Debug.Log($"Created new settlement: {enemyTown2.Name}");

            grid[6, 15].Settlement = enemyTown2;
            enemyTown2.Tile = grid[6, 15];
            UnityEngine.Debug.Log($"Assigned settlement to grid[6,15]");

            Army startingEnemyArmy = new Army("AI");
            startingEnemyArmy.AddUnit("Militia Spearman", 100);
            startingEnemyArmy.AddUnit("Militia Spearman", 100);
            startingEnemyArmy.AddUnit("Militia Archer", 100);
            grid[5, 3].Army = startingEnemyArmy;
        }

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                if (grid[x, y].HasSettlement)
                {
                    UnityEngine.Debug.Log($"Tile ({x},{y}) has settlement: {grid[x, y].Settlement?.Name}");
                }
            }
        }
    }

    Color TerrainColor(TerrainType type)
    {
        return type switch
        {
            TerrainType.Grassland => Color.green,
            TerrainType.Woodland => new Color(0.3f, 0.5f, 0.2f),
            TerrainType.Desert => Color.yellow,
            TerrainType.Water => Color.blue,
            TerrainType.Hills => new Color(0.5f, 0.4f, 0.1f),
            TerrainType.Mountains => Color.gray,
            TerrainType.Snow => Color.white,
            TerrainType.HighMountains => Color.black,
            _ => Color.magenta
        };
    }

    public Tile GetTileAt(int x, int y)
    {
        if (x < 0 || y < 0 || x >= grid.GetLength(0) || y >= grid.GetLength(1))
            return null;

        return grid[x, y];
    }

    public Tile GetTileOfArmy(Army army)
    {
        foreach (var tile in Tiles)
        {
            if (tile != null && tile.Army == army)
                return tile;
        }
        return null;
    }


    public List<Tile> GetAdjacentTiles(Tile centerTile, bool includeDiagonals = true)
    {
        List<Tile> neighbors = new List<Tile>();
        Vector2Int pos = centerTile.GridPosition;

        Vector2Int[] directions = includeDiagonals
            ? new Vector2Int[]
            {
            new Vector2Int(1, 0),   // East
            new Vector2Int(-1, 0),  // West
            new Vector2Int(0, 1),   // North
            new Vector2Int(0, -1),  // South
            new Vector2Int(1, 1),   // NE
            new Vector2Int(-1, 1),  // NW
            new Vector2Int(1, -1),  // SE
            new Vector2Int(-1, -1)  // SW
            }
            : new Vector2Int[]
            {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
            };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborPos = pos + dir;
            Tile neighborTile = GetTileAt(neighborPos.x, neighborPos.y);

            if (neighborTile != null)
            {
                neighbors.Add(neighborTile);
            }
        }

        return neighbors;
    }

    public List<Tile> GetTilesInRange(Tile startTile, int range, bool includeDiagonals = true)
    {
        List<Tile> result = new List<Tile>();
        Queue<(Tile tile, int distance)> queue = new Queue<(Tile, int)>();
        HashSet<Tile> visited = new HashSet<Tile>();

        queue.Enqueue((startTile, 0));
        visited.Add(startTile);

        while (queue.Count > 0)
        {
            var (current, dist) = queue.Dequeue();
            result.Add(current);

            if (dist >= range) continue;

            foreach (var neighbor in GetAdjacentTiles(current, includeDiagonals))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue((neighbor, dist + 1));
                }
            }
        }
        return result;
    }
}