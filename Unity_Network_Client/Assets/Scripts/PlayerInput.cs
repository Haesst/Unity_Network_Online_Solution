using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 400;
    [SerializeField] float speed = 1000;

    public int connectionID;

    public static PlayerInput instance;
    private Rigidbody rb;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private void Awake()
    {
        instance = this;
        // Set the rigidbody
        rb = GetComponent<Rigidbody>();
    }

    private void LateUpdate()
    {
        // Make sure to only be able to move your own spaceship
        if (connectionID == NetPlayer.connectionID)
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                PlayerMovement();
                if((transform.position != lastPosition) || (transform.rotation != lastRotation))
                {
                    ClientTCP.PACKAGE_SendMovement(transform.position.x, transform.position.y, transform.rotation.eulerAngles.z);
                }
            }
            
        }
    }

    private void PlayerMovement()
    {
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;
        float thrust = Input.GetAxis("Vertical") * speed * Time.deltaTime;

        transform.Rotate(new Vector3(0, 0, -1) * rotation);
        rb.AddForce(transform.up * thrust);

    }
}

