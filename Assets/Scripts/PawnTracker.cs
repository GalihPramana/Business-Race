using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnTracker : MonoBehaviour
{
    // Indeks petak di main path. -1 berarti masih di base/nest.
    public int currentTileIndex;

    // Indeks petak di home path. -1 berarti masih di main path.
    public int currentHomeTileIndex = -1;
}