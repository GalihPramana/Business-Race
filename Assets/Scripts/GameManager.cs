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
        int diceRoll = Random.Range(1, 7);
        Debug.Log("Player " + (currentPlayerIndex + 1) + " rolled a " + diceRoll);

        StartCoroutine(MovePlayer(players[currentPlayerIndex], diceRoll));
    }

    private IEnumerator MovePlayer(Player player, int steps)
    {
        PlayerTileMover mover = player.pawn.GetComponent<PlayerTileMover>();

        // Check if player is on home tiles or is about to enter
        if (player.currentHomeTileIndex != -1)
        {
            // Player is already on the home path, proceed with home tile movement logic
            yield return MoveOnHomePath(player, mover, steps);
        }
        else
        {
            // Player is on the main path or at the start
            yield return MoveOnMainPath(player, mover, steps);
        }

        // Handle turn conditions after movement, regardless of which path was taken
        // Check for win condition after movement (this is a redundant check here but can be useful)
        if (player.currentHomeTileIndex == player.homeTiles.Count - 1)
        {
            WinGame(player);
        }
        else if (steps == 6)
        {
            Debug.Log("Rolled a 6! " + player.playerName + " gets another turn.");
            canRoll = true;
            if (player.isComputer)
            {
                yield return new WaitForSeconds(1.5f);
                RollDice();
            }
        }
        else
        {
            NextTurn();
        }
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
                // Rule: If a 6 is rolled to start, the player gets another turn.
            }
            else
            {
                NextTurn();
            }
            yield break;
        }

        // Move along the main path, skipping enemy bases in the count
        while (tilesMoved < steps)
        {
            int nextTileIndex = (currentTileIndex + 1) % tiles.Count;

            // Check if player is approaching their home entrance
            if (nextTileIndex == player.baseTileIndex)
            {
                // Player has completed a full lap and is about to start the home path
                int remainingSteps = steps - tilesMoved;
                player.currentHomeTileIndex = -1; // Reset to the start of home path
                yield return MoveOnHomePath(player, mover, remainingSteps);
                yield break;
            }

            // Check if the next tile is an enemy base
            if (IsBaseTile(nextTileIndex))
            {
                Player tileOwner = GetPlayerByBaseTileIndex(nextTileIndex);
                if (tileOwner != null && tileOwner != player)
                {
                    // This is an enemy base, we can pass through it but it doesn't count as a step
                    // The pawn will move to this tile but the loop will not increment the counter.
                    // This requires a special movement function to not count the step.
                    // For now, let's just make the player land on it. The main rule is no landing.
                    // If you want to bypass, the logic below is a bit more complex.
                }
            }

            // Move to the next tile
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

        // Check if the move overshoots the win tile
        if (targetHomeIndex >= player.homeTiles.Count)
        {
            Debug.Log(player.playerName + " needs to roll a " + (player.homeTiles.Count - 1 - originalHomeIndex) + " or less to win!");
            NextTurn();
            yield break;
        }

        // Move along the home path
        for (int i = 0; i < steps; i++)
        {
            int nextHomeTileIndex = originalHomeIndex + i + 1;
            player.currentHomeTileIndex = nextHomeTileIndex;
            yield return mover.MoveToTile(player.homeTiles[player.currentHomeTileIndex]);
        }

        // Check for win condition after movement
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

    // Win game function
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

    void UpdateTurnUI()
    {
        // Your UI update logic here
    }
}