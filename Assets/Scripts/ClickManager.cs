using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickManager : MonoBehaviour
{
    public GameObject highlightPrefab;
    private GameObject highlightInstance;
    public UIManager uiManager;

    Tile selectedArmyTile;
    Army selectedArmy;
    List<Tile> currentMoveRangeTiles = new List<Tile>();
    public static ClickManager Instance { get;  private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (highlightPrefab == null)
        {
            UnityEngine.Debug.LogWarning("highlightPrefab not assigned, skipping instantiation");
            return;
        }

        highlightInstance = Instantiate(highlightPrefab);
        highlightInstance.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            DetectClick(); 
        }

        if (Input.GetMouseButtonDown(1)) 
        {
            if (selectedArmy != null && selectedArmy.Faction=="Player")
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit) && !hit.collider.CompareTag("Army"))
                {
                    TryMoveToHoveredTile();
                }
            }
        }
    }

    void DetectClick()
    {
        ClearAllHighlights();
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            GameObject clickedObject = hit.collider.gameObject;
            UnityEngine.Debug.Log("Clicked GameObject: " + clickedObject.name);

            if (clickedObject.CompareTag("Army"))
            {
                UnityEngine.Debug.Log("Clicked on an Army!");
                MoveHighlight(clickedObject.transform.position);
                uiManager.HideSettlementInfo();
                uiManager.HideEnemySettlementInfo();

                ClickableObject clickable = clickedObject.GetComponent<ClickableObject>();
                if (clickable != null && clickable.linkedArmy != null && clickable.linkedTile != null)
                {
                    selectedArmy = clickable.linkedArmy;
                    selectedArmyTile = clickable.linkedTile;

                    UnityEngine.Debug.Log($"Selected army at tile: {selectedArmyTile.GridPosition}");
                    UnityEngine.Debug.Log($"Army movement remaining: {selectedArmy.RemainingMovement}");
                    if (selectedArmy.RemainingMovement > 0)
                    {
                        currentMoveRangeTiles = GridManager.Instance.GetTilesInRange(selectedArmyTile, selectedArmy.RemainingMovement);
                        HighlightMoveRange(currentMoveRangeTiles);
                    }

                    UnityEngine.Debug.Log($"Showing Army UI for {clickable.linkedArmy.Faction} Army");
                    uiManager.ShowArmyPanel(clickable.linkedArmy);
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Clicked on an Army, but it has no linked Army or Tile component.");
                }
            }

            else if (clickedObject.CompareTag("Settlement"))
            {
                UnityEngine.Debug.Log("Clicked on a Settlement!");
                MoveHighlight(clickedObject.transform.position);

                ClickableObject clickable = clickedObject.GetComponent<ClickableObject>();
                UnityEngine.Debug.Log($"Clickable component exists? {clickable != null}");

                if (clickable != null)
                {
                    UnityEngine.Debug.Log($"LinkedTile exists? {clickable.linkedTile != null}");
                    if (clickable.linkedTile != null)
                    {
                        UnityEngine.Debug.Log($"Tile has settlement? {clickable.linkedTile.HasSettlement}");
                        UnityEngine.Debug.Log($"Tile settlement name: {clickable.linkedTile.Settlement?.Name}");
                    }

                    UnityEngine.Debug.Log($"Linked settlement exists? {clickable.linkedSettlement != null}");
                    if (clickable.linkedSettlement != null)
                    {
                        UnityEngine.Debug.Log($"Linked settlement name: {clickable.linkedSettlement.Name}");
                    }

                    if (clickable.linkedSettlement == null && clickable.linkedTile != null && clickable.linkedTile.HasSettlement)
                    {
                        UnityEngine.Debug.Log("Attempting to get settlement from tile instead");
                        clickable.linkedSettlement = clickable.linkedTile.Settlement;
                    }

                    if (clickable.linkedSettlement != null && clickable.linkedSettlement.Faction=="Player")
                    {
                        UnityEngine.Debug.Log($"Clicked on Settlement: {clickable.linkedSettlement.Name}");
                        uiManager.ShowSettlementInfo(clickable.linkedSettlement);
                    }
                    else if (clickable.linkedSettlement != null && clickable.linkedSettlement.Faction=="AI")
                    {
                        UnityEngine.Debug.Log($"Clicked on Enemy Settlement: {clickable.linkedSettlement.Name}");
                        uiManager.ShowEnemySettlementInfo(clickable.linkedSettlement);
                    }
                    else
                    {
                        UnityEngine.Debug.Log("Clicked on a Settlement but couldn't find its data.");
                    }
                }
                else
                {
                    UnityEngine.Debug.Log("Settlement has no ClickableObject component.");
                }
            }
            else
            {
                ClickableObject clickable = clickedObject.GetComponent<ClickableObject>();
                selectedArmy = null;
                if (clickable != null)
                {
                    Tile clickedTile = clickable.linkedTile;
                    HandleTileClick(clickedTile);
                    uiManager.HideSettlementInfo();
                    uiManager.HideEnemySettlementInfo();
                }
                else
                {
                    UnityEngine.Debug.Log("Clicked on unknown object.");
                }
            }
        }
    }

    void HandleTileClick(Tile tile)
    {
        if (tile == null) return;

        UnityEngine.Debug.Log($"Clicked Tile at ({tile.GridPosition.x}, {tile.GridPosition.y}) - Terrain: {tile.Terrain}");

        if (uiManager != null)
        {
            uiManager.SetTerrainInfo(tile.Terrain.ToString());
        }
        if (selectedArmy != null && !currentMoveRangeTiles.Contains(tile))
        {
            ClearArmySelection();
            uiManager.HideArmyPanel();
        }
        else if (selectedArmy == null)
        {
            uiManager.HideArmyPanel();
        }

        MoveHighlight(tile);
    }


    void MoveHighlight(Tile tile)
    {
        if (tile == null) return;

        Vector3 tilePos = new Vector3(tile.GridPosition.x, tile.GridPosition.y, 0);
        MoveHighlight(tilePos);
    }

    void MoveHighlight(Vector3 position)
    {
        highlightInstance.transform.position = position + new Vector3(0, 0, -0.2f);
        highlightInstance.SetActive(true);
    }

    void ClearSelectHighlight() { 
        highlightInstance.SetActive(false);
    }

    public void MoveArmyToTile(Army army, Tile fromTile, Tile toTile)
    {
        if (army == null || fromTile == null || toTile == null)
        {
            UnityEngine.Debug.LogError("[ClickManager] MoveArmyToTile called with null reference.");
            return;
        }

        UnityEngine.Debug.Log($"Moving army from {fromTile.GridPosition} to {toTile.GridPosition}");

        int distance = Mathf.Max(
            Mathf.Abs(toTile.GridPosition.x - fromTile.GridPosition.x),
            Mathf.Abs(toTile.GridPosition.y - fromTile.GridPosition.y));

        if (!army.CanMoveTo(distance))
        {
            UnityEngine.Debug.Log("Not enough movement.");
            return;
        }

        // If there’s an enemy army on the tile, handle battle first
        if (toTile.HasArmy && toTile.Army.Faction != army.Faction)
        {
            UnityEngine.Debug.Log("[ClickManager] Battle triggered between armies!");
            BattleManager.Instance.ResolveBattle(army, toTile.Army, fromTile, toTile);
            return;
        }
        if (toTile.HasArmy && toTile.Army.Faction == army.Faction)
        {
            UnityEngine.Debug.Log("[ClickManager] Friendly army encountered — attempting merge.");

            Army stationaryArmy = toTile.Army;
            Army movingArmy = army;
            List<Unit> unitsToTransfer = new List<Unit>(movingArmy.Units);

            foreach (var unit in unitsToTransfer)
            {
                if (stationaryArmy.Units.Count < Army.MaxUnits)
                {
                    stationaryArmy.Units.Add(unit);
                }
                else
                {
                    break;
                }
            }
            foreach (var unit in unitsToTransfer)
            {
                if (stationaryArmy.Units.Contains(unit))
                    movingArmy.Units.Remove(unit);
            }

            stationaryArmy.MergeUnits();
            if (movingArmy.Units.Count == 0)
            {
                UnityEngine.Debug.Log("[ClickManager] Moving army fully merged and removed.");
                if (fromTile.ArmyObject != null)
                    Destroy(fromTile.ArmyObject);

                fromTile.Army = null;
                fromTile.ArmyObject = null;

                TurnManager.Instance.GetPlayerByName(movingArmy.Faction)?.Armies.Remove(movingArmy);
            }
            else
            {
                UnityEngine.Debug.Log("[ClickManager] Some units could not merge — moving army retains leftovers.");
            }
            stationaryArmy.MergeUnits();
            UIManager.Instance?.RefreshArmyPanel(stationaryArmy);

            return;
        }

        if (fromTile.ArmyObject == null)
        {
            UnityEngine.Debug.LogWarning("[ClickManager] fromTile.ArmyObject is null — likely cleared after battle.");
            return;
        }

        if (toTile.HasSettlement && toTile.Settlement.Faction != army.Faction)
        {
            UnityEngine.Debug.Log("[ClickManager] Army is attacking a settlement!");
            BattleManager.Instance.ResolveSiege(army, toTile.Settlement, fromTile, toTile);
            return;
        }

        if (toTile.HasSettlement && toTile.Settlement.Faction == army.Faction)
        {
            BattleManager.Instance.EnterFriendlySettlement(army, toTile.Settlement, fromTile, toTile);
            return;
        }
        ClearAllHighlights();

        GameObject armyObj = fromTile.ArmyObject;
        toTile.Army = army;
        toTile.ArmyObject = armyObj;
        fromTile.Army = null;
        fromTile.ArmyObject = null;

        ClickableObject clickable = armyObj.GetComponent<ClickableObject>();
        if (clickable != null)
        {
            clickable.linkedTile = toTile;
            UnityEngine.Debug.Log($"Updated ClickableObject linkedTile to {toTile.GridPosition}");
        }
        else
        {
            UnityEngine.Debug.LogError("[ClickManager] Army object has no ClickableObject component!");
        }

        armyObj.transform.position = new Vector3(toTile.GridPosition.x, toTile.GridPosition.y, 0);
        army.SpendMovement(distance);
        MoveHighlight(toTile);

        selectedArmyTile = toTile;
        selectedArmy = army;

        UnityEngine.Debug.Log($"selectedArmyTile updated to: {selectedArmyTile.GridPosition}");

        if (army.RemainingMovement > 0)
        {
            currentMoveRangeTiles = GridManager.Instance.GetTilesInRange(toTile, army.RemainingMovement);
            HighlightMoveRange(currentMoveRangeTiles);
        }
        else
        {
            selectedArmy = null;
            selectedArmyTile = null;
            currentMoveRangeTiles.Clear();
        }
    }
    void HighlightMoveRange(List<Tile> tiles)
    {

        if (selectedArmy.Faction == "Player")
        {

            if (tiles == null) return;

            foreach (Tile tile in tiles)
            {
                if (tile != null && tile.Visual != null)
                {
                    tile.Visual.Highlight(Color.orange);
                }
            }
        }
    }
    void TryMoveToHoveredTile()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            ClickableObject clickable = hit.collider.GetComponent<ClickableObject>();
            if (clickable != null && clickable.linkedTile != null)
            {
                Tile clickedTile = clickable.linkedTile;
                if (selectedArmy != null && selectedArmyTile != null)
                {
                    bool inRange = currentMoveRangeTiles.Contains(clickedTile);
                    bool isEnemyArmy = clickedTile.HasArmy && clickedTile.Army.Faction != selectedArmy.Faction;

                    UnityEngine.Debug.Log($"Clicked tile: {clickedTile.GridPosition}, HasArmy={clickedTile.HasArmy}, " +
                    $"Faction={clickedTile.Army?.Faction}, PlayerArmy={selectedArmy.Faction}, " +
                    $"InRange={inRange}, IsEnemyArmy={isEnemyArmy}");

                    if (inRange || isEnemyArmy)
                    {
                        MoveArmyToTile(selectedArmy, selectedArmyTile, clickedTile);
                    }
                    else
                    {
                        UnityEngine.Debug.Log("[ClickManager] Clicked tile is out of range and has no enemy army.");
                    }
                }
            }
        }
    }
    void ClearAllHighlights()
    {
        if (currentMoveRangeTiles != null)
        {
            foreach (Tile tile in currentMoveRangeTiles)
            {
                if (tile != null && tile.Visual != null)
                {
                    tile.Visual.ResetColor();
                }
            }
            currentMoveRangeTiles.Clear();
        }
        Tile[,] tiles = GridManager.Instance.Tiles;
        if (tiles != null)
        {
            int width = tiles.GetLength(0);
            int height = tiles.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (tiles[x, y] != null && tiles[x, y].Visual != null)
                    {
                        tiles[x, y].Visual.ResetColor();
                    }
                }
            }
        }
    }
    public void ClearArmySelection()
    {
        selectedArmy = null;
        selectedArmyTile = null;
        ClearAllHighlights();
        ClearSelectHighlight();
        uiManager.HideArmyPanel();
    }
    public void TryDisbandUnit(Unit unit)
    {
        if (selectedArmyTile != null && selectedArmy != null && selectedArmy.Units.Contains(unit))
        {
            if (selectedArmy.Faction == "Player")
            {
                bool armyIsEmpty = selectedArmy.RemoveUnit(unit);
                if (armyIsEmpty)
                {
                    selectedArmyTile.Army = null;

                    if (selectedArmyTile.ArmyObject != null)
                    {
                        Destroy(selectedArmyTile.ArmyObject);
                        selectedArmyTile.ArmyObject = null;
                    }

                    TurnManager.Instance.CurrentPlayer.Armies.Remove(selectedArmy);

                    ClearArmySelection();
                    UnityEngine.Debug.Log("[ClickManager] Army disbanded completely.");
                }

                uiManager.RefreshArmyPanel(selectedArmy);
            }
        }
        else if (selectedArmyTile != null && selectedArmyTile.HasSettlement)
        {
            Settlement settlement = selectedArmyTile.Settlement;

            if (settlement.Faction == "Player" && settlement.Garrison.Units.Contains(unit))
            {
                settlement.Garrison.RemoveUnit(unit);
                uiManager.RefreshSettlementPanel(settlement);
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("[ClickManager] Cannot disband unit: not in selected player's army or garrison.");
        }
    }
    public void SelectArmy(Army army, Tile tile)
    {
        if (army == null || tile == null)
        {
            UnityEngine.Debug.LogWarning("[ClickManager] Tried to select null army or tile.");
            selectedArmy = null;
            selectedArmyTile = null;
            return;
        }

        selectedArmy = army;
        selectedArmyTile = tile;

        if (uiManager != null)
        {
            uiManager.ShowArmyPanel(army);
        }

        UnityEngine.Debug.Log($"[ClickManager] Army reselected after battle at {tile.GridPosition}");
    }
}