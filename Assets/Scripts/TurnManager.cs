using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
    public int TurnNumber { get; private set; } = 1;
    public Player CurrentPlayer { get; private set; }
    public List<Player> Players { get; private set; }
    private int currentPlayerIndex = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            UnityEngine.Debug.LogWarning("[TurnManager] Duplicate instance found. Destroying this instance.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        UnityEngine.Debug.Log("[TurnManager] instance set");

        Players = new List<Player>
        {
            new Player("Player", true),
            new Player("AI", false)
        };

        CurrentPlayer = Players[0];
        UnityEngine.Debug.Log("[TurnManager] Current player set to " + CurrentPlayer.Name);
    }

    public void EndTurn()
    {
        UnityEngine.Debug.Log("EndTurn: Processing turn end");

        if (ClickManager.Instance != null)
        {
            ClickManager.Instance.ClearArmySelection();
        }

        if (CurrentPlayer != null)
        {
            CurrentPlayer.EndTurn();
        }
        ProcessAllRecruitmentQueues(CurrentPlayer);

        do
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % Players.Count;
            CurrentPlayer = Players[currentPlayerIndex];
            UnityEngine.Debug.Log($"Switched to player: {CurrentPlayer.Name}");

            if (currentPlayerIndex == 0)
            {
                TurnNumber++;
                UnityEngine.Debug.Log($"Turn number increased to: {TurnNumber}");
            }

            GameManager.Instance.CheckVictoryConditions();
            CurrentPlayer.BeginTurn();

            if (!CurrentPlayer.isHuman)
            {
                CurrentPlayer.EndTurn();
                ProcessAllRecruitmentQueues(CurrentPlayer);
            }

        } while (CurrentPlayer.isHuman == false);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnEndTurn();
            UnityEngine.Debug.Log("UI updated after turn end");
        }
        else
        {
            UnityEngine.Debug.LogError("EndTurn: UIManager.Instance is null!");
        }
    }

    public Player GetPlayerByName(string name)
    {
        if (Players == null)
        {
            UnityEngine.Debug.LogError("[TurnManager] Players list is null!");
            return null;
        }

        foreach (var player in Players)
        {
            if (player.Name == name)
                return player;
        }

        UnityEngine.Debug.LogWarning($"[TurnManager] No player found with name '{name}'");
        return null;
    }

    private void ProcessAllRecruitmentQueues(Player player)
    {
        if (player == null) return;

        foreach (Settlement settlement in player.Settlements)
        {
            if (settlement.RecruitmentQueue != null && settlement.RecruitmentQueue.Count > 0)
            {
                UnityEngine.Debug.Log($"[TurnManager] Processing recruitment queue for {settlement.Name}");
                settlement.ProcessRecruitmentQueue();
            }
        }
    }
}