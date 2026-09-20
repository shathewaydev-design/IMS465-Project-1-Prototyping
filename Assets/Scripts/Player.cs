using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Player Instance;

    public Rigidbody rb;

    public float moveSpeed;
    public float jumpForce;

    private Vector3 _moveDirection;
    private Vector2 _lookInput;

    public bool _isGrounded;

    [SerializeField] private Transform playerCamera;
    [SerializeField] private float mouseSensitivity;
    private float cameraPitch = 0f;

    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform weaponTip;
    [SerializeField] private Camera camera;
    [SerializeField] private float bulletSpeed = 25f;



    public InputActionReference move;
    public InputActionReference jump;
    public InputActionReference look;
    public InputActionReference fire;
    public InputActionReference deadeye;


    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        _moveDirection = move.action.ReadValue<Vector3>();
        _lookInput = look.action.ReadValue<Vector2>();

        PlayerMovement();
        CameraMovement();

        if (fire.action.WasPressedThisFrame())
        {
            Shoot();
        }

        if (deadeye.action.WasPressedThisFrame())
        {

            GameManager.Instance.SlowEnemyTime();

        }


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

    private void Shoot()
    {
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
        Debug.Log("player taking damage!");
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true;
        }
    }

}
