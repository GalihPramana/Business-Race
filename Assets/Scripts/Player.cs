using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    public string playerName;
    public int playerID;
    public List<Transform> pawns = new List<Transform>(); // 2 pawns
    public int activePawnIndex = -1; // 0 or 1 when selected

    public int currentTileIndex = -1;
    public bool isComputer = false;
    public int baseTileIndex;

    public List<Transform> homeTiles;
    public int currentHomeTileIndex = -1;
    public int coin;

    // Helper function to get the currently active pawn
    public Transform ActivePawn =>
        (activePawnIndex >= 0 && activePawnIndex < pawns.Count) ? pawns[activePawnIndex] : null;
}
