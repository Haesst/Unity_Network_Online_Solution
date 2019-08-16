using UnityEngine;

public class PlayerInput : MonoBehaviour
{
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
