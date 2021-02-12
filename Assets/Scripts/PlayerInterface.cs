using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerInterface : MonoBehaviour
{
    public static PlayerInterface Instance;

    [Header("Game survive timer")]
    [SerializeField] float surviveToWinTimer = 180;
    [SerializeField] Text countDownText;

    [Header("Interface references")]
    [SerializeField] GameObject debugPanel;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] GameObject gameWonPanel;
    [SerializeField] GameObject escapeMenu;
    [SerializeField] Text debugText;
    [SerializeField] Text followerCounter;

    [Header("Sound")]
    [SerializeField] Slider soundSlider;
    [SerializeField] Text soundValueText;

    [Header("Mouse Sensitivity")]
    [SerializeField] Slider sensitivitySlider;
    [SerializeField] Text sensitivityValueText;
    

    [HideInInspector] public int enemyCounter = 0;
    public bool debugActive = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        gameOverPanel.SetActive(false);
        GameManager.InEscapeMenu = false;

        // Set win countdown to 3 minutes
        GameManager.GameCountDown = surviveToWinTimer;
    }

    
    void Update()
    {
        SetCursor();
        SetSliderValues();
        GameCountDown();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EscapeMenu();
        }

        if (debugActive)
        {
            debugPanel.SetActive(true);
            debugText.text = "Enemy count: " + enemyCounter;
        }
        else
        {
            debugPanel.SetActive(false);
        }

        if (PlayerController.Instance.isDead && !GameManager.GameWon)
        {
            Time.timeScale = 0;
            gameOverPanel.SetActive(true);
            GameManager.GameOver = true;
        }
            

        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugActive = !debugActive;
        }

        followerCounter.text = PlayerController.Instance.minionTrail.Count.ToString();
    }

    public void EscapeMenu()
    {
        escapeMenu.SetActive(!escapeMenu.activeSelf);
        GameManager.InEscapeMenu = !GameManager.InEscapeMenu;

        if (GameManager.InEscapeMenu)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
            
        }
    }

    private void GameCountDown()
    {
        if(!PlayerController.Instance.isDead)
            GameManager.GameCountDown -= Time.deltaTime;

        float timer = Mathf.Floor(GameManager.GameCountDown);
        if (timer < 0)
            timer = 0;

        float seconds = timer % 60f;
        float minutes = Mathf.Floor(timer / 60);

        if (seconds < 10)
        {
            countDownText.text = minutes + ":0" + seconds + " s";
        }
        else
        {
            countDownText.text = minutes + ":" + seconds + " s";
        }

        

        if (!PlayerController.Instance.isDead && timer == 0)
        {
            GameManager.GameWon = true;
            Time.timeScale = 0;
            gameWonPanel.SetActive(true);
        }
    }

    private void SetCursor()
    {
        if (GameManager.InEscapeMenu || GameManager.TopViewMode || gameOverPanel.activeSelf == true || gameWonPanel.activeSelf == true)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void SetSliderValues()
    {
        GameManager.SoundVolume = soundSlider.value;
        soundValueText.text = GameManager.SoundVolume.ToString() + "%";

        GameManager.MouseSensitivity = sensitivitySlider.value;
        sensitivityValueText.text = GameManager.MouseSensitivity.ToString();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ResetGame()
    {
        GameManager.TopViewMode = true;
        GameManager.GameWon = false;
        GameManager.GameOver = false;
        SceneManager.LoadScene(0);
    }
}
