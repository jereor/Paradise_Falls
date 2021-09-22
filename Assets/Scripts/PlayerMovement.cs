using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float speed = 10;
    public float runSpeed;
    public float jumpForce = 5;
    public float gravity = 9.81f;

    public bool grounded = false;


    // Start is called before the first frame update
    void Start()
    {
        runSpeed = speed * 1.5f;
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

        if (grounded && Input.GetKeyDown("space"))
        {

        }
    }
}
