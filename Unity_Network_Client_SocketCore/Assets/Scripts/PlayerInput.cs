using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput instance;

    public float shipSpeed = 1000f;
    float velocityLimit = 5f;
    public Guid id;

    private Rigidbody2D rb;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private Camera mainCamera;
    private CursorLockMode oldLockState;
    private bool showCursor;

    private void Awake()
    {
        instance = this;
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {

        if (/*Input.GetAxis("Mouse X") != 0 && */ showCursor == false)
        {
            PlayerRotate();
        }

        if (Input.GetAxis("Fire2") != 0 && showCursor == false)
        {
            PlayerThrust();
        }

        if ((transform.position != lastPosition) || (transform.rotation != lastRotation))
        {
            //ClientTCP.PACKAGE_SendMovement(transform.position.x, transform.position.y, transform.rotation.eulerAngles.z);
            lastRotation = transform.rotation;
            lastPosition = transform.position;
            UpdateCameraPosition();
        }
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (showCursor)
            {
                Cursor.visible = false;
                showCursor = false;
                NetworkManager.instance.quitButton.SetActive(false);
                NetPlayer.crossair.SetActive(true);
            }
            else
            {
                Cursor.visible = true;
                showCursor = true;
                NetworkManager.instance.quitButton.SetActive(true);
                NetPlayer.crossair.SetActive(false);
            }
        }

        if (Input.GetButtonDown("Fire1") && showCursor == false)
        {
            NetPlayer.InstantiateNewProjectile(id);
        }
    }
    private void PlayerThrust()
    {
        float thrust = Input.GetAxis("Fire2") * shipSpeed * Time.deltaTime;
        ClientTCP.PACKAGE_SendNewMovement(transform.position, transform.rotation.eulerAngles.z, thrust);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, velocityLimit);
        rb.AddForce(transform.up * thrust);
    }

    private void PlayerRotate()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        float angle = Mathf.Atan2(transform.position.y - mousePosition.y, transform.position.x - mousePosition.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90f));
    }

    private void UpdateCameraPosition()
    {
        mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, mainCamera.transform.position.z);
    }
}

