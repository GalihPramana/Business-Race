using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTileMover : MonoBehaviour
{
    [Header("Tiles Settings")]
    public List<Transform> tiles;   // Assign your tile positions in Inspector
    public float moveSpeed = 5f;    // Speed of movement
    private int currentTileIndex = 0;
    private bool isMoving = false;

    private Transform player;

    void Start()
    {
        player = this.transform; // Player this script is attached to
    }

    // Call this function from your Button OnClick
    public void MoveToNextTile()
    {
        if (!isMoving && currentTileIndex < tiles.Count)
        {
            StartCoroutine(MoveToTile(tiles[currentTileIndex]));
            currentTileIndex++; // Increase AFTER moving
        }
    }

    private System.Collections.IEnumerator MoveToTile(Transform targetTile)
    {
        isMoving = true;

        while (Vector2.Distance(player.position, targetTile.position) > 0.01f)
        {
            player.position = Vector2.MoveTowards(player.position, targetTile.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        player.position = targetTile.position; // Snap to tile
        isMoving = false;
    }
}
