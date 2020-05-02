using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    private PlayerMovement player;
    private CharacterController controller;

    public float slideBoost = 1.2f; // speed multiplier
    public float slideBoostMax = 5f; // max magnitude for boost to actually boost
    public float slideFriction = 3f;
    public float slideHeight = 0.5f; // scale to set
    public float slideTurnControl = 1.0f;

    public float transitionTime_s = 0.1f;

    private Transform trans;

    bool updateReady;
    bool entered; //prevent entries before exits

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        player = GetComponent<PlayerMovement>();
        trans = GetComponent<Transform>();

        updateReady = false;
        entered = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void slideUpdate()
    {
        if (!updateReady)
            return;

        // Step1. apply friction
        player.velocity.x /= slideFriction;
        player.velocity.z /= slideFriction;

        // Step 2 redirect velocity to camera direction
        Vector3 move = transform.forward * player.velocity.magnitude;
        Vector3 newvel = Vector3.Lerp(player.velocity, move, slideTurnControl);
        player.velocity = newvel;

        Debug.Log(newvel.magnitude);

        // Step 3 check exit condition
        float magXY = new Vector3(player.velocity.x, 0f, player.velocity.z).magnitude;
        if (magXY < 0.5f)
        {
            StartCoroutine(transitionSlide(slideHeight, 1f));
            Debug.Log("exit too slow");
            

            entered = false;
            player.isSliding = false;
        }
    }

    public bool jumpFromSlide()
    {
        if (!updateReady)
            return false;

        if (!exitSlide())
            return false;
        // add jump influence from camera direction
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        player.JumpFromGrounded(x, y);

        
        return true;
    }

    public void enterSlide()
    {
        if (entered)
            return;
        // shrink player height
        updateReady = false;
        StartCoroutine(transitionSlide(1f, slideHeight));

        // boost velocity
        float velXY = new Vector3(player.velocity.x, 0f, player.velocity.z).magnitude;
        if (velXY < slideBoostMax)
        {
            player.velocity.x *= slideBoost;
            player.velocity.z *= slideBoost;
        }
        Debug.Log("Enter Slide");
        player.isSliding = true;
        entered = true;
    }

    public bool exitSlide()
    {
        if (!entered || !updateReady)
        {
            Debug.Log("prevented");
            return false;
        }
        StartCoroutine(transitionSlide(slideHeight, 1f));
        Debug.Log("exit Slide");

        entered = false;
        return true;
    }

    public bool checkSlide()
    {
        bool slidePressed = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C);

        if (player.isWallRunning)
            return false;


        // check enter slide
        if (slidePressed && !player.isSliding)
        {
            return true;
        }

        return false;
    }


    private IEnumerator transitionSlide(float oldHeight, float newHeight)
    {
        float curtime = 0f;

        while (curtime < transitionTime_s)
        {
            curtime += Time.deltaTime;
            float perc = Mathf.Clamp(curtime / transitionTime_s, 0f, 1f);
            float height = Mathf.Lerp(oldHeight, newHeight, perc);
            transform.parent.localScale = new Vector3(1f, height, 1f);
            yield return null;
        }
        updateReady = true;
    }
}
