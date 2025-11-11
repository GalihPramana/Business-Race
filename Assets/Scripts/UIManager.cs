using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TMP_InputField player1Input;
    public TMP_InputField player2Input;
    public TMP_InputField player3Input;
    public TMP_InputField player4Input;

    public void SetPlayers(int count)
    {
        // Make all visible but not interactable
        player1Input.interactable = count >= 1;
        player2Input.interactable = count >= 2;
        player3Input.interactable = count >= 3;
        player4Input.interactable = count >= 4;
    }

    // These functions go to your 1/2/3/4 player buttons
    public void On1PlayerClick() => SetPlayers(1);
    public void On2PlayerClick() => SetPlayers(2);
    public void On3PlayerClick() => SetPlayers(3);
    public void On4PlayerClick() => SetPlayers(4);
}