using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //Movement variables
    public float moveSpeed = 5f;
    public float sprintFactor = 1.5f;
    private float sprintSpeed;  //To store the math for sprinting

   

    //Default Unity Physics.gravity. Which is (0, -9.81, 0)
    //public Vector3 gravity;
    public float lowJumpFraction = 0.5f;    //A tap vs a hold will be 1/2 as much
    public float fastFallMultiplier = 1.8f; //Fall quicker than ascend
    private bool isGrounded;

    //A helper variable to store velocity math for us
    private Vector3 velocity;

    //Camera stuff!
    [Header("DRAG THE CAMERA OR BE VERY DIZZY")]
    public Transform cameraTransform;

    //Default mouse movement is 1; far too slow.
    public float mouseSensitivity = 5;
    private float verticalLookRotation = 0; //Helper variable for current rotation

    //Component variable for the CharacterController
    private CharacterController controller;


    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();

        //Set the initial sprint speed
        sprintSpeed = moveSpeed * sprintFactor;

        //Hide the cursor. ESC to bring it back
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //Once per  frame, call the custom functions
        MouseLook();
        Movement();

        Debug.Log(Physics.gravity);
    }

    void MouseLook()
    {
        //Mouse X and Mouse Y are built in to the Input system
        //Defaults are -1, 0, or 1 which is too slow for our demo
        //So, we scale by mouseSensitivity!
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        //Only the camera can move up and down
        verticalLookRotation -= mouseY;
        //Make sure the camera can't go lower than -85 or higher than +85
        //Otherwise, it can rotate infinitely.
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -85f, 85f);

        //REMEMBER: The number is the axis that the object is rotating around,
        //not the direction of the rotation. i.e., we are moving the camera left
        //and right, so that is rotating on the x axis.
        cameraTransform.localRotation = Quaternion.Euler
                                        (verticalLookRotation, 0, 0);
        //Player rotates on the y axis. Vector3.up = (0, 1, 0)
        transform.Rotate(Vector3.up * mouseX);
    }

    void Movement()
    {
        //The CharacterController has a built-in isGrounded function
        //BUT I created my own variable, so we can control when it updates
        isGrounded = controller.isGrounded;

        //Make sure the player stays on the ground. This is mostly a failsafe.
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2;
        }

        //Input with floats, as we did before.
        //W and S for up and down, A and S for left and right
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        //Calculate the target movement direction
        Vector3 move = transform.right * x + transform.forward * z;

        //Calculate current speed
        float currentSpeed = moveSpeed;

        //If the player holds our sprint button and is already moving...
        if (Input.GetKey(KeyCode.LeftControl) && Mathf.Abs(z) > 0.1f)
        {
            currentSpeed = sprintSpeed;
        }

        //Go there!
        //We have already calculated where to go and how fast.
        controller.Move(move * currentSpeed * Time.deltaTime);

        //If the player is in the air and not pressing spacebar, they
        //tapped the button, so it's a shorter jump
        if (velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            //lowJumpFraction is "extra Physics.gravity" applied for a short jump
            //0 < the calculated value < 1)
            velocity.y += Physics.gravity.y * lowJumpFraction * Time.deltaTime;
        }

        //Extra Physics.gravity while falling
        if (velocity.y < 0)
        {
            //Physics.gravity.y * 1.75 - 1
            velocity.y += Physics.gravity.y * (fastFallMultiplier - 1) * Time.deltaTime;
        }

        //Apply those calculation to the player's velocity
        velocity.y += Physics.gravity.y * Time.deltaTime;

        //FINALLY! Calculate the final movement and do it.
        Vector3 moveCalculation = move * currentSpeed +
            Vector3.up * velocity.y;

        controller.Move(moveCalculation * Time.deltaTime);
    }
}
