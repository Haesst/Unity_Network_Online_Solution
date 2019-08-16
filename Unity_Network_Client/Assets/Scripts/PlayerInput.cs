using UnityEngine;

public class PlayerInput : MonoBehaviour
{
<<<<<<< HEAD
    public static PlayerInput instance;
    public int connectionID;

    private void Awake()
    {
        instance = this;
    }

    private void FixedUpdate()
    {
        if (connectionID <= 0)
        {
            ClientTCP.PACKAGE_RequestConnectionID();
        }
    }

}
=======
    private Rigidbody rb;
    [SerializeField] float rotationSpeed = 400;
    [SerializeField] float speed = 1000;

    private void Awake()
    {
        // Set the rigidbody
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;
        float thrust = Input.GetAxis("Vertical") * speed * Time.deltaTime;

        transform.Rotate(new Vector3(0,0,-1) * rotation);
        rb.AddForce(transform.up * thrust);
    }
}
>>>>>>> 6f13bf17af905de1f66e328ae9368709adf901ce
