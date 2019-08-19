using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput instance;
    public int connectionID;
    private Rigidbody rb;
    [SerializeField] float rotationSpeed = 400;
    [SerializeField] float speed = 1000;

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
            }
            
        }
    }

    private void PlayerMovement()
    {
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;
        float thrust = Input.GetAxis("Vertical") * speed * Time.deltaTime;

        transform.Rotate(new Vector3(0, 0, -1) * rotation);
        rb.AddForce(transform.up * thrust);

        ClientTCP.PACKAGE_SendMovement(transform.position.x, transform.position.y, transform.rotation.z);
    }
}

