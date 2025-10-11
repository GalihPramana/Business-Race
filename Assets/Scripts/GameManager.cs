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

    private bool canRoll = true;
    private bool gameOver = false;

    void Start()
    {
        HandleCurrentTurn();
    }

    public void RollDice()
    {
        if (!canRoll || gameOver) return;

        canRoll = false;

        // Use the new SpinWheel method instead of a dice roll
        int wheelResult = SpinWheel();
        Debug.Log("Player " + (currentPlayerIndex + 1) + " spun the wheel and got " + wheelResult + " steps.");

        // For now, move the player directly. Trivia questions will be added later.
        StartCoroutine(MovePlayer(players[currentPlayerIndex], wheelResult));
    }

    private int SpinWheel()
    {
        int spinResult = Random.Range(1, 101); // 1-100 to work with percentages

        if (spinResult <= 45) // Easy (45%)
        {
            return Random.Range(1, 4); // 1-3 steps
        }
        else if (spinResult <= 80) // Normal (45% + 35% = 80%)
        {
            return Random.Range(4, 7); // 4-6 steps
        }
        else if (spinResult <= 95) // Hard (80% + 15% = 95%)
        {
            return Random.Range(7, 10); // 7-9 steps
        }
        else // Lucky (5%)
        {
            // Lucky grants a specific number of steps without a question
            return 6;
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

        // The turn ends after a move, no extra turns for a 6
        NextTurn();
    }

    private IEnumerator MoveOnMainPath(Player player, PlayerTileMover mover, int steps)
    {
        int originalIndex = player.currentTileIndex;
        int tilesMoved = 0;
        int currentTileIndex = originalIndex;

        // Check for starting a new game (pawn at -1 index)
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

        // Move along the main path, skipping enemy bases
        while (tilesMoved < steps)
        {
            int nextTileIndex = (currentTileIndex + 1) % tiles.Count;

            // Check if player is approaching their home entrance
            if (nextTileIndex == player.baseTileIndex)
            {
                int remainingSteps = steps - tilesMoved;
                player.currentHomeTileIndex = -1;
                yield return MoveOnHomePath(player, mover, remainingSteps);
                yield break;
            }

            // Check if the next tile is an enemy base
            if (IsBaseTile(nextTileIndex))
            {
                Player tileOwner = GetPlayerByBaseTileIndex(nextTileIndex);
                if (tileOwner != null && tileOwner != player)
                {
                    // Move visually but don't count the step
                    player.currentTileIndex = nextTileIndex;
                    yield return mover.MoveToTile(tiles[player.currentTileIndex]);
                    currentTileIndex = player.currentTileIndex;
                    continue; // Continue to the next iteration without counting a step
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
        //UpdateTurnUI();

        if (currentPlayer.isComputer)
        {
            if (diceButton != null)
            {
                diceButton.gameObject.SetActive(false);
            }
            Debug.Log("Computer's turn...");
            StartCoroutine(ComputerTurn());
        }
        else
        {
            if (diceButton != null)
            {
                diceButton.gameObject.SetActive(true);
            }
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
        if (diceButton != null) diceButton.gameObject.SetActive(false);
        Debug.Log(winningPlayer.playerName + " has won the game!");
    }

    private bool IsBaseTile(int index)
    {
        foreach (var player in players)
        {
            if (player.baseTileIndex == index)
            {
                return true;
            }
        }
        return false;
    }

    private Player GetPlayerByBaseTileIndex(int index)
    {
        foreach (var player in players)
        {
            if (player.baseTileIndex == index)
            {
                return player;
            }
        }
        return null;
    }
}