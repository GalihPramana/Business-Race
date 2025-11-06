using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleBgGameImg : MonoBehaviour
{
    public GameObject backgroundImage;
    public Camera mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        scaleBackgroundFitScreenSize();
    }

    private void scaleBackgroundFitScreenSize()
    {
        // Get device screen aspect ratio
        Vector2 deviceScreenResolution = new Vector2(Screen.width, Screen.height);

        float scrHeight = Screen.height;
        float scrWidth = Screen.width;
        float DeviceScreenAspect = scrWidth / scrHeight;


        // Set Main Camera aspect  = device screen aspect ratio
        mainCamera.aspect = DeviceScreenAspect;

        // Scale background image to fit with camera size
        float camHeight = 100.0f *mainCamera.orthographicSize * 2.0f;
        float camWidth = camHeight * DeviceScreenAspect;

        // Get background image size
        SpriteRenderer backgroundImageSR = backgroundImage.GetComponent<SpriteRenderer>();
        float bgImageWidth = backgroundImageSR.sprite.rect.height;
        float bgImageHeight = backgroundImageSR.sprite.rect.width;

        // Calculate Ratio for scaling
        float bgImg_scale_ratio_Height = camHeight / bgImageHeight;
        float bgImg_scale_ratio_Width = camWidth / bgImageWidth;

        backgroundImage.transform.localScale = new Vector3(bgImg_scale_ratio_Width, bgImg_scale_ratio_Height, 1);
    }
}
