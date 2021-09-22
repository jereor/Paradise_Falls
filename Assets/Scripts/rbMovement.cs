using UnityEngine;

public class rbMovement : MonoBehaviour
{

    [Range(1, 10)] public float speed = 10;

    [Range(1, 10)] public float jumpVelocity;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKey("d"))
        {
            transform.Translate(Vector2.right * speed * Time.deltaTime);
        }
        else if (Input.GetKey("a"))
        {
            transform.Translate(Vector2.left * speed * Time.deltaTime);
        }
        if (Input.GetButtonDown("Jump"))
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.up * jumpVelocity;
        }
    }
}
