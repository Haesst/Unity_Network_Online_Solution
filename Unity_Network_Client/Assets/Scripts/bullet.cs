using UnityEngine;

public class bullet : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float maxDistance = 2000f;

    private GameObject shooter;
    private Vector3 origin;

    private bool running = false;
    public void SetSource(GameObject gameObject, Vector3 origin)
    {
        shooter = gameObject;
        this.origin = origin;
        running = true;
    }

    private void Update()
    {
        if (running)
        { 
            transform.position += transform.up * speed * Time.deltaTime;
            if (Vector3.Distance(transform.position, origin) >= maxDistance)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.parent.name == shooter.name || collision.gameObject == gameObject)
        {
            return;
        }
        Debug.Log(shooter.name);
        Debug.Log($"Hit: {collision.transform.parent.name}");
        collision.transform.parent.gameObject.GetComponent<ParticleSystem>()?.Play();
        Destroy(gameObject);
    }
}