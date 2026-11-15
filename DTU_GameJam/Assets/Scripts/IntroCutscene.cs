using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroCutscene : MonoBehaviour
{
    public CameraFollow cameraFollow;
    public RectTransform bubble;
    public TMP_Text bubbleText;
    public string line = "CLEAN YOUR ROOM! NOW!";

    [Header("Timing")]
    public float introZoom = 3f;        // starting orthographic size (smaller = closer)
    public float startHold = 1.2f;      // seconds watching the player before the yell
    public float shakeDuration = 0.6f;
    public float shakeMagnitude = 0.25f;
    public float bubbleTime = 2.2f;     // how long the bubble stays up
    public float zoomOutTime = 1.2f;    // time for the camera to settle before the timer starts
    public float charsPerSecond = 30f;
    
    [Header("Mom portrait")]
    public Image momPortrait;
    public Sprite momCalm;          // shown when the bubble appears
    public Sprite momYelling;       // shown after the swap point
    public int swapAtCharacter = 9; // swap once this many characters have been typed
    
    void Awake()
    {
        if (bubble) bubble.gameObject.SetActive(false);
        if (momPortrait) momPortrait.gameObject.SetActive(false);
        
    }

    public void Play(Action onDone) => StartCoroutine(Run(onDone));

    IEnumerator Run(Action onDone)
    {
        float normalSize = cameraFollow.GetComponent<Camera>().orthographicSize;

        // 1. start zoomed in on the player
        cameraFollow.SnapZoom(introZoom);
        yield return new WaitForSeconds(startHold);
        
        // 2. mom yells: shake + portrait + speech bubble
        cameraFollow.Shake(shakeDuration, shakeMagnitude);

        momPortrait.sprite = momCalm;
        momPortrait.gameObject.SetActive(true);
        bubble.gameObject.SetActive(true);
        bubbleText.text = "";

        StartCoroutine(Pop(momPortrait.rectTransform, 0.25f));
        yield return Pop(bubble, 0.25f);
        yield return Typewriter(bubbleText, line);
        yield return new WaitForSeconds(bubbleTime);

        bubble.gameObject.SetActive(false);
        momPortrait.gameObject.SetActive(false);

        // 3. zoom back to the normal view, then hand over to the game
        cameraFollow.ZoomTo(normalSize);
        yield return new WaitForSeconds(zoomOutTime);

        onDone?.Invoke();
    }

    // scales the bubble in with a small overshoot
    IEnumerator Pop(RectTransform rt, float duration)
    {
        Vector3 baseScale = rt.localScale;   // whatever you set in the editor
        const float c1 = 1.70158f, c3 = c1 + 1f;

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = t / duration - 1f;
            float s = 1f + c3 * k * k * k + c1 * k * k;
            rt.localScale = baseScale * s;
            yield return null;
        }
        rt.localScale = baseScale;
    }
    
    IEnumerator Typewriter(TMP_Text text, string message)
    {
        text.text = message;
        text.maxVisibleCharacters = 0;
        
        text.ForceMeshUpdate();   // so the character count is correct right away

        int total = text.textInfo.characterCount;
        float delay = 1f / charsPerSecond;

        for (int i = 1; i <= total; i++)
        {
            text.maxVisibleCharacters = i;

            if (i == swapAtCharacter && momPortrait && momYelling)
            {
                momPortrait.sprite = momYelling;
            }
            
            char c = message[Mathf.Min(i - 1, message.Length - 1)];
            if (c == '!' || c == '.' || c == ',') yield return new WaitForSeconds(0.15f);
            yield return new WaitForSeconds(delay);
        }
    }
}