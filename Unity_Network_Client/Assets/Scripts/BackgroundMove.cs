using UnityEngine;

public class BackgroundMove : MonoBehaviour
{
    public static BackgroundMove instance;

    private Transform origin;
    private Camera mainCamera;
    private Transform player;

    public Transform Player { get => player; set => player = value; }

    private void Awake()
    {
        instance = this;
        origin = transform;
        mainCamera = Camera.main;
    }

    private void FixedUpdate()
    {
        if (Player != null)
        {
            player.transform.position = NetPlayer.players[NetPlayer.connectionID].transform.position;
            if (player.transform.position.x > (transform.position.x + 5))
            {
                transform.position = new Vector3(transform.position.x + 1, transform.position.y, transform.position.z);
            }
            if (player.transform.position.x < (transform.position.x + 5))
            {
                transform.position = new Vector3(transform.position.x - 1, transform.position.y, transform.position.z);
            }
            //Debug.Log(player.transform.position);
        }
    }

}
