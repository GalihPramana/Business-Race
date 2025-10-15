using System.Collections;
using UnityEngine;
using System;

public class RotateWheel : MonoBehaviour
{
    public bool isSpinning = false;
    private float spinDuration = 7f;
    private float finalAngle;

    // Callback for when spin finishes
    public Action<string> OnSpinComplete;

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

        Vector3 finalRotation = transform.eulerAngles;
        finalRotation.z = Mathf.Repeat(finalRotation.z, 360f);
        transform.eulerAngles = finalRotation;
        finalAngle = finalRotation.z;

        isSpinning = false;
        string reward = GetReward(finalAngle);

        Debug.Log($"Reward: {reward}");
        OnSpinComplete?.Invoke(reward); // notify GameManager
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
