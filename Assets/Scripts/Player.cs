using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    public string playerName;
    public int playerID;
    public List<Transform> pawns = new List<Transform>(); // 2 pawns
    public int activePawnIndex = -1; // 0 or 1 when selected

    public bool isComputer = false;
    public bool isFrozen = false;
    public bool isFinished = false;
    public bool isBom = false;
    public int baseTileIndex;

    public List<Transform> homeTiles;
    public int coin;

    // Achievement progress tracking
    public AchievementProgress achievements = new AchievementProgress();

    // Helper function to get the currently active pawn
    public Transform ActivePawn =>
        (activePawnIndex >= 0 && activePawnIndex < pawns.Count) ? pawns[activePawnIndex] : null;
}

[System.Serializable]
public class AchievementProgress
{
    // Unlock flags
    public bool got300Coins;
    public bool fiveConsecutiveCorrect;
    public bool frozeOpponents5Times;
    public bool threwOpponentsToBase3Times;

    // Counters
    public int consecutiveCorrect;     // streak of correct answers
    public int frozeOpponentsCount;    // how many times this player froze others (Iceball)
    public int threwToBaseCount;       // how many times this player threw others back to base (Bom)
}
