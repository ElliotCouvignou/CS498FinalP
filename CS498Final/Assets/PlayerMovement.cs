using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // THIS IS NOT USINGRIGIDBODIES, they tend to cause collision issues from what i see
    

    private CharacterController controller;
    private WallRunning WR_script;

    public float moveSpeed = 10f;
    public float sprintSpeed = 15f;
    public float walkSpeed = 10f;
    public float acceleration = 4f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;
    public float airStrafe = 20f; // lower val -> more control
    public float groundFriction = 1.8f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public Vector3 velocity;
    bool isGrounded;
    public bool isWallRunning;
    bool isSliding;
    bool isSprinting;

    // Start is called before the first frame update
    void Start()
    {
        // update movespeed so equillibrium speed = expected movespeed
        WR_script = GetComponent<WallRunning>();
        controller = GetComponent<CharacterController>();
        velocity = new Vector3(1f, 0f, -1f);

        isWallRunning = false;
        isSliding = false;
        isSprinting = false;
    }

    // Update is called once per frame
    void Update()
    {
        // check for floors underneath, done via sphere collision check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // check for collisions on left/right wallruns
        int WR_status = WR_script.checkWalls();

        // check for outof wallrunning
        if (WR_status == 0 && isWallRunning)
        {
            isWallRunning = false;
            GetComponent<WallRunning>().undoWallRun();
        }    



        // USER INPUT SECTION

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        bool hasMoveInput = Mathf.Abs(x) > 0.1f || Mathf.Abs(y) > 0.1f;
        bool SprintPressed = Input.GetKey(KeyCode.LeftShift);
       
        // ground movement
        if (isGrounded)
        {
            if (hasMoveInput)
            {
                // check for sprints
                isSprinting = SprintPressed;
                if (isSprinting)
                    moveSpeed = sprintSpeed;
                else
                    moveSpeed = walkSpeed;
                
                Vector3 move = (transform.right * x + transform.forward * y) * acceleration;
                velocity.x += move.x;
                velocity.z += move.z;
                velocity = Vector3.ClampMagnitude(velocity, moveSpeed);
            }
            if (isWallRunning)
            {
                // exit
                isWallRunning = false;
                WR_script.undoWallRun();
            }

            // reset Y velocity of we touch ground
            if (hasMoveInput && velocity.y < -0.5f)
            {
                velocity.y = -0.5f; // make sure we 'stick' to ground
            }

            // apply ground friction
            if (!hasMoveInput)
            {
                velocity -= velocity*groundFriction * Time.deltaTime;
            }
        }
        else
        {
            // CHECK ENTER WALLRUN
            if (WR_status == 1 && !isWallRunning)
            {
                isWallRunning = true;
                WR_script.doWallRun(true);
            }
            if (WR_status == 2 && !isWallRunning)
            {
                isWallRunning = true;
                WR_script.doWallRun(false);
            }

            // Gravity
            if(!isWallRunning)
                velocity.y += gravity * Time.deltaTime;  //  x = 1/2 a t^2
            else
                WR_script.inWallRunUpdate();
        }
        // jumping
        if(Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                velocity.y = 0;
                float mag = velocity.magnitude;

                if (hasMoveInput)
                {
                    Vector3 move = (transform.right * x + transform.forward * y) * mag;
                    velocity = new Vector3(move.x, Mathf.Sqrt(jumpHeight * -1f * gravity), move.z);
                }
                else
                {
                    velocity.y = Mathf.Sqrt(jumpHeight * -1f * gravity);
                }
                
                isGrounded = false;
                isSprinting = false;
            }
            else if (isWallRunning)
            {
                if(WR_script.jumpExitWallRun())
                    isWallRunning = false;
            }
            else
            {
                // walljump boost check
                WR_script.wallJumpBackBoost();
            }
        }




        // apply movement
        controller.Move(velocity * Time.deltaTime);

    }
}
