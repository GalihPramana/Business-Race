using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [Header("Turn Choice UI")]
    public GameObject choicePanel;   // panel berisi 2 tombol: SpinWheel & Shop
    public Button spinWheelButton;   // tombol untuk Spin Wheel
    public Button shopButton;        // tombol untuk buka Shop
    public GameObject shopPanel;     // panel Shop milikmu (GameObject "Shop")

    public TMP_Text coinDisplayInShop;    

    private bool canRoll = true;
    private bool gameOver = false;

    public static GameManager Instance;
    private bool waitingForPawnSelection = false;

    private Dictionary<string, int> itemPrices = new Dictionary<string, int>()
    {
        {"Bom", 200},
        {"Iceball", 175},
        {"TimeReverse-3", 100},
        {"TimeReverse-5", 125},
        {"TimeReverse-7", 150}
    };

    void Start()
    {
        Instance = this;

        if (spinWheel != null)
            spinWheel.OnSpinComplete += OnWheelComplete;

        if (spinWheelUI != null)
            spinWheelUI.SetActive(false);

        if (choicePanel != null)
            choicePanel.SetActive(false);

        if (shopPanel != null)
            shopPanel.SetActive(false);

        // Initialize PawnSelector2D for all pawns
        foreach (Player player in players)
        {
            for (int i = 0; i < player.pawns.Count; i++)
            {
                Transform pawn = player.pawns[i];

                if (pawn == null)
                {
                    Debug.LogError(player.playerName + " has a null pawn at index " + i + ". Check the Player Inspector.");
                    continue;
                }

                PawnSelector2D selector = pawn.GetComponent<PawnSelector2D>();
                if (selector != null)
                {
                    selector.Initialize(player, i);
                    Debug.Log("Initialized " + player.playerName + "'s pawn " + (i + 1) + ": " + pawn.name);
                }
                else
                {
                    Debug.LogWarning(pawn.name + " has no PawnSelector2D component!");
                }
            }
        }

        HandleCurrentTurn();
    }

    public void UpdateCoinDisplay()
    {
        Player currentPlayer = players[currentPlayerIndex];
        string coinText = currentPlayer.coin.ToString();

        if (coinDisplayInShop != null)
        {
            coinDisplayInShop.text = coinText;
        }
    }

    // === SISTEM ITEM SHOP ===
    public void BuyItem(string itemName)
    {
        Player currentPlayer = players[currentPlayerIndex];

        if (!itemPrices.ContainsKey(itemName))
        {
            Debug.LogWarning("Item tidak dikenal: " + itemName);
            return;
        }

        int itemCost = itemPrices[itemName];

        if (currentPlayer.coin < itemCost)
        {
            Debug.LogWarning($"{currentPlayer.playerName} tidak punya cukup coin untuk membeli {itemName}");
            return;
        }

        currentPlayer.coin -= itemCost;
        UpdateCoinDisplay();

        Debug.Log($"{currentPlayer.playerName} membeli item {itemName} seharga {itemCost}. Sisa coin: {currentPlayer.coin}");
    }

    public void OnPawnClicked(Player player, int pawnIndex)
    {
        if (!waitingForPawnSelection) return;
        Player currentPlayer = players[currentPlayerIndex];
        if (player != currentPlayer) return;

        currentPlayer.activePawnIndex = pawnIndex;
        waitingForPawnSelection = false;

        Debug.Log(player.playerName + " selected pawn " + (pawnIndex + 1));
        HighlightPawn(player.ActivePawn, true);

        // Disable pawn colliders
        foreach (var pawn in currentPlayer.pawns)
        {
            Collider2D col = pawn.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        // Show and set up the choice UI
        SetupTurnChoiceUI();
    }


    private void HighlightPawn(Transform pawn, bool active)
    {
        Renderer rend = pawn.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = active ? Color.yellow : Color.white;
        }
    }


    // === PHASE 1: Player chooses between Shop or Spin Wheel ===
    void HandleCurrentTurn()
    {
        Player currentPlayer = players[currentPlayerIndex];

        // Reset UI
        if (choicePanel != null) choicePanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (spinWheelUI != null) spinWheelUI.SetActive(false);

        if (currentPlayer.isComputer)
        {
            StartCoroutine(ComputerTurn());
            return;
        }

        Debug.Log(currentPlayer.playerName + "'s turn! Click a pawn to select.");

        waitingForPawnSelection = true;

        // Re-enable pawn colliders
        foreach (var pawn in currentPlayer.pawns)
        {
            Collider2D col = pawn.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;
            HighlightPawn(pawn, false);
        }
    }

    private void SetupTurnChoiceUI()
    {
        if (choicePanel == null) return;

        choicePanel.SetActive(true);

        // Clear previous listeners
        spinWheelButton.onClick.RemoveAllListeners();
        shopButton.onClick.RemoveAllListeners();

        // Spin Wheel button
        spinWheelButton.onClick.AddListener(() =>
        {
            Debug.Log("Player chose Spin Wheel");
            choicePanel.SetActive(false);
            RollDice();
        });

        // Shop button
        shopButton.onClick.AddListener(() =>
        {
            Debug.Log("Player chose Shop");
            choicePanel.SetActive(false);
            OpenShop();
        });
    }



    void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            UpdateCoinDisplay();
        }
    }

    // ini dipanggil dari tombol "Close" di panel Shop
    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        Debug.Log("Shop ditutup, lanjut ke Spin Wheel");
        NextTurn();
    }

    // === PHASE 2: Spin wheel logic ===
    public void RollDice()
    {
        if (!canRoll || gameOver) return;

        canRoll = false;

        if (spinWheelUI != null)
            spinWheelUI.SetActive(true);

        if (diceButton != null)
            diceButton.gameObject.SetActive(false);
    }

    private void OnWheelComplete(string difficulty)
    {
        if (spinWheelUI != null)
            spinWheelUI.SetActive(false);

        if (difficulty == "Lucky")
        {
            int steps = GetStepsFromDifficulty(difficulty);
            Debug.Log($"Lucky spin! Player {currentPlayerIndex + 1} moves {steps} steps automatically!");
            StartCoroutine(MovePlayer(players[currentPlayerIndex], steps));
            return;
        }

        if (spinWheel.quizPopup != null)
        {
            spinWheel.quizPopup.OnQuizFinished = (correct) =>
            {
                Player currentPlayer = players[currentPlayerIndex];

                if (correct)
                {
                    int steps = GetStepsFromDifficulty(difficulty);
                    int coinReward = 0;

                    switch (difficulty)
                    {
                        case "Easy": coinReward = 25; break;
                        case "Normal": coinReward = 50; break;
                        case "Hard": coinReward = 75; break;
                    }

                    currentPlayer.coin += coinReward;
                    Debug.Log($"{currentPlayer.playerName} benar! Dapat {coinReward} coin. Total: {currentPlayer.coin}");
                    StartCoroutine(MovePlayer(currentPlayer, steps));
                }
                else
                {
                    Debug.Log($"{currentPlayer.playerName} salah. Tidak bergerak & tidak dapat coin.");
                    NextTurn();
                }
            };
        }
    }

    private int GetStepsFromDifficulty(string difficulty)
    {
        switch (difficulty)
        {
            case "Easy": return Random.Range(1, 4);
            case "Normal": return Random.Range(4, 7);
            case "Hard": return Random.Range(7, 10);
            case "Lucky": return 6;
            default: return 3;
        }
    }

    private IEnumerator MovePlayer(Player player, int steps)
    {
        if (player == null)
        {
            Debug.LogError("MovePlayer called with null player!");
            yield break;
        }

        Transform activePawn = player.ActivePawn;

        if (activePawn == null)
        {
            Debug.LogError($"{player.playerName} has no ActivePawn selected! Did you forget to click a pawn?");
            yield break;
        }

        PlayerTileMover mover = activePawn.GetComponent<PlayerTileMover>();
        if (mover == null)
        {
            Debug.LogError($"Active pawn '{activePawn.name}' has no PlayerTileMover component!");
            yield break;
        }

        if (player.currentHomeTileIndex != -1)
        {
            yield return MoveOnHomePath(player, mover, steps);
        }
        else
        {
            yield return MoveOnMainPath(player, mover, steps);
        }

        NextTurn();
    }


    private IEnumerator MoveOnMainPath(Player player, PlayerTileMover mover, int steps)
    {
        int originalIndex = player.currentTileIndex;
        int tilesMoved = 0;
        int currentTileIndex = originalIndex;

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

        while (tilesMoved < steps)
        {
            int nextTileIndex = (currentTileIndex + 1) % tiles.Count;

            if (nextTileIndex == player.baseTileIndex)
            {
                int remainingSteps = steps - tilesMoved;
                player.currentHomeTileIndex = -1;
                yield return MoveOnHomePath(player, mover, remainingSteps);
                yield break;
            }

            if (IsBaseTile(nextTileIndex))
            {
                Player tileOwner = GetPlayerByBaseTileIndex(nextTileIndex);
                if (tileOwner != null && tileOwner != player)
                {
                    player.currentTileIndex = nextTileIndex;
                    yield return mover.MoveToTile(tiles[player.currentTileIndex]);
                    currentTileIndex = player.currentTileIndex;
                    continue;
                }
            }

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
        UpdateCoinDisplay();
    }

    IEnumerator ComputerTurn()
    {
        yield return new WaitForSeconds(1.0f);

        Player aiPlayer = players[currentPlayerIndex];

        // Hide all player UI completely
        if (choicePanel != null) choicePanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (spinWheelUI != null) spinWheelUI.SetActive(false);

        // Also hide quiz popup if somehow open
        if (spinWheel != null && spinWheel.quizPopup != null)
            spinWheel.quizPopup.quizPanel.SetActive(false);

        Debug.Log("Computer's turn started...");

        // 1. Randomly select a pawn
        if (aiPlayer.pawns.Count > 0)
        {
            int randomPawnIndex = Random.Range(0, aiPlayer.pawns.Count);
            aiPlayer.activePawnIndex = randomPawnIndex;

            Transform chosenPawn = aiPlayer.ActivePawn;
            Debug.Log("Computer selected pawn " + (randomPawnIndex + 1) + " (" + chosenPawn.name + ")");

            // Optional: highlight the chosen pawn
            HighlightPawn(chosenPawn, true);
        }
        else
        {
            Debug.LogWarning("AI player has no pawns assigned!");
            yield break;
        }

        yield return new WaitForSeconds(0.5f); // small pause before spin

        // 2. Simulate spin result (no UI shown)
        string[] difficulties = { "Easy", "Normal", "Hard", "Lucky" };
        string chosenDifficulty = difficulties[Random.Range(0, difficulties.Length)];
        Debug.Log("Computer spun: " + chosenDifficulty);

        // 3. Simulate quiz result internally (no popup)
        bool correct = Random.value < 0.7f; // 70% chance success
        int steps = GetStepsFromDifficulty(chosenDifficulty);
        int coinReward = 0;

        switch (chosenDifficulty)
        {
            case "Easy": coinReward = 25; break;
            case "Normal": coinReward = 50; break;
            case "Hard": coinReward = 75; break;
            case "Lucky": coinReward = 100; break;
        }

        // 4. Apply results and move or fail
        if (chosenDifficulty == "Lucky" || correct)
        {
            aiPlayer.coin += coinReward;
            Debug.Log("Computer answered correctly (or got Lucky)! Moves " + steps + " steps, earns " + coinReward + " coins.");
            yield return MovePlayer(aiPlayer, steps);
        }
        else
        {
            Debug.Log("Computer failed the '" + chosenDifficulty + "' quiz! No movement.");
            yield return new WaitForSeconds(1f);
            NextTurn();
        }
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
