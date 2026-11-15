using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int levelNumber = 1;   // set to 1, 2, 3... in each level's Inspector
    public enum State { Intro, Countdown, Scanning, Won, Lost }
    public State state { get; private set; }

    [Header("Rules")]
    public float countdownSeconds = 30f;

    [Header("References")]
    public PlayerMovement player;
    public MomController mom;

    [Header("UI")]
    public TMP_Text timerText;
    public GameObject winPanel;
    public GameObject losePanel;
    
    [Header("Camera")]
    public CameraFollow cameraFollow;
    public Transform scanFocus;     // optional: where the camera looks during the scan
    public float scanZoom = 0f;     // orthographic size during the scan, 0 = don't change
    
    float timeLeft;
    
    public float spotZoom = 2.5f;     // orthographic size when zoomed on the item
    public float loseDelay = 1.5f;    // seconds before the lose panel appears
    

    public IntroCutscene intro;
    public static bool skipIntroOnce;   // set on Retry so the intro doesn't replay

    void Start()
    {
        timeLeft = countdownSeconds;
        winPanel.SetActive(false);
        losePanel.SetActive(false);

        mom.Spotted += OnSpotted;
        mom.ScanFinished += OnScanFinished;

        bool skip = skipIntroOnce;
        skipIntroOnce = false;

        if (intro != null && !skip)
        {
            state = State.Intro;
            player.enabled = false;     // no movement during the cutscene
            timerText.text = "";
            intro.Play(BeginCountdown);
        }
        else
        {
            BeginCountdown();
        }
    }

    void BeginCountdown()
    {
        state = State.Countdown;
        player.enabled = true;
    }

    void OnDestroy()
    {
        if (!mom) return;
        mom.Spotted -= OnSpotted;
        mom.ScanFinished -= OnScanFinished;
    }

    void Update()
    {
        if (state != State.Countdown) return;

        timeLeft -= Time.deltaTime;
        timerText.text = Mathf.CeilToInt(Mathf.Max(timeLeft, 0)).ToString();

        if (timeLeft <= 0f) BeginScan();
    }

    void BeginScan()
    {
        state = State.Scanning;
        timerText.text = "She's coming...";

        player.enabled = false;   // stops reading input
        FreezeEverything();       // player and crates stop where they are
        
        if (cameraFollow)
        {
            cameraFollow.SetTarget(scanFocus ? scanFocus : mom.transform);
            if (scanZoom > 0f) cameraFollow.ZoomTo(scanZoom);
        }
        mom.StartScan();
    }

    void FreezeEverything()
    {
        foreach (var rb in FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None))
        {
            rb.linearVelocity = Vector2.zero;   // use rb.velocity on Unity 2022 or earlier
            rb.angularVelocity = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    void OnSpotted()
    {
        if (state != State.Scanning) return;
        state = State.Lost;
        timerText.text = "";
        StartCoroutine(SpottedSequence());
    }

    System.Collections.IEnumerator SpottedSequence()
    {
        if (cameraFollow && mom.CaughtTarget)
        {
            cameraFollow.SetTarget(mom.CaughtTarget);
            cameraFollow.ZoomTo(spotZoom);
        }
        yield return new WaitForSeconds(loseDelay);
        losePanel.SetActive(true);
    }

    void OnScanFinished()
    {
        if (state != State.Scanning) return;
        state = State.Won;
        timerText.text = "";
        Progress.CompleteLevel(levelNumber);
        winPanel.SetActive(true);
    }

    // Hook these to the buttons' On Click ()
    public void Retry()
    {
        skipIntroOnce = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
            SceneManager.LoadScene("LevelSelect");   // no more levels yet
    }

    public void ToLevelSelect() => SceneManager.LoadScene("LevelSelect");
}