using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static  GameManager Instance;

    [SerializeField] private float spawnTimer;
    [SerializeField] private float spawnCooldown;

    [SerializeField] private float maxEnemyCount;
    private float enemyCount;

    [SerializeField] private Transform spawnLocation;

    public GameObject enemy;

    [SerializeField] private float slowDuration = 10f;
    [SerializeField] private float slowCooldown = 30f;
    [SerializeField] private float slowTimeScale = 0.5f;

    private float timer;
    private bool timeSlowed = false;
    private bool isOnCooldown = false;


    private void Awake()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        Debug.Log(timer);

        spawnTimer -= Time.deltaTime;
        SpawnManager();

        if (timeSlowed)
        {
            timer -= Time.unscaledDeltaTime;

            if (timer <= 0)
            {
                Time.timeScale = 1f;
                timeSlowed = false;

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

        GameObject newEnemy = Instantiate(enemy, spawnLocation.position, Quaternion.identity);
        enemyCount++;
        Debug.Log("# of enemies: " + enemyCount);

        
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

            //Time.timeScale = 1f;
            //timeSlowed = false;
            //Debug.Log("time returned to normal!");
            //return;
        }




        //Time.timeScale = 0.5f;
        //timeSlowed = true;
        //Debug.Log("time slowed!");
    }


    public bool GetTimeSlowed()
    {
        return timeSlowed;
    }

}

