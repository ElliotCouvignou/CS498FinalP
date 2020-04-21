using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // THIS IS NOT USINGRIGIDBODIES, they tend to cause collision issues from what i see
    

    private CharacterController controller;

    public float moveSpeed = 100f;
    public float acceleration = 4f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;
    public float airStrafe = 20f; // lower val -> more control
    public float groundFriction = 1.1f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public Vector3 velocity;
    bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        velocity = new Vector3(0f, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        // check for floors underneath, done via sphere collision check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        

        if(isGrounded && velocity.y < 0f)
        {
            velocity.y = -0.5f;
            velocity = velocity / groundFriction;
        }

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        // ground movement
        if (isGrounded)
        {
            Vector3 move = (transform.right * x + transform.forward * y) * acceleration;
            velocity.x += move.x;
            velocity.z += move.z;
            velocity = Vector3.ClampMagnitude(velocity, moveSpeed);
            
        }
        else
        {
            // Gravity
            velocity.y += gravity * Time.deltaTime;  //  x = 1/2 a t^2
        }
        // jumping
        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            Debug.Log("Vel: ");
            Debug.Log(velocity.normalized);
            Debug.Log("Forward: ");
            Debug.Log(transform.forward);
            velocity.y = Mathf.Sqrt(jumpHeight*-1f* gravity);
            isGrounded = false;
        }

        // apply movement
        controller.Move(velocity * Time.deltaTime);



    }
}
