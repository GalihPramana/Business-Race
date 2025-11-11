using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SwitchScene : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainPanel;        // DRAG Panel utama (yg ada tombol StartGame) ke sini
    public GameObject mapSelectionPanel; // DRAG Panel (PanelPilihMap) ke sini

    public UIManager uiManager;



    void Start()
    {
        // Saat scene menu pertama kali nyala,
        // pastikan panel utama terlihat dan panel map tersembunyi.
        if (mainPanel != null) mainPanel.SetActive(true);
        if (mapSelectionPanel != null) mapSelectionPanel.SetActive(false);
    }


    public void StartGame()
    {
        //SceneManager.LoadScene("Scene");
        if (mainPanel != null) mainPanel.SetActive(false);
        if (mapSelectionPanel != null) mapSelectionPanel.SetActive(true);
    }

    public void QuitGame()
    {
        // This will quit the built application
        Application.Quit();
    }

    // (OPSIONAL) Fungsi untuk tombol "Back" di panel map
    public void ShowMainPanel()
    {
        if (mapSelectionPanel != null) mapSelectionPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }


    public void PilihMapKeuangan()
    {
        uiManager.SavePlayerSettings();
        // 1. Simpan pilihan player ke PlayerPrefs
        PlayerPrefs.SetString("SelectedMap", "Keuangan");

        // 2. Pindah ke scene gameplay
        SceneManager.LoadScene("Scene");
    }

    public void PilihMapMarketing()
    {
        uiManager.SavePlayerSettings();
        PlayerPrefs.SetString("SelectedMap", "Marketing");
        SceneManager.LoadScene("Scene");
    }

    public void PilihMapPajak()
    {
        uiManager.SavePlayerSettings();
        PlayerPrefs.SetString("SelectedMap", "Pajak");
        SceneManager.LoadScene("Scene");
    }

    public void PilihMapAdventure()
    {
        uiManager.SavePlayerSettings();
        PlayerPrefs.SetString("SelectedMap", "Adventure");
        SceneManager.LoadScene("Scene");
    }
}
