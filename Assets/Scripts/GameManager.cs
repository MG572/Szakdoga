using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private TurnManager turnManagerPrefab;
    [SerializeField] private UIManager uiManagerPrefab;
    [SerializeField] private GridManager gridManagerPrefab;

    public bool IsInitialized { get; private set; } = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            UnityEngine.Debug.LogWarning("Multiple GameManager instances found. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        UnityEngine.Debug.Log("GameManager initialized");

        InitializeGameSystems();
    }

    private void InitializeGameSystems()
    {
        UnityEngine.Debug.Log("Initializing");
        TurnManager turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager == null && turnManagerPrefab != null)
        {
            UnityEngine.Debug.Log("Creating TurnManager instance");
            turnManager = Instantiate(turnManagerPrefab);
        }

        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager == null && uiManagerPrefab != null)
        {
            UnityEngine.Debug.Log("Creating UIManager instance");
            uiManager = Instantiate(uiManagerPrefab);
        }

        GridManager gridManager = FindFirstObjectByType<GridManager>();
        if (gridManager == null && gridManagerPrefab != null)
        {
            UnityEngine.Debug.Log("Creating GridManager instance");
            gridManager = Instantiate(gridManagerPrefab);
        }

        IsInitialized = true;
        UnityEngine.Debug.Log("Game systems initialized");
    }

    public void CheckVictoryConditions()
    {
        Player player = TurnManager.Instance.GetPlayerByName("Player");
        Player ai = TurnManager.Instance.GetPlayerByName("AI");

        int playerSettlements = player?.Settlements.Count ?? 0;
        int aiSettlements = ai?.Settlements.Count ?? 0;

        if (playerSettlements == 0 && aiSettlements == 0)
        {
            UnityEngine.Debug.LogWarning("[GameManager] Draw – both factions have no settlements!");
            EndGame("Draw");
            return;
        }

        if (playerSettlements == 0)
        {
            UnityEngine.Debug.Log("[GameManager] Player defeated!");
            EndGame("Defeat");
            return;
        }

        if (aiSettlements == 0)
        {
            UnityEngine.Debug.Log("[GameManager] Player victorious!");
            EndGame("Victory");
            return;
        }
    }

    private void EndGame(string result)
    {
        PlayerPrefs.SetString("GameResult", result);
        SceneManager.LoadScene("EndGame");
    }
}