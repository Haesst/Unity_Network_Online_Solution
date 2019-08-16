using UnityEngine;

public class PlayerInput : MonoBehaviour
{
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