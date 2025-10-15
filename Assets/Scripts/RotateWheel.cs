using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWheel : MonoBehaviour
{
    private bool isSpinning = false;
    private float spinDuration = 7f;

    public void Spin()
    {
        if (!isSpinning)
            StartCoroutine(SpinWheel());
    }

    private IEnumerator SpinWheel()
    {
        isSpinning = true;
        float speed = Random.Range(500f, 1000f);
        float elapsed = 0f;

        while (elapsed < spinDuration)
        {
            float t = elapsed / spinDuration;
            float currentSpeed = Mathf.Lerp(speed, 0, t);

            transform.Rotate(0f, 0f, -currentSpeed * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap rotation cleanly to 0–360 range
        Vector3 finalRotation = transform.eulerAngles;
        finalRotation.z = Mathf.Repeat(finalRotation.z, 360f);
        transform.eulerAngles = finalRotation;

        isSpinning = false;
    }
}
