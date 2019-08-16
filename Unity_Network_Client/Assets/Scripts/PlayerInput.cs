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

    private void FixedUpdate()
    {
        if (connectionID <= 0) { ClientTCP.PACKAGE_RequestConnectionID(); }

        if (connectionID == NetPlayer.connectionID)
        {
            float rotation = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;
            float thrust = Input.GetAxis("Vertical") * speed * Time.deltaTime;

            transform.Rotate(new Vector3(0, 0, -1) * rotation);
            rb.AddForce(transform.up * thrust);
        }
        

    }
}

