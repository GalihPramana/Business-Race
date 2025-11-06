using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{

    public void StartGame()
    {
        SceneManager.LoadScene("Scene");
    }

    public void PilihMapKeuangan()
    {
        // 1. Simpan pilihan player ke PlayerPrefs
        PlayerPrefs.SetString("SelectedMap", "Keuangan");

        // 2. Pindah ke scene gameplay
        SceneManager.LoadScene("Scene");
    }

    public void PilihMapMarketing()
    {
        PlayerPrefs.SetString("SelectedMap", "Marketing");
        SceneManager.LoadScene("Scene");
    }

    public void PilihMapPajak()
    {
        PlayerPrefs.SetString("SelectedMap", "Pajak");
        SceneManager.LoadScene("Scene");
    }

}
