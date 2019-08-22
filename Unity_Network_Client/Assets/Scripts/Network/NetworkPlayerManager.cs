using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayerManager : MonoBehaviour
{
    NetworkPlayerManager instance;
    private void Awake()
    {
        instance = this;
    }
    public static void InstantiateNewPlayer(string connectionID, int spriteID = 0)
    {
        GameObject go = Instantiate(Resources.Load("Prefabs/Player", typeof(GameObject))) as GameObject;
        go.name = $"Player: {connectionID}";
    }

    public static GameObject InstantiateNewOtherPlayer(string connectionID, int spriteID = 0)
    {
        GameObject go = Instantiate(Resources.Load("Prefabs/OtherPlayer", typeof(GameObject))) as GameObject;
        go.name = $"Player: {connectionID}";
        return go;
    }

    public static GameObject InstantiateNewOtherPlayer(string connectionID, float posX, float posY, float rotation, int spriteID = 0)
    {
        Debug.Log("Test");
        GameObject go = Instantiate(Resources.Load("Prefabs/OtherPlayer", typeof(GameObject)), new Vector3(posX, posY, 0), Quaternion.Euler(new Vector3(0,0,rotation))) as GameObject;
        go.name = $"Player: {connectionID}";
        return go;
    }

    //private static void SpawnGameObjectFromPrefab(string path, string name = null)
    //{
    //    UnityThread.executeInUpdate(() =>
    //    {
    //        GameObject go = Instantiate(Resources.Load(path, typeof(GameObject))) as GameObject;
    //        go.name = name == null
    //            ? $"Prefab [{path}]"
    //            : name;
    //    });
    //}

    /*
    public void InstantiateNewPlayerAtPosition(int connectionID, float posX, float posY, float rotation, int sprite = 0)
    {
        if (connectionID <= 0 || players.ContainsKey(connectionID)) { return; }

        GameObject go = Instantiate(Resources.Load("Prefabs/Player", typeof(GameObject))) as GameObject;
        go.transform.position = new Vector3(posX, posY, go.transform.position.z);
        go.transform.rotation = Quaternion.Euler(0, 0, rotation);
        go.GetComponent<Player>().ConnectionID = connectionID;
        go.GetComponentInChildren<SpriteRenderer>().sprite = sprites[sprite];
        go.name = $"Player | {connectionID}";
        players.Add(connectionID, go);
    }
    */
}