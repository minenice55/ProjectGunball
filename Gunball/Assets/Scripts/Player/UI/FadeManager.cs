using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    [SerializeField] Image fadePanel;
    static float startFadeTime, fadeIn, fadeOut, fadeHold;
    static FadeManager instance;

    void Start()
    {
        instance = this;
        fadePanel.color = new Color(0f, 0f, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        float time = Time.time;
        if (time > fadeIn && time < fadeHold)
            fadePanel.color = new Color(0f, 0f, 0f, 1f);
        else if (time > startFadeTime && time < fadeIn)
            fadePanel.color = new Color(0f, 0f, 0f, (time - startFadeTime) / (fadeIn - startFadeTime));
        else if (time > fadeHold && time < fadeOut)
            fadePanel.color = new Color(0f, 0f, 0f, 1f - ((time - fadeHold) / (fadeOut - fadeHold)));
        else
            fadePanel.color = new Color(0f, 0f, 0f, 0f);
    }

    public static void Fade(float startTime, float fadeInTime, float fadeOutTime, float fadeHoldTime)
    {
        startFadeTime = startTime;
        fadeIn = fadeInTime;
        fadeOut = fadeOutTime;
        fadeHold = fadeHoldTime;
    }
}
