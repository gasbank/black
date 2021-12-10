﻿using UnityEngine;
using AudienceNetwork;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AudienceNetwork.Utility;
using System;

public class AdViewScene : BaseScene
{
    private AdView adView;
    private AdPosition currentAdViewPosition;
    private ScreenOrientation currentScreenOrientation;
    public Text statusLabel;
    private AdSize[] adSizeArray = (AdSize[])Enum.GetValues(typeof(AdSize));
    private int currentAdSize;
    public Button loadAdButton;

    void OnDestroy()
    {
        // Dispose of banner ad when the scene is destroyed
        if (adView)
        {
            adView.Dispose();
        }
        Debug.Log("AdViewTest was destroyed!");
    }

    private void Awake()
    {
        AudienceNetworkAds.Initialize();
        SetLoadAddButtonText();
        SettingsScene.InitializeSettings();
    }

    private void SetLoadAddButtonText()
    {
        loadAdButton.GetComponentInChildren<Text>().text =
            "Load Banner (" + adSizeArray[currentAdSize].ToString() + ")";
    }

    public void LoadBanner()
    {
        if (adView)
        {
            adView.Dispose();
        }

        statusLabel.text = "Loading Banner...";

        // Create a banner's ad view with a unique placement ID
        // (generate your own on the Facebook app settings).
        // Use different ID for each ad placement in your app.
        adView = new AdView("YOUR_PLACEMENT_ID", adSizeArray[currentAdSize]);
        

        adView.Register(gameObject);
        currentAdViewPosition = AdPosition.CUSTOM;

        // Set delegates to get notified on changes or when the user interacts
        // with the ad.
        adView.AdViewDidLoad = delegate ()
        {
            currentScreenOrientation = Screen.orientation;
            adView.Show(100);
            string isAdValid = adView.IsValid() ? "valid" : "invalid";
            statusLabel.text = "Banner loaded and is " + isAdValid + ".";
            Debug.Log("Banner loaded");
        };
        adView.AdViewDidFailWithError = delegate (string error)
        {
            statusLabel.text = "Banner failed to load with error: " + error;
            Debug.Log("Banner failed to load with error: " + error);
        };
        adView.AdViewWillLogImpression = delegate ()
        {
            statusLabel.text = "Banner logged impression.";
            Debug.Log("Banner logged impression.");
        };
        adView.AdViewDidClick = delegate ()
        {
            statusLabel.text = "Banner clicked.";
            Debug.Log("Banner clicked.");
        };

        // Initiate a request to load an ad.
        adView.LoadAd();
    }

    public void ChangeBannerSize()
    {
        currentAdSize += 1;
        currentAdSize %= adSizeArray.Length;
        SetLoadAddButtonText();
    }

    public void NextScene()
    {
        SceneManager.LoadScene("RewardedVideoAdScene");
    }

    // Change button
    // Change the position of the ad view when button is clicked
    // ad view is at top: move it to bottom
    // ad view is at bottom: move it to 100 pixels along y-axis
    // ad view is at custom position: move it to the top
    public void ChangePosition()
    {
        switch (currentAdViewPosition)
        {
            case AdPosition.TOP:
                SetAdViewPosition(AdPosition.BOTTOM);
                break;
            case AdPosition.BOTTOM:
                SetAdViewPosition(AdPosition.CUSTOM);
                break;
            case AdPosition.CUSTOM:
                SetAdViewPosition(AdPosition.TOP);
                break;
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        if (adView && Screen.orientation != currentScreenOrientation)
        {
            SetAdViewPosition(currentAdViewPosition);
            currentScreenOrientation = Screen.orientation;
        }
    }

    private void SetAdViewPosition(AdPosition adPosition)
    {
        switch (adPosition)
        {
            case AdPosition.TOP:
                adView.Show(AdPosition.TOP);
                currentAdViewPosition = AdPosition.TOP;
                break;
            case AdPosition.BOTTOM:
                adView.Show(AdPosition.BOTTOM);
                currentAdViewPosition = AdPosition.BOTTOM;
                break;
            case AdPosition.CUSTOM:
                adView.Show(100);
                currentAdViewPosition = AdPosition.CUSTOM;
                break;
        }
    }
}
