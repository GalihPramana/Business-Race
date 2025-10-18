using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public List<Player> players;
    public int currentPlayerIndex = 0;
    public Transform diceButton;

    [Header("Board Settings")]
    public List<Transform> tiles;

    [Header("Wheel Settings")]
    public GameObject spinWheelUI;
    public RotateWheel spinWheel;

    private bool canRoll = true;
    private bool gameOver = false;

    void Start()
    {
        if (spinWheel != null)
            spinWheel.OnSpinComplete += OnWheelComplete;

        //HandleCurrentTurn();

        // Make sure the spin wheel is hidden at start
        if (spinWheelUI != null)
            spinWheelUI.SetActive(false);
    }

    public void RollDice()
    {
        if (!canRoll || gameOver) return;

        canRoll = false;

        // Show spin wheel
        if (spinWheelUI != null)
            spinWheelUI.SetActive(true);

        // Hide dice button during spin
        if (diceButton != null)
            diceButton.gameObject.SetActive(false);
    }

    private void OnWheelComplete(string difficulty)
    {
        if (spinWheelUI != null)
            spinWheelUI.SetActive(false);

        // Jika Lucky  langsung gerak
        if (difficulty == "Lucky")
        {
            int steps = GetStepsFromDifficulty(difficulty);
            Debug.Log($"Lucky spin! Player {currentPlayerIndex + 1} moves {steps} steps automatically!");
            StartCoroutine(MovePlayer(players[currentPlayerIndex], steps));
            return;
        }

        // Jika bukan Lucky tunggu quiz
        if (spinWheel.quizPopup != null)
        {
            spinWheel.quizPopup.OnQuizFinished = (correct) =>
            {
                Player currentPlayer = players[currentPlayerIndex];

                if (correct)
                {
                    int steps = GetStepsFromDifficulty(difficulty);

                    // === COIN REWARD BASED ON DIFFICULTY ===
                    int coinReward = 0;
                    switch (difficulty)
                    {
                        case "Easy": coinReward = 25; break;
                        case "Normal": coinReward = 50; break;
                        case "Hard": coinReward = 75; break;
                    }

                    currentPlayer.coin += coinReward;

                    Debug.Log($"{currentPlayer.playerName} answered correctly! Gained {coinReward} coins. Total coins: {currentPlayer.coin}");
                    Debug.Log($"Player {currentPlayerIndex + 1} answered correctly! Moving {steps} steps.");
                    StartCoroutine(MovePlayer(currentPlayer, steps));
                }
                else
                {
                    Debug.Log($"{players[currentPlayerIndex].playerName} answered wrong. No movement and no coins.");
                    NextTurn();
                }
            };
        }
    }


    private int GetStepsFromDifficulty(string difficulty)
    {
        switch (difficulty)
        {
            case "Easy": return Random.Range(1, 4);   // 1–3
            case "Normal": return Random.Range(4, 7); // 4–6
            case "Hard": return Random.Range(7, 10);  // 7–9
            case "Lucky": return 6;
            default: return 3;
        }
    }

    private IEnumerator MovePlayer(Player player, int steps)
    {
        PlayerTileMover mover = player.pawn.GetComponent<PlayerTileMover>();

        // Simplified movement logic
        if (player.currentHomeTileIndex != -1)
        {
            yield return MoveOnHomePath(player, mover, steps);
        }
        else
        {
            yield return MoveOnMainPath(player, mover, steps);
        }

        // End turn after movement
        NextTurn();
    }

    private IEnumerator MoveOnMainPath(Player player, PlayerTileMover mover, int steps)
    {
        int originalIndex = player.currentTileIndex;
        int tilesMoved = 0;
        int currentTileIndex = originalIndex;

        // Starting a new game (pawn not on board yet)
        if (originalIndex == -1)
        {
            if (steps == 6)
            {
                player.currentTileIndex = player.baseTileIndex;
                yield return mover.MoveToTile(tiles[player.currentTileIndex]);
            }
            else
            {
                NextTurn();
            }
            yield break;
        }

        // Move along main path
        while (tilesMoved < steps)
        {
            int nextTileIndex = (currentTileIndex + 1) % tiles.Count;

            // Check if player is reaching home entrance
            if (nextTileIndex == player.baseTileIndex)
            {
                int remainingSteps = steps - tilesMoved;
                player.currentHomeTileIndex = -1;
                yield return MoveOnHomePath(player, mover, remainingSteps);
                yield break;
            }

            // Skip enemy base tiles
            if (IsBaseTile(nextTileIndex))
            {
                Player tileOwner = GetPlayerByBaseTileIndex(nextTileIndex);
                if (tileOwner != null && tileOwner != player)
                {
                    // Move visually but don’t count the step
                    player.currentTileIndex = nextTileIndex;
                    yield return mover.MoveToTile(tiles[player.currentTileIndex]);
                    currentTileIndex = player.currentTileIndex;
                    continue;
                }
            }

            // Normal movement
            player.currentTileIndex = nextTileIndex;
            yield return mover.MoveToTile(tiles[player.currentTileIndex]);
            tilesMoved++;
            currentTileIndex = player.currentTileIndex;
        }
    }

    private IEnumerator MoveOnHomePath(Player player, PlayerTileMover mover, int steps)
    {
        int originalHomeIndex = player.currentHomeTileIndex;
        int targetHomeIndex = originalHomeIndex + steps;

        if (targetHomeIndex >= player.homeTiles.Count)
        {
            Debug.Log(player.playerName + " needs to roll a " + (player.homeTiles.Count - 1 - originalHomeIndex) + " or less to win!");
            NextTurn();
            yield break;
        }

        for (int i = 0; i < steps; i++)
        {
            int nextHomeTileIndex = originalHomeIndex + i + 1;
            player.currentHomeTileIndex = nextHomeTileIndex;
            yield return mover.MoveToTile(player.homeTiles[player.currentHomeTileIndex]);
        }

        if (player.currentHomeTileIndex == player.homeTiles.Count - 1)
        {
            WinGame(player);
        }
    }

    public void NextTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        canRoll = true;
        HandleCurrentTurn();
    }

    void HandleCurrentTurn()
    {
        Player currentPlayer = players[currentPlayerIndex];

        if (currentPlayer.isComputer)
        {
            if (diceButton != null)
                diceButton.gameObject.SetActive(false);

            Debug.Log("Computer's turn...");
            StartCoroutine(ComputerTurn());
        }
        else
        {
            if (diceButton != null)
                diceButton.gameObject.SetActive(true);

            Debug.Log("Human Player's turn...");
        }
    }

    IEnumerator ComputerTurn()
    {
        yield return new WaitForSeconds(1.5f);
        RollDice();
    }

    private void WinGame(Player winningPlayer)
    {
        gameOver = true;
        if (diceButton != null)
            diceButton.gameObject.SetActive(false);
        Debug.Log(winningPlayer.playerName + " has won the game!");
    }

    private bool IsBaseTile(int index)
    {
        foreach (var player in players)
        {
            if (player.baseTileIndex == index)
                return true;
        }
        return false;
    }

    private Player GetPlayerByBaseTileIndex(int index)
    {
        foreach (var player in players)
        {
            if (player.baseTileIndex == index)
                return player;
        }
        return null;
    }
}
