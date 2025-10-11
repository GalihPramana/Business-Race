using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTileMover : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Transform player;

    void Start()
    {
        player = this.transform;
    }

    // This method is now public and returns an IEnumerator
    // so the GameManager can wait for the move to finish.
    public IEnumerator MoveToTile(Transform targetTile)
    {
        while (Vector2.Distance(player.position, targetTile.position) > 0.01f)
        {
            player.position = Vector2.MoveTowards(player.position, targetTile.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        player.position = targetTile.position;
    }
}