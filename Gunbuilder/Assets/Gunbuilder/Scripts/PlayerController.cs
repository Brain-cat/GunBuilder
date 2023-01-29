using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //defined in editor
    [Header("Player")]
    public GameObject playerObj;
    public GameObject playerMesh;

    [Header("Camera")]
    public float mouseSens;
    public GameObject playerCameraHolder;
    public Transform cameraPos;
    public Camera PlayerCamera;
    public float CameraFOV;

    [Header("Movement")]
    public float setMovementSpeed;
    public float sprintSpeed;
    public Rigidbody playerRigidbody; //unity component, 
    public float playerHeight; //this is for raycasting to the ground to see if grounded.
    public float playerHeightOffset;
    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float airMoveMultiplier;
    public float fallSpeed;

    [Header("Crouching")]
    public float crouchHeight; // needs to be between 0 and 1
    public float crouchSpeed;

    //defined in script
    float yRotation;
    float xRotation;

    float movementSpeed;
    float xMovementInput;
    float zMovementInput;
    Vector3 findMoveDirection; //for storing x and z
    Vector3 moveDirection; // for storing slope angles y and 
    bool isGrounded;
    bool canStand;
    bool isCrouching;
    bool alreadyCrouching = false;
    float oldCapHeight;
    float oldMeshHeight;
    float oldMeshPos;
    RaycastHit groundHit; // stores type of ground standing on

    // Start is called before the first frame update
    void Start()
    {
        //locks cursor and makes it invis, might hav to change cursor visible later for ingame menu.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerRigidbody.freezeRotation = true; //prevents rb from rotating and flopping around (could turn on for death).

        movementSpeed = setMovementSpeed;

        PlayerCamera.fieldOfView = CameraFOV;
    }

    //Called every frame
    void Update()
    {
        //shoots invis laser down to see if ground is there,
        if (!alreadyCrouching)
            isGrounded = Physics.Raycast(playerObj.transform.position + new Vector3(0f,playerHeight,0f), Vector3.down, out groundHit, playerHeight + playerHeightOffset);
        else //changes raycast length to crouching height
            isGrounded = Physics.Raycast(playerObj.transform.position + new Vector3(0f, playerHeight, 0f), Vector3.down, out groundHit, crouchHeight + playerHeightOffset);

        //shoots invis laser up to see if something is blocking standing,, might have to change to colider sphere can still clip through edges while walking out
        canStand = Physics.Raycast(playerObj.transform.position + new Vector3(0f, playerHeight, 0f), Vector3.up, playerHeight + playerHeightOffset);

        CameraControl();
        GetMovementInput();

        //sets drag if the player is grounded or not
        if (isGrounded)
            playerRigidbody.drag = groundDrag; //want to curve this
        else
        {
            playerRigidbody.drag = 0f;
            playerRigidbody.AddForce(Vector3.down * fallSpeed * playerRigidbody.mass * Time.fixedDeltaTime, ForceMode.Force); //will activily pull user down to work around floaty falling
        }

        SpeedControl();
    }

    //we put in here cus rigidbody works on a fixed framerate
    void FixedUpdate()
    {
        PlayerMovement();
    }

    void CameraControl() 
    {
        //moves the actual camera to empty obj on body, is a work around with buggy/ stuttering camera movement with rigedbody movement
        playerCameraHolder.transform.position = cameraPos.position;

        //getting mouse input, AxisRaw returns an unsmoothed value for precision, fixedDeltaTime is workaround for fluctuating framerate and ridigbody body cam stuttering (update is called per frame).
        //Mouse X and Mouse Y in getaxis are defined in the unity editor.
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.fixedDeltaTime * mouseSens * 10;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.fixedDeltaTime * mouseSens * 10;

        //idk the math here but do flip + - for inverted mouse control.
        yRotation += mouseX;
        xRotation -= mouseY;

        //clamps up and down rotation to 90 so we dont do sick flips.
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCameraHolder.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0); //rotates camera up and down, Quaterion Euler is just what we have to use for rotation.
        playerMesh.transform.rotation = Quaternion.Euler(0, yRotation, 0); //rotates the players mesh body so not to move the cameraPos, moving cameraPos can cause jittering while looking around
    }

    void GetMovementInput() 
    {
        //gets our inputs from unity editor unsmoothed.
        xMovementInput = Input.GetAxisRaw("Horizontal");
        zMovementInput = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Jump") && isGrounded) //checks if user is on ground and pressed jump
            Jump(); //jump, u jump

        if (Input.GetButton("Sprint") && isGrounded || movementSpeed > setMovementSpeed && !isGrounded) // checks if user is on ground and pressed sprint or is not on ground but was sprinting
            movementSpeed = sprintSpeed; // sets movement speed to the sprint speed // will maintain our velocity in the air too if we were sprinting before
        else if (!Input.GetButton("Sprint") && !alreadyCrouching) 
            movementSpeed = setMovementSpeed; // if not pressing shift sets back to base speed

        if (Input.GetButton("Crouch"))
        {
            //transform.localScale = new Vector3(transform.localScale.x, crouchHeight, transform.localScale.z);
            isCrouching = true;
            Crouching();
        }
        if (!Input.GetButton("Crouch") && alreadyCrouching)
            //transform.localScale = new Vector3(transform.localScale.x, playerHeight, transform.localScale.z);
            Standing(); isCrouching = false;
    }

    void PlayerMovement() 
    {
        //finds the movement direction letting us move in the direction we're facing using the playerMesh's rotation, not 100% sure on the math
        findMoveDirection = playerMesh.transform.forward * zMovementInput + playerMesh.transform.right * xMovementInput;
        moveDirection = Vector3.ProjectOnPlane(findMoveDirection, groundHit.normal).normalized;

        //moves the rigidbody in the direction we want to move with our inputs, movespeed is to adjust how fast we go, forcemode will constantly apply that force 
        if (isGrounded)
            playerRigidbody.AddForce(moveDirection.normalized * movementSpeed * playerRigidbody.mass * 10f, ForceMode.Force); //movement on the ground

        else if (!isGrounded)
            playerRigidbody.AddForce(moveDirection.normalized * movementSpeed * playerRigidbody.mass * 10f * airMoveMultiplier, ForceMode.Force); // movement in the air
    }

    void SpeedControl() 
    {
        //gets how fast our obj is going
        Vector3 objVelocity = new Vector3(playerRigidbody.velocity.x,0f,  playerRigidbody.velocity.z);

        //checks if obj movement speed is faster than our set speed (magnitude is just taking all the velocity values to calculate the speed)
        if (objVelocity.magnitude > movementSpeed) 
        {
            //limits the speed
            Vector3 speedLimit = objVelocity.normalized * movementSpeed;
            playerRigidbody.velocity = new Vector3(speedLimit.x, playerRigidbody.velocity.y, speedLimit.z);
        }
    }

    void Jump() 
    {
        playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0f, playerRigidbody.velocity.z); //resets jump speed to 0

        playerRigidbody.AddForce(transform.up * jumpForce * playerRigidbody.mass, ForceMode.Impulse); //Pushing the ridgidbody up to jump, Impulse is a large amount of force in a short time
    }

    void Crouching() 
    {
        //here is the funky math for this
        // Cy collider centre y                 Cy = 1 - crouchHeight + Cy
        // H collider height                    H = H  * crouchHeight
        // Py mesh pos y                        Py = 1 - crouchHeight + Py
        // Sy mesh scale y                      Sy = crouchHeight

        if (isCrouching == true && alreadyCrouching == false)
        {
            //stores old values for standing
            oldCapHeight = GetComponent<CapsuleCollider>().height;
            oldMeshHeight = playerMesh.transform.localScale.y; 
            oldMeshPos = playerMesh.transform.localScale.y; 

            float newCapHeight;    //capsual height
            float newMeshPos;   //mesh posistion y

            newCapHeight = GetComponent<CapsuleCollider>().height * crouchHeight; //sets new capsual collider height using set crouching height
            newMeshPos = 1 - crouchHeight + playerMesh.transform.localPosition.y; //sets new mesh y possision which will also be used for the capsual collider y centre 

            GetComponent<CapsuleCollider>().center = new Vector3(GetComponent<CapsuleCollider>().center.x, newMeshPos, GetComponent<CapsuleCollider>().center.z); // moves capsual collider up
            GetComponent<CapsuleCollider>().height = newCapHeight; // squishes the capsual colider
            playerMesh.transform.localPosition = new Vector3(playerMesh.transform.localPosition.x, newMeshPos, playerMesh.transform.localPosition.z); //moves the player mesh up
            playerMesh.transform.localScale = new Vector3(playerMesh.transform.localScale.x, crouchHeight, playerMesh.transform.localScale.z); // squishes the mesh

            alreadyCrouching = true;
        }

        //changes speed to crouch speed while if touching ground
        if (isGrounded)
            movementSpeed = crouchSpeed;
    }

    void Standing() 
    {
        if (!canStand)
        {
            //returns all values back to their original value
            GetComponent<CapsuleCollider>().center = new Vector3(GetComponent<CapsuleCollider>().center.x, oldMeshPos, GetComponent<CapsuleCollider>().center.z);
            GetComponent<CapsuleCollider>().height = oldCapHeight;
            playerMesh.transform.localPosition = new Vector3(playerMesh.transform.localPosition.x, oldMeshPos, playerMesh.transform.localPosition.z);
            playerMesh.transform.localScale = new Vector3(playerMesh.transform.localScale.x, oldMeshHeight, playerMesh.transform.localScale.z);
            movementSpeed = setMovementSpeed;
            alreadyCrouching = false;
        }
    }
}
