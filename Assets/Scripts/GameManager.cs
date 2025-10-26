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

        // Disable pawn colliders
        foreach (var pawn in currentPlayer.pawns)
        {
            Collider2D col = pawn.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        // Show and set up the choice UI
        SetupTurnChoiceUI();
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

        Player currentPlayer = players[currentPlayerIndex];

        if (difficulty == "Lucky")
        {
            int steps = GetStepsFromDifficulty(difficulty);
            Debug.Log($"Lucky spin! Player {currentPlayer.playerName} moves {steps} steps automatically!");

            // Tambahkan validasi exact roll juga di Lucky spin
            if (IsExactRollValid(currentPlayer, steps))
            {
                StartCoroutine(MovePlayer(currentPlayer, steps));
            }
            else
            {
                Debug.LogWarning($"{currentPlayer.playerName} tidak bisa bergerak karena hasil Lucky tidak tepat untuk mencapai base!");
                NextTurn();
            }

            return;
        }

        if (spinWheel.quizPopup != null)
        {
            spinWheel.quizPopup.OnQuizFinished = (correct) =>
            {
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
                    Debug.Log($"{currentPlayer.playerName} mendapat {steps} langkah!");

                    if (IsExactRollValid(currentPlayer, steps))
                    {
                        StartCoroutine(MovePlayer(currentPlayer, steps));
                    }
                    else
                    {
                        Debug.LogWarning($"{currentPlayer.playerName} tidak bisa bergerak karena hasil spin tidak tepat untuk mencapai base!");
                        NextTurn();
                    }
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
            yield break;
        }

        PawnTracker tracker = activePawn.GetComponent<PawnTracker>();
        if (tracker == null)
        {
            Debug.LogError($"Active pawn '{activePawn.name}' missing PawnTracker component!");
            yield break;
        }

        PlayerTileMover mover = activePawn.GetComponent<PlayerTileMover>();
        if (mover == null)
        {
            yield break;
        }

        if (tracker.currentHomeTileIndex != -1)
        {
            yield return MoveOnHomePath(player, mover, steps, tracker);
        }
        else
        {
            yield return MoveOnMainPath(player, mover, steps, tracker);
        }

        NextTurn();
    }


    private IEnumerator MoveOnMainPath(Player player, PlayerTileMover mover, int steps, PawnTracker tracker)
    {
        int originalIndex = tracker.currentTileIndex;

        // === Pre-check for exact-roll when this move would enter home path ===
        // If this roll would reach the base entry tile and then overshoot inside home, cancel the entire move.
        if (player.homeTiles != null && player.homeTiles.Count > 0)
        {
            int stepsToBaseEntry = StepsToReachBaseEntry(player, tracker);
            int stepToBaseEntry = stepsToBaseEntry - 1;
            if (steps >= stepsToBaseEntry)
            {
                int remainingInsideHome = steps - stepsToBaseEntry;
                if (remainingInsideHome > player.homeTiles.Count)
                {
                    int neededExact = stepsToBaseEntry + player.homeTiles.Count;
                    Debug.LogWarning($"Aturan Exact Roll: {player.playerName} butuh tepat {neededExact} langkah untuk finish, tetapi mendapat {steps}. Tidak bisa bergerak.");
                    yield break; // Do not move at all this turn
                }
            }
        }

        int tilesMoved = 0;
        int currentTileIndex = originalIndex;

        // Kalau pawn masih di luar papan (belum keluar base)
        if (originalIndex == -1)
        {
            // Langsung keluarkan pawn ke tile awal milik player (base entry di main path)
            tracker.currentTileIndex = player.baseTileIndex;
            yield return mover.MoveToTile(tiles[tracker.currentTileIndex]);
            currentTileIndex = tracker.currentTileIndex;

            // Lanjutkan sisa langkah (steps - 1) karena sudah keluar 1 tile
            steps -= 1;
        }

        while (tilesMoved < steps)
        {
            int nextTileIndex = (currentTileIndex + 1) % tiles.Count;

            // --- Masuk ke jalur home (base) ---
            if (nextTileIndex == player.baseTileIndex)
            {
                int remainingSteps = steps - tilesMoved - 1; // -1 untuk langkah ke base entry

                // Kalau tidak punya home path, lanjut jalan biasa
                if (player.homeTiles == null || player.homeTiles.Count == 0)
                    break;

                // Validasi exact roll masih diperlukan di sini (double-safety)
                if (remainingSteps > player.homeTiles.Count)
                {
                    int stepsNeeded = player.homeTiles.Count;
                    Debug.LogWarning($"Aturan Exact Roll: {player.playerName} butuh tepat {stepsNeeded} langkah di jalur home, tetapi mendapat {remainingSteps}. Tidak bisa bergerak.");
                    yield break;
                }

                // 1) Pindah ke base entry (konsumsi 1 langkah)
                tracker.currentTileIndex = nextTileIndex;
                yield return mover.MoveToTile(tiles[tracker.currentTileIndex]);
                tilesMoved++;
                currentTileIndex = tracker.currentTileIndex;

                // 2) Mulai jalur home menggunakan sisa langkah
                tracker.currentHomeTileIndex = -1;
                yield return MoveOnHomePath(player, mover, remainingSteps, tracker);
                yield break;
            }

            // Cek apakah tile berikut adalah base milik lawan
            Player ownerOfBase = GetPlayerByBaseTileIndex(nextTileIndex);
            if (ownerOfBase != null && ownerOfBase != player)
            {
                Debug.LogWarning($"{player.playerName} tidak bisa berhenti di base milik {ownerOfBase.playerName}. Melewati tile tersebut.");
                nextTileIndex = (nextTileIndex + 1) % tiles.Count; // skip tile lawan
            }

            // Pindahkan ke tile berikut yang valid
            tracker.currentTileIndex = nextTileIndex;
            yield return mover.MoveToTile(tiles[tracker.currentTileIndex]);
            tilesMoved++;
            currentTileIndex = tracker.currentTileIndex;
        }
    }

    private IEnumerator MoveOnHomePath(Player player, PlayerTileMover mover, int steps, PawnTracker tracker)
    {
        int originalHomeIndex = tracker.currentHomeTileIndex;
        int targetHomeIndex = originalHomeIndex + steps;

        // --- LOGIKA TEPAT SASARAN (EXACT ROLL) ---
        int finalIndex = player.homeTiles.Count - 1;

        if (targetHomeIndex > finalIndex)
        {
            int stepsNeeded = finalIndex - originalHomeIndex;
            // If originalHomeIndex is -1, stepsNeeded will be player.homeTiles.Count (correct)
            Debug.LogWarning($"Aturan Exact Roll: {player.playerName} butuh tepat {stepsNeeded} langkah untuk menang, tetapi mendapat {steps}. Tidak bisa bergerak.");

            // Do NOT call NextTurn() here — MovePlayer will call NextTurn() once after the coroutine returns.
            yield break;
        }

        // Jika langkah cukup atau tepat sasaran
        for (int i = 0; i < steps; i++)
        {
            int nextHomeTileIndex = originalHomeIndex + i + 1;
            tracker.currentHomeTileIndex = nextHomeTileIndex;
            yield return mover.MoveToTile(player.homeTiles[tracker.currentHomeTileIndex]);
        }

        // Cek Kemenangan
        if (tracker.currentHomeTileIndex == finalIndex)
        {
            if (AllPawnsInBase(player))
            {
                WinGame(player);
            }
            else
            {
                Debug.Log($"{player.playerName} berhasil membawa 1 pawn ke base. Masih ada pawn lain di luar.");
            }
        }
    }

    private bool AllPawnsInBase(Player player)
    {
        foreach (Transform pawn in player.pawns)
        {
            PawnTracker tracker = pawn.GetComponent<PawnTracker>();
            if (tracker == null) continue;

            // Jika belum mencapai tile terakhir di home path, berarti belum finish
            if (tracker.currentHomeTileIndex != player.homeTiles.Count - 1)
            {
                return false;
            }
        }
        return true;
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
            case "Lucky": coinReward = 0; break;
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

    private bool IsExactRollValid(Player player, int steps)
    {
        if (player == null || steps <= 0) return false;

        Transform activePawn = player.ActivePawn;
        if (activePawn == null) return false;

        PawnTracker tracker = activePawn.GetComponent<PawnTracker>();
        if (tracker == null) return false;

        // If player has no home tiles, no exact rule needed.
        if (player.homeTiles == null || player.homeTiles.Count == 0)
            return true;

        // Case 1: Pawn already on home path -> must be exact to finish.
        if (tracker.currentHomeTileIndex != -1)
        {
            int stepsNeeded = (player.homeTiles.Count - 1) - tracker.currentHomeTileIndex;
            return steps == stepsNeeded;
        }

        // Case 2: Pawn on main path.
        // If this move would enter home path, ensure it doesn't overshoot the final home tile.
        int stepsToBaseEntry = StepsToReachBaseEntry(player, tracker);

        // If we won't reach the base entry crossing this turn, move is legal.
        if (steps < stepsToBaseEntry)
            return true;

        // Otherwise, check remaining steps inside home path.
        int remainingInsideHome = steps - stepsToBaseEntry;

        // Legal if remaining steps inside home do not exceed the number of home tiles to traverse (from -1 to finalIndex = Count - 1 => Count steps max).
        return remainingInsideHome <= player.homeTiles.Count;
    }

    // Helper: steps required to land on the player's base entry from current position on main path,
    // counting the step that moves onto the base entry tile and starts home-path traversal in this turn.
    //
    // - If currently in nest (-1): one step to pop out to base entry + a full loop to re-encounter base entry
    //   for entering home path => tiles.Count + 1.
    // - If already on the base entry tile, it takes a full loop to re-encounter it => tiles.Count.
    // - Else: positive distance to land on the base entry tile (1..tiles.Count-1).
    private int StepsToReachBaseEntry(Player player, PawnTracker tracker)
    {
        if (tracker.currentHomeTileIndex != -1) return int.MaxValue; // already on home path, not relevant here

        int total = tiles.Count;

        // From nest: pop out (1) + full loop to re-encounter base entry.
        if (tracker.currentTileIndex == -1)
            return total + 1;

        // Distance to land on base entry tile.
        int dist = (player.baseTileIndex - tracker.currentTileIndex + total) % total;
        if (dist == 0) dist = total; // same tile -> next occurrence after a full loop

        return dist;
    }
}
