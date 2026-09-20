using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    public static Enemy Instance;

    public Transform player;
    private NavMeshAgent agent;
    [SerializeField] Animator animator;

    [SerializeField] private Transform weaponTip;
    [SerializeField] private GameObject bullet;
    [SerializeField] private float bulletSpeed;

    [SerializeField] private float attackDistance;
    [SerializeField] private float attackCooldown = 1.5f;
    private float attackTimer = 5f;

    private bool isDead;


    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("player").transform;
        agent = GetComponent<NavMeshAgent>();
        animator.SetBool("isRunning", true);
        isDead = false;
    }

    // Update is called once per frame
    void Update()
    {
        attackTimer -= Time.deltaTime;

        if (Vector3.Distance(transform.position, player.transform.position) <= attackDistance)
        {
            animator.SetBool("isRunning", false);
            animator.SetTrigger("Shooting");
            Attack();
        } 
        else
        {
            agent.isStopped = false;
            animator.SetBool("isRunning", true);
            agent.SetDestination(player.transform.position);

        }


    }



    private void Attack()
    {
        // need to face enemy towards player
        // transform.position.Rotate();
        if (isDead)
            return;

        agent.isStopped = true;

        if (attackTimer > 0f)
            return;

        // Reset timer
        attackTimer = attackCooldown;


        Vector3 playerDirection = (player.position - weaponTip.position).normalized;
        transform.rotation = Quaternion.LookRotation(playerDirection);

        Ray ray = new Ray(weaponTip.position, playerDirection);
        Vector3 targetPoint;


        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            targetPoint = hit.point;

            if (hit.transform.CompareTag("Player"))
            {
                // play anim
                // damage player
                PullTrigger(targetPoint);
                Player.Instance.TakeDamage();

            }

        }
        else
        {
            targetPoint = ray.GetPoint(100f);
        }


        //Vector3 playerDirection = (targetPoint - weaponTip.position).normalized;
        //Quaternion.LookRotation(playerDirection);

    }

    private void PullTrigger(Vector3 targetPoint)
    {
        Vector3 shootDirection = (targetPoint - weaponTip.position).normalized;
        // create bullet from gun tip
        GameObject newBullet = Instantiate(bullet, weaponTip.position,
           Quaternion.LookRotation(shootDirection)); // also rotated 90 to look like a real bullet
        // move bullet in direction created earlier
        newBullet.GetComponent<Rigidbody>().linearVelocity = shootDirection * bulletSpeed; 
    }


    public void Death()
    {
        Debug.Log("Enemy has died!");
        isDead = true;
        GameManager.Instance.DecreaseEnemyCount();
        animator.SetTrigger("Death");
        Destroy(gameObject, 5f);
    }

}
