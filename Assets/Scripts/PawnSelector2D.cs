using UnityEngine;
using UnityEngine.EventSystems;

public class PawnSelector2D : MonoBehaviour, IPointerClickHandler
{
    private Player owner;
    private int pawnIndex;

    public void Initialize(Player player, int index)
    {
        owner = player;
        pawnIndex = index;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Only respond if the click actually hits this object
        if (GameManager.Instance == null || owner == null) return;

        GameManager.Instance.OnPawnClicked(owner, pawnIndex);
    }
}
