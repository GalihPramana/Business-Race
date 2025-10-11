using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public List<Player> players;
    public int currentPlayerIndex = 0;
    public Transform diceButton; // Assign the dice roll button here

    [Header("Board Settings")]
    public List<Transform> tiles;

    private bool canRoll = true;

    void Start()
    {
        // Example setup for a 3-player human, 1-computer game
        UpdateTurnUI();
    }

    public void RollDice()
    {
        if (!canRoll) return;

        canRoll = false;
        int diceRoll = Random.Range(1, 7);
        Debug.Log("Player " + (currentPlayerIndex + 1) + " rolled a " + diceRoll);

        // Move the player based on the roll
        StartCoroutine(MovePlayer(players[currentPlayerIndex], diceRoll));
    }

    private IEnumerator MovePlayer(Player player, int steps)
    {
        PlayerTileMover mover = player.pawn.GetComponent<PlayerTileMover>();
        int originalIndex = player.currentTileIndex;
        int targetIndex = originalIndex + steps;

        for (int i = 0; i < steps; i++)
        {
            int nextTileIndex = originalIndex + i + 1;

            // Check if the next tile is a base tile
            if (IsBaseTile(nextTileIndex))
            {
                Player tileOwner = GetPlayerByBaseTileIndex(nextTileIndex);

                // If the tile belongs to another player and is not a destination, skip it
                if (tileOwner != null && tileOwner != player && nextTileIndex != targetIndex)
                {
                    // Move through the tile without stopping
                    if (nextTileIndex < tiles.Count)
                    {
                        player.currentTileIndex = nextTileIndex;
                        yield return mover.MoveToTile(tiles[nextTileIndex]);
                    }
                    continue; // Skip the rest of the loop for this step
                }
            }

            // Normal movement logic
            if (nextTileIndex < tiles.Count)
            {
                player.currentTileIndex = nextTileIndex;
                yield return mover.MoveToTile(tiles[nextTileIndex]);
            }
        }

        // After moving, check if the player rolled a 6
        if (steps == 6)
        {
            Debug.Log("Rolled a 6! Player " + (currentPlayerIndex + 1) + " gets another turn.");
            canRoll = true;
        }
        else
        {
            NextTurn();
        }
    }

    // Helper methods to make the code cleaner
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

    public void NextTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        canRoll = true;
        UpdateTurnUI();
        HandleCurrentTurn();
    }

    void HandleCurrentTurn()
    {
        Player currentPlayer = players[currentPlayerIndex];

        if (currentPlayer.isComputer)
        {
            diceButton.gameObject.SetActive(false); // Hide the button for computer
            // Computer's turn
            Debug.Log("Computer's turn...");
            StartCoroutine(ComputerTurn());
        }
        else
        {
            diceButton.gameObject.SetActive(true); // Show the button for human
            // Wait for human player to roll
            Debug.Log("Human Player's turn...");
        }
    }

    IEnumerator ComputerTurn()
    {
        yield return new WaitForSeconds(1.5f); // A slight delay to make it feel natural
        RollDice();
    }

    // You can call this method from a setup screen
    public void SetupPlayers(int humanPlayers, int computerPlayers)
    {
        players.Clear();
        // Instantiate and add human players
        for (int i = 0; i < humanPlayers; i++)
        {
            // Example of adding a new player object
            Player newPlayer = new Player();
            newPlayer.playerName = "Human " + (i + 1);
            newPlayer.playerID = i;
            // You'll need to link this to a pawn in your scene
            // newPlayer.pawn = ...
            players.Add(newPlayer);
        }

        // Instantiate and add computer players
        for (int i = 0; i < computerPlayers; i++)
        {
            Player newPlayer = new Player();
            newPlayer.playerName = "Computer " + (i + 1);
            newPlayer.playerID = humanPlayers + i;
            newPlayer.isComputer = true;
            // You'll need to link this to a pawn in your scene
            // newPlayer.pawn = ...
            players.Add(newPlayer);
        }

        // After setting up, you can shuffle or arrange based on game rules
    }

    void UpdateTurnUI()
    {
        // Update a UI Text element to show whose turn it is
        // For example: `turnText.text = players[currentPlayerIndex].playerName + "'s Turn";`
    }
}