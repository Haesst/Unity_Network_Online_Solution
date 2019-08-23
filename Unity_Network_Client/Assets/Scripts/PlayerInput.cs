using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 400f;
    [SerializeField] float speed = 1000f;
    [SerializeField] float maxSpeed = 15f;
    [SerializeField] float fireHold = 0.4f;

    public int connectionID;
    private float fireTimer = 0f;

    public static PlayerInput instance;
    private Rigidbody2D rb;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private Camera mainCamera;
    private Transform background;

    private void Awake()
    {
        instance = this;
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        background = GameObject.Find("background").transform;
    }

    private void LateUpdate()
    {
        PlayerMovement();
        if(transform.position != lastPosition || transform.rotation != lastRotation)
        {
            UpdateCameraPosition();
            HandleClientData.SendPlayerMovement(transform.position.x, transform.position.y, transform.rotation.eulerAngles.z);
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }
        if (fireTimer <= 0 && Input.GetAxis("Fire1") > 0)
        {
            GameObject go = Instantiate(Resources.Load("Prefabs/Bullet", typeof(GameObject)), transform.position + transform.up, transform.rotation) as GameObject;
            go.GetComponent<bullet>().SetSource(gameObject, transform.position + transform.up);
            fireTimer = fireHold;
        }

        if(fireTimer > 0)
        {
            fireTimer -= Time.deltaTime;
        }
        if(Vector3.Magnitude(rb.velocity) > maxSpeed)
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
        }
    }

    private void PlayerMovement()
    {
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;
        float thrust = Input.GetAxis("Vertical") * speed * Time.deltaTime;

        transform.Rotate(new Vector3(0, 0, -1) * rotation);
        rb.AddForce(transform.up * thrust);
    }
    public static float Berp(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
        return start + (end - start) * value;
    }
    private void UpdateCameraPosition()
    {
        mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, mainCamera.transform.position.z);
        background.position = new Vector3(Berp(background.position.x, transform.position.x, 1f), Berp(background.position.y, transform.position.y, 1f), background.position.z);
    }
}

