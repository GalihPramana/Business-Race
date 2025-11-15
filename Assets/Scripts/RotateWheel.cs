using System.Collections;
using UnityEngine;
using System;

public class RotateWheel : MonoBehaviour
{
    public bool isSpinning = false;
    private float spinDuration = 7f;
    private float finalAngle;

    [Header("References")]
    public QuizPopupManager quizPopup;  // drag QuizPopupManager dari scene ke sini lewat Inspector

    public string currentMap = "Keuangan";

    // Callback untuk GameManager jika dibutuhkan
    public Action<string> OnSpinComplete;

    [Header("Spin Audio")]
    public AudioSource spinAudioSource;      // Assign an AudioSource (on this GameObject or child)
    public AudioClip spinClip;               // Your 2s spin SFX
    [Tooltip("Durasi fade out di akhir spin")]
    public float fadeOutDuration = 1.0f;     // Adjust as needed

    private Coroutine audioRoutine;

    private void Start()
    {
        OnSpinComplete += (difficulty) =>
        {
            if (difficulty == "Lucky")
            {
                return;
            }
            if (quizPopup != null)
                //quizPopup.ShowQuiz(difficulty);
                quizPopup.ShowQuiz(currentMap, difficulty);
            else
                Debug.LogWarning("QuizPopupManager belum di-assign di RotateWheel!");
        };
    }

    public void Spin()
    {
        if (!isSpinning)
            StartCoroutine(SpinWheel());
    }

    private IEnumerator SpinWheel()
    {
        isSpinning = true;

        // Start looping audio
        if (spinAudioSource != null && spinClip != null)
        {
            // Stop any previous routine (safety)
            if (audioRoutine != null)
                StopCoroutine(audioRoutine);

            audioRoutine = StartCoroutine(PlaySpinSound());
        }

        float speed = UnityEngine.Random.Range(500f, 1000f);
        float elapsed = 0f;

        while (elapsed < spinDuration)
        {
            float t = elapsed / spinDuration;
            float currentSpeed = Mathf.Lerp(speed, 0, t);
            transform.Rotate(0f, 0f, -currentSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // pastikan rotasi berhenti pada posisi akhir yang valid
        Vector3 finalRotation = transform.eulerAngles;
        finalRotation.z = Mathf.Repeat(finalRotation.z, 360f);
        transform.eulerAngles = finalRotation;
        finalAngle = finalRotation.z;

        isSpinning = false;
        string reward = GetReward(finalAngle);

        Debug.Log($"Reward: {reward}");

        OnSpinComplete?.Invoke(reward);
    }

    private IEnumerator PlaySpinSound()
    {
        // Basic validation
        if (spinAudioSource == null || spinClip == null)
            yield break;

        spinAudioSource.clip = spinClip;
        spinAudioSource.loop = true;
        spinAudioSource.volume = 1f;
        spinAudioSource.Play();

        // Prevent negative wait if fadeOutDuration > spinDuration
        float activeTime = Mathf.Max(0f, spinDuration - fadeOutDuration);
        yield return new WaitForSeconds(activeTime);

        // Fade out
        float startVol = spinAudioSource.volume;
        float t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float k = t / fadeOutDuration;
            spinAudioSource.volume = Mathf.Lerp(startVol, 0f, k);
            yield return null;
        }

        spinAudioSource.Stop();
        spinAudioSource.volume = startVol; // Reset for next spin
    }

    private string GetReward(float rot)
    {
        if (rot >= 0 && rot < 45)
            return "Hard";
        else if (rot >= 45 && rot < 135)
            return "Lucky";
        else if (rot >= 135 && rot < 225)
            return "Easy";
        else if (rot >= 225 && rot < 315)
            return "Normal";
        else
            return "Hard"; // fallback
    }
}
