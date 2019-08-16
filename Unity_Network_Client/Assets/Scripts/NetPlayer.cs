using UnityEngine;

public class NetPlayer : MonoBehaviour
{
    
    public static int connectionID { get; protected set; }

    public static void SetConnectionID(int id)
    {
        if (connectionID != 0) { return; }
        connectionID = id;
    }
}
