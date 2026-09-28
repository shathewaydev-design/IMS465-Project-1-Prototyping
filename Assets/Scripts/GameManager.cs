using System;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static  GameManager Instance;

    public static event Action OnTimeRestored;

    [SerializeField] private float spawnTimer;
    private float spawnCooldown = 7f;

    [SerializeField] private float maxEnemyCount;
    private float enemyCount;
    private int killCount = 0;

    [SerializeField] private Transform spawnLocation1;
    [SerializeField] private Transform spawnLocation2;
    [SerializeField] private Transform spawnLocation3;

    public GameObject enemy;

    [SerializeField] private float slowDuration = 20f;
    [SerializeField] private float slowCooldown = 30f;
    [SerializeField] private float slowTimeScale = 0.5f;

    private float timer;
    private bool timeSlowed = false;
    private bool isOnCooldown = false;

    public InputActionReference pause;
    private bool isPaused = false;
    [SerializeField] private GameObject PauseScreen;


    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        Debug.Log(spawnTimer);


        spawnTimer -= Time.deltaTime;
        SpawnManager();
        SpawnCooldownUpdate();
        SlowTimeCooldownManager();

        if (pause.action.WasPressedThisFrame())
        {
            PauseGame();
        }


    }

    public void PauseGame()
    {
        if (!isPaused)
        {
            isPaused = true;
            Time.timeScale = 0f;
            // pull up pause screen
            PauseScreen.SetActive(true);
            // give player cursor access
            Player.Instance.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

        } 
        else
        {
            isPaused = false;
            Time.timeScale = 1f;
            // close pause screen
            PauseScreen.SetActive(false);
            Player.Instance.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }


    }

    private void SlowTimeCooldownManager()
    {
        if (timeSlowed)
        {
            timer -= Time.unscaledDeltaTime;

            if (timer <= 0)
            {
                Time.timeScale = 1f;
                timeSlowed = false;

                OnTimeRestored?.Invoke();

                isOnCooldown = true;
                timer = slowCooldown;
            }

        }
        else if (isOnCooldown)
        {
            timer -= Time.unscaledDeltaTime;

            if (timer <= 0)
            {
                isOnCooldown = false;
            }

        }
    }

    public void SpawnManager()
    {
        if (enemyCount >= maxEnemyCount)
            return;

        if (spawnTimer > 0f)
            return;

        spawnTimer = spawnCooldown;

        Transform spawnLocation;

        int randomLocation = UnityEngine.Random.Range(0, 3);

        if (randomLocation == 0)
        {
            spawnLocation = spawnLocation1;
        }
        else if (randomLocation == 1)
        {
            spawnLocation = spawnLocation2;
        }
        else
        {
            spawnLocation = spawnLocation3;
        }


        GameObject newEnemy = Instantiate(enemy, spawnLocation.position, Quaternion.identity);
        enemyCount++;
        //Debug.Log("# of enemies: " + enemyCount);


    }

    private void SpawnCooldownUpdate()
    {
        if (killCount >= 10)
        {
            spawnCooldown = 5f;
        } 
        if (killCount >= 20)
        {
            spawnCooldown = 3f;
        }
        if (killCount >= 30)
        {
            spawnCooldown = 1.5f;
        }
        if (killCount >= 50)
        {
            spawnCooldown = 0.5f;
        }


    }

    public void DecreaseEnemyCount()
    {
        enemyCount--;
    }

    public void SlowEnemyTime()
    {


        
        if (!timeSlowed && !isOnCooldown)
        {
            timeSlowed = true;
            Time.timeScale = slowTimeScale;
            timer = slowDuration;


        }

    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void GameOver()
    {
        // fade to black, then fade in via ui/manager?
        // restart scene
        //RestartScene();
        //Debug.Log("Game over!!!");
        UIManager.Instance.GameOverFade();
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Player.Instance.enabled = false;
        //RestartScene();

    }

    public bool GetTimeSlowed()
    {
        return timeSlowed;
    }

    public int GetKillCount()
    {
        return killCount;
    }

    public void IncreaseKillCount()
    {
        killCount++;
    }

    public float GetSlowTimer()
    {
        return timer;
    }

    public bool GetIsOnCoolDown()
    {
        return isOnCooldown;
    }



}

