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

    [Header("Turn Choice UI")]
    public GameObject choicePanel;   // panel berisi 2 tombol: SpinWheel & Shop
    public Button spinWheelButton;   // tombol untuk Spin Wheel
    public Button shopButton;        // tombol untuk buka Shop
    public GameObject shopPanel;     // panel Shop milikmu (GameObject "Shop")

    private bool canRoll = true;
    private bool gameOver = false;

    void Start()
    {
        if (spinWheel != null)
            spinWheel.OnSpinComplete += OnWheelComplete;

        if (spinWheelUI != null)
            spinWheelUI.SetActive(false);

        if (choicePanel != null)
            choicePanel.SetActive(false);

        if (shopPanel != null)
            shopPanel.SetActive(false);

        HandleCurrentTurn();
    }

    // === PHASE 1: Player chooses between Shop or Spin Wheel ===
    void HandleCurrentTurn()
    {
        Player currentPlayer = players[currentPlayerIndex];

        // pastikan UI pilihan selalu mati saat giliran berganti
        if (choicePanel != null) choicePanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (spinWheelUI != null) spinWheelUI.SetActive(false);

        if (currentPlayer.isComputer)
        {
            if (diceButton != null)
                diceButton.gameObject.SetActive(false);

            Debug.Log("Computer's turn...");
            StartCoroutine(ComputerTurn());
        }
        else
        {
            Debug.Log($"Giliran {currentPlayer.playerName}, pilih Spin Wheel atau Shop...");

            if (choicePanel != null)
                choicePanel.SetActive(true);

            if (diceButton != null)
                diceButton.gameObject.SetActive(false);

            // bersihkan listener sebelumnya
            spinWheelButton.onClick.RemoveAllListeners();
            shopButton.onClick.RemoveAllListeners();

            // jika pilih spin wheel
            spinWheelButton.onClick.AddListener(() =>
            {
                Debug.Log("Pemain memilih Spin Wheel");
                choicePanel.SetActive(false);
                RollDice();
            });

            // jika pilih shop
            shopButton.onClick.AddListener(() =>
            {
                Debug.Log("Pemain memilih Shop");
                choicePanel.SetActive(false);
                OpenShop();
            });
        }
    }

    void OpenShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);
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
        PlayerTileMover mover = player.pawn.GetComponent<PlayerTileMover>();

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
    }

    IEnumerator ComputerTurn()
    {
        yield return new WaitForSeconds(1.5f);

        // Hide all UI to make sure it doesn't appear
        if (choicePanel != null) choicePanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (spinWheelUI != null) spinWheelUI.SetActive(false);
        if (spinWheel.quizPopup != null) spinWheel.quizPopup.quizPanel.SetActive(false);

        Debug.Log("Computer's turn (silent mode).");

        // Simulate random difficulty as if from the wheel
        string[] difficulties = { "Easy", "Normal", "Hard", "Lucky" };
        string chosenDifficulty = difficulties[Random.Range(0, difficulties.Length)];
        Player aiPlayer = players[currentPlayerIndex];

        // Simulate quiz result: 70% chance correct
        bool correct = Random.value < 0.7f;

        int steps = GetStepsFromDifficulty(chosenDifficulty);
        int coinReward = 0;
        switch (chosenDifficulty)
        {
            case "Easy": coinReward = 25; break;
            case "Normal": coinReward = 50; break;
            case "Hard": coinReward = 75; break;
            case "Lucky": coinReward = 100; break;
        }

        if (chosenDifficulty == "Lucky" || correct)
        {
            aiPlayer.coin += coinReward;
            Debug.Log($"Computer got '{chosenDifficulty}' and answered {(correct ? "correctly" : "by luck")}! Moves {steps} steps, earns {coinReward} coins.");
            yield return MovePlayer(aiPlayer, steps);
        }
        else
        {
            Debug.Log($"Computer failed the '{chosenDifficulty}' quiz! No movement.");
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
