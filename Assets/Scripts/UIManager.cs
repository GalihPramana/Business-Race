using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TMP_InputField player1Input;
    public TMP_InputField player2Input;
    public TMP_InputField player3Input;
    public TMP_InputField player4Input;

    private int selectedPlayerCount = 4;

    public void SetPlayers(int count)
    {
        // Make all visible but not interactable
        player1Input.interactable = count >= 1;
        player2Input.interactable = count >= 2;
        player3Input.interactable = count >= 3;
        player4Input.interactable = count >= 4;
    }

    public void SavePlayerSettings()
    {
        PlayerPrefs.SetInt("PlayerCount", selectedPlayerCount);

        PlayerPrefs.SetString("P1Name", player1Input.text);
        PlayerPrefs.SetString("P2Name", player2Input.text);
        PlayerPrefs.SetString("P3Name", player3Input.text);
        PlayerPrefs.SetString("P4Name", player4Input.text);

        PlayerPrefs.Save();
    }



    // These functions go to your 1/2/3/4 player buttons
    public void On1PlayerClick()
    {
        selectedPlayerCount = 1;
        SetPlayers(1);
    }

    public void On2PlayerClick()
    {
        selectedPlayerCount = 2;
        SetPlayers(2);
    }

    public void On3PlayerClick()
    {
        selectedPlayerCount = 3;
        SetPlayers(3);
    }

    public void On4PlayerClick()
    {
        selectedPlayerCount = 4;
        SetPlayers(4);
    }

}