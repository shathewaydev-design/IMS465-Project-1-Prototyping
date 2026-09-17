using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameManager Instance;

    [SerializeField] private float spawnTimer;
    [SerializeField] private float maxEnemyCount;

    public GameObject enemy;


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
        SpawnManager();
    }

    public void SpawnManager()
    {

    }

    public void SlowEnemyTime()
    {

    }
}
