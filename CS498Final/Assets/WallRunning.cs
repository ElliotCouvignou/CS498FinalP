using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    public Transform wallCheckL;
    public Transform wallCheckR;
    public Transform wallCheckB;
    public float wallRunDistance = 0.3f;
    public LayerMask wallRunMask;

    public float wallRunAcceleration = 10f;
    public float transitionTime_s = 0.15f;

    public Camera FPSCamera;
    public float cameraTilt = 10;
    public float wallGravity = 0.5f;
    public float wallRunSpeed = 15f;
    public float wallRunJumpImpulse = 15f;
    public float wallFriction = 20f;

    public float wallRunTimeout = 0.1f;
    private bool wallRunTimedOut = false;

    private PlayerMovement player;
    private Vector3 runningDir;
    private Vector3 runningNormal;
    private bool updateReady = false;

    private RaycastHit hit;
    private Vector3 hitdir;

    bool onLeft;
    

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // return: 0 = no wallrun, 1 = wallrun Left, 2 = wallrun Right
    public int checkWalls()
    {
        if (wallRunTimedOut)
            return 0;

       
        Collider[] leftOverlap = Physics.OverlapSphere(wallCheckL.position, wallRunDistance, wallRunMask);
        Collider[] rightOverlap = Physics.OverlapSphere(wallCheckR.position, wallRunDistance, wallRunMask);

        bool leftCheck = false;
        bool rightCheck = false;
        int ret = 0;
        
        if (leftOverlap.Length == 1)
        {
            leftCheck = true;
            ret = 1;
        }
        else if (rightOverlap.Length == 1)
        {
            rightCheck = true;
            ret = 2;
        }
        return ret;
    }

    // called only once as transition into wallrunning
    public void doWallRun(bool isLeft)
    {
        onLeft = isLeft;

        // Step 1: Tilt Camera based on direction
        if (!onLeft)
            StartCoroutine(rotateCameraZ(0f, cameraTilt));
        else
            StartCoroutine(rotateCameraZ(0f, -cameraTilt));

        // Step 2: find vector/line for surface we can move across
        updateReady = false;

        
        StartCoroutine(recordDirNextFrame());
    }

    public void undoWallRun()
    {

        // Step 1: UnTilt Camera based on direction
        FPSCamera.transform.localRotation = Quaternion.Euler(FPSCamera.transform.localEulerAngles.x, 0f, 0f);

        if (!onLeft)
            StartCoroutine(rotateCameraZ(cameraTilt, 0f));
        else
            StartCoroutine(rotateCameraZ(-cameraTilt, 0f));

        onLeft = false;

        

    }
    
    public void inWallRunUpdate()
    {
        if (!player.isWallRunning)
            return;
        if (!updateReady)
        {
            return;
        }
        // Update velocity on User inputs and Gravity + wallFriction

        // Velocity from User Inputs
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        bool hasMoveInput = Mathf.Abs(x) > 0.1f || Mathf.Abs(y) > 0.1f;

        Vector3 move = (transform.right * x + transform.forward * y).normalized;
        float dot = Vector3.Dot(move, runningDir);
        move = runningDir * dot * wallRunAcceleration;

        Vector3 velocityXZ = new Vector3(player.velocity.x, 0f, player.velocity.z);
        if (velocityXZ.magnitude < wallRunSpeed)
        {
            velocityXZ += move;
            Vector3.ClampMagnitude(velocityXZ, wallRunSpeed);
            player.velocity.x = velocityXZ.x;
            player.velocity.z = velocityXZ.z;
        }

        // apply friction
        if (!hasMoveInput)
        {
            player.velocity.x /= wallFriction;
            player.velocity.z /= wallFriction;
        }
        
        // add reduced force from gravity only when negative (running up walls)
        player.velocity.y += player.gravity * wallGravity * Time.deltaTime;  //  x = 1/2 a t^2
        Debug.Log(player.velocity.y);
    }

    public bool jumpExitWallRun()
    {
        
        // check if we face away from  wall, if so jump in that 
        if (Mathf.Abs(Vector3.Dot(transform.forward, runningNormal)) < 0.25f)
            return false;
       
        // pretty much add impulse based on inputs
        float x = Input.GetAxis("Horizontal");
        Vector3 move =  (transform.forward) * wallRunJumpImpulse;
        move.y = move.magnitude;
        player.velocity += wallRunJumpImpulse * move.normalized;

        undoWallRun();

        // timeout wallruns 
        StartCoroutine(timeoutWallRun());

        return true;

    }

    public void wallJumpBackBoost()
    {
        // check
        Collider[] overlap  = Physics.OverlapSphere(wallCheckB.position, wallRunDistance, wallRunMask);
        if (overlap.Length != 1)
            return;

        // do boost
        Vector3 move = (transform.forward) * wallRunJumpImpulse;
        move.y = move.magnitude / 2;

        player.velocity += wallRunJumpImpulse * move.normalized;

    }







    private void AllignPlayerToRunningDir()
    {
        Vector3 velocityXZ = new Vector3(player.velocity.x, 0f, player.velocity.z).normalized;
        float dot = Vector3.Dot(velocityXZ, runningDir);
        velocityXZ *= dot;
        player.velocity.x = velocityXZ.x;
        player.velocity.z = velocityXZ.z;
    }

    // also rotates camera smoothly
    private IEnumerator recordDirNextFrame()
    {
        Vector3 startpos = new Vector3(transform.position.x, 0f, transform.position.z);
        yield return new WaitForSeconds(transitionTime_s);
        runningDir = (new Vector3(transform.position.x, 0f, transform.position.z) - startpos).normalized;
        runningNormal = Vector3.Cross(transform.up, runningDir).normalized;
        AllignPlayerToRunningDir();
        updateReady = true;
    }

    private IEnumerator timeoutWallRun()
    {
        wallRunTimedOut = true;
        yield return new WaitForSeconds(wallRunTimeout);
        wallRunTimedOut = false;
    }

    private IEnumerator rotateCameraZ(float start, float end)
    {
        float curtime = 0f;
        
        while(curtime < transitionTime_s)
        {
            curtime += Time.deltaTime;
            float perc = Mathf.Clamp(curtime / transitionTime_s, 0f, 1f);
            float angle =  Mathf.Lerp(start, end, perc);
            Debug.Log(angle);
            FPSCamera.transform.localRotation = Quaternion.Euler(FPSCamera.transform.localEulerAngles.x, 0f, angle);
            yield return null;
        }
        
    }
}
