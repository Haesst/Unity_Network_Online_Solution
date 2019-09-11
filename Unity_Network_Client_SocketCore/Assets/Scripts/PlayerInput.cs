using UnityEngine;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 0.1f;
    [SerializeField] float shipSpeed = 1000f;
    [SerializeField] float velocityLimit = 5f;
    public int connectionID;

    public static PlayerInput instance;
    private Rigidbody2D rb;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private Camera mainCamera;
    private CursorLockMode oldLockState;
    private bool showCursor;
    private GameObject quitButton;

    private void Awake()
    {
        instance = this;
        // Set the rigidbody
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        Cursor.lockState = CursorLockMode.None;
        //quitButton = GameObject.Find("Button_Quit");
        //quitButton.SetActive(false);
        Cursor.visible = false;
    }
    private void OnEnable()
    {
        oldLockState = Cursor.lockState;
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void OnDisable()
    {
        Cursor.lockState = oldLockState;
    }

    private void LateUpdate()
    {
        // Make sure to only be able to move your own spaceship
        if (connectionID == NetPlayer.connectionID)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (showCursor)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    showCursor = false;
                    //quitButton.SetActive(false);
                }
                else
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    showCursor = true;
                    //quitButton.SetActive(true);
                }
            }

            if (Input.GetAxis("Mouse X") != 0 && showCursor == false)
            {
                PlayerRotate();
            }
            if (Input.GetAxis("Fire2") != 0 && showCursor == false)
            {
                PlayerThrust();
            }
            if (Input.GetButtonDown("Fire1") && showCursor == false)
            {
                NetPlayer.InstantiateNewProjectile(connectionID);
            }

            if ((transform.position != lastPosition) || (transform.rotation != lastRotation))
            {
                ClientTCP.PACKAGE_SendMovement(transform.position.x, transform.position.y, transform.rotation.eulerAngles.z);
                lastRotation = transform.rotation;
                lastPosition = transform.position;
                UpdateCameraPosition();
            }
        }
    }
    private void PlayerThrust()
    {
        float thrust = Input.GetAxis("Fire2") * shipSpeed * Time.deltaTime;
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, velocityLimit);
        rb.AddForce(transform.up * thrust);
    }

    private void PlayerRotate()
    {
        float rotation = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        transform.Rotate(0, 0, -rotation);
    }

    private void UpdateCameraPosition()
    {
        if (connectionID == NetPlayer.connectionID)
        {
            mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, mainCamera.transform.position.z);
        }
    }
}

