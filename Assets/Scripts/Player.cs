using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Player Instance;

    public static event Action<AudioSource> OnPlayerShoot;
    public static event Action OnSlowTrigger;

    public AudioSource playerAudioSource;

    public Rigidbody rb;

    public float moveSpeed;
    public float jumpForce;

    private Vector3 _moveDirection;
    private Vector2 _lookInput;

    public bool _isGrounded;

    [SerializeField] private Transform playerCamera;
    [SerializeField] private float mouseSensitivity;
    private float cameraPitch = 0f;

    [Header("Dead Eye")]
    [SerializeField] private GameObject targetMarkerPrefab;
    private List<GameObject> targetMarkers = new List<GameObject>();
    [SerializeField] private float targetScanDistance = 100f;
    [SerializeField] private float timeBetweenShots = 0.25f;

    private List<Enemy> markedEnemies = new List<Enemy>();

    private bool isTargeting = false;
    private bool isShootingSequence = false;

    [Header("Gun")]
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform weaponTip;
    [SerializeField] private Camera camera;
    [SerializeField] private float bulletSpeed = 25f;

    [SerializeField] private float health;

    public InputActionReference move;
    public InputActionReference jump;
    public InputActionReference look;
    public InputActionReference fire;
    public InputActionReference deadeye;

    private void Awake()
    {
        Instance = this;
        moveSpeed = 2000f;

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        GameManager.OnTimeRestored += EndDeadEye;
    }

    private void OnDisable()
    {
        GameManager.OnTimeRestored -= EndDeadEye;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("player health: " + health);

        _moveDirection = move.action.ReadValue<Vector3>();
        _lookInput = look.action.ReadValue<Vector2>();

        PlayerMovement();
        CameraMovement();

        if (fire.action.WasPressedThisFrame())
        {
            if (GameManager.Instance.GetTimeSlowed())
            {
                // begin shooting if we are selecting targets
                if (isTargeting && !isShootingSequence)
                {
                    if (markedEnemies.Count > 0)
                    {
                        StartCoroutine(ShootMarkedEnemies());
                    }
                    else
                    {
                        Debug.Log("No targets have been marked!");
                    }
                }

            } 
            else
            {
                Shoot();
            }


        }

        if (deadeye.action.WasPressedThisFrame())
        {
            if (GameManager.Instance.GetTimeSlowed())
            {
                // dead Eye already active, exit early
                GameManager.Instance.EndSlowTimeEarly();
            }
            else
            {
                // attempt to activate Dead Eye
                GameManager.Instance.SlowEnemyTime();

                if (GameManager.Instance.GetTimeSlowed())
                {
                    markedEnemies.Clear();
                    ClearTargetMarkers();

                    isTargeting = true;
                    isShootingSequence = false;

                    OnSlowTrigger?.Invoke();
                }
            }
        }

        if (health <= 0f)
        {
            //Debug.Log("health has reached 0!!!");
            GameManager.Instance.GameOver();
        }


        if (isTargeting)
        {
            TrackEnemy();
        }

    }
    private void EndDeadEye()
    {
        isTargeting = false;
        isShootingSequence = false;

        StopAllCoroutines();

        ClearTargetMarkers();
        markedEnemies.Clear();

        Debug.Log("Dead Eye ended!");
    }

    private void PlayerMovement()
    {
        if (jump.action.WasPressedThisFrame() && _isGrounded)
        {
            Jump();
        }

        Vector3 localMovement = transform.TransformDirection(_moveDirection);

        rb.linearVelocity = new Vector3(localMovement.x * moveSpeed * Time.deltaTime,
            rb.linearVelocity.y, localMovement.z * moveSpeed * Time.deltaTime);
    }

    private void Jump()
    {
        _isGrounded = false;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce,
            rb.linearVelocity.z);
    }

    private void CameraMovement()
    {
        // look left/right
        transform.Rotate(Vector3.up * _lookInput.x * mouseSensitivity);

        // up/down
        cameraPitch -= _lookInput.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

    }


    private void TrackEnemy()
    {
        // only track targets while Dead Eye is active
        if (!isTargeting || isShootingSequence)
            return;

        Ray ray = camera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, targetScanDistance))
        {
            // check whether the ray hit an enemy
            Enemy enemy = hit.collider.GetComponentInParent<Enemy>();

            if (enemy != null)
            {
                MarkEnemy(enemy);
            }
        }
    }

    private void MarkEnemy(Enemy enemy)
    {

        // do not mark the same enemy twice
        if (markedEnemies.Contains(enemy))
            return;

        markedEnemies.Add(enemy);

        // create a marker attached to the enemy
        if (targetMarkerPrefab != null)
        {
            GameObject marker = Instantiate(
                targetMarkerPrefab,
                enemy.transform.position + Vector3.up * 2f,
                Quaternion.identity);

            marker.transform.SetParent(enemy.transform, true);

            targetMarkers.Add(marker);
        }

        Debug.Log("Marked enemy! Total targets: " + markedEnemies.Count);

    }


    private void ShootMarkedEnemy(Enemy enemy)
    {
        // skip enemies that no longer exist
        if (enemy == null)
            return;

        OnPlayerShoot?.Invoke(playerAudioSource);

        Vector3 targetPoint = enemy.transform.position + Vector3.up;
        Vector3 shootDirection = (targetPoint - weaponTip.position).normalized;

        GameObject newBullet = Instantiate(
            bullet,
            weaponTip.position,
            Quaternion.LookRotation(shootDirection));

        Rigidbody bulletRb = newBullet.GetComponent<Rigidbody>();

        if (bulletRb != null)
        {
            bulletRb.linearVelocity = shootDirection * bulletSpeed;
        }

        enemy.Death();
    }

    private IEnumerator ShootMarkedEnemies()
    {
        isShootingSequence = true;
        isTargeting = false;

        // shoot every marked enemy in the order they were marked
        foreach (Enemy enemy in markedEnemies)
        {
            // stop if Dead Eye has expired
            if (!GameManager.Instance.GetTimeSlowed())
                break;

            if (enemy != null)
            {
                ShootMarkedEnemy(enemy);
            }

            // wait before shooting the next target
            yield return new WaitForSeconds(timeBetweenShots);
        }

        isShootingSequence = false;

        // remove the target markers
        ClearTargetMarkers();

        // allow player to start another targeting sequence
        markedEnemies.Clear();
    }

    private void ClearTargetMarkers()
    {
        foreach (GameObject marker in targetMarkers)
        {
            if (marker != null)
            {
                Destroy(marker);
            }
        }

        targetMarkers.Clear();
    }

    private void Shoot()
    {
        // play audio clip of gunshot
        OnPlayerShoot?.Invoke(playerAudioSource);

        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            targetPoint = hit.point;

            if (hit.transform.CompareTag("Enemy"))
            {
                // kill enemy
                Enemy enemy = hit.transform.GetComponent<Enemy>();

                if (enemy != null)
                {
                    enemy.Death();
                }



            }

        }
        else
        {
            targetPoint = ray.GetPoint(100f);
        }

        // direction for the bullet
        Vector3 shootDirection = (targetPoint - weaponTip.position).normalized;
        // create bullet from gun tip
        GameObject newBullet = Instantiate(bullet, weaponTip.position, 
           Quaternion.LookRotation(shootDirection)); 
        // move bullet in direction created earlier
        newBullet.GetComponent<Rigidbody>().linearVelocity = shootDirection * bulletSpeed;

        //Instantiate(bullet);
    }

    public void TakeDamage()
    {
        //Debug.Log("player taking damage!");
        health -= 5f;
    }

    public float GetPlayerHealth()
    {
        return health;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true;
        }
    }


}
