using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchToMainMenu : MonoBehaviour
{
    private bool _isLoading;

    // Call this from a UI Button (OnClick) or another script.
    public void GoToMainMenu()
    {
        if (_isLoading) return;
        _isLoading = true;
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }
}
