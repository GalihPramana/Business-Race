using UnityEngine;

[System.Serializable]
public class Player
{
    public string playerName;
    public int playerID;
    public Transform pawn; // A reference to the physical pawn object in the scene
    public int currentTileIndex = 0;
    public bool isComputer = false;
    public int baseTileIndex;
    // You could also add a color or team ID here
}