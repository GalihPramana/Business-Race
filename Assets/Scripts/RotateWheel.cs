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

    // Callback untuk GameManager jika dibutuhkan
    public Action<string> OnSpinComplete;

    private void Start()
    {
        // Hubungkan event hanya sekali
        OnSpinComplete += (difficulty) =>
        {
            // Jangan tampilkan quiz untuk "Lucky"
            if (difficulty == "Lucky")
            {
                Debug.Log("Lucky spin! No quiz this time.");
                return;
            }

            if (quizPopup != null)
                quizPopup.ShowQuiz(difficulty);
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
            return "Hard"; 
    }
}
