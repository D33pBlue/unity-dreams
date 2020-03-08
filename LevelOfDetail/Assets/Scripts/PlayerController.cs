using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerController : MonoBehaviour
{

	public Transform PlayerBody;
	public Transform PlayerCamera;
	public Transform SpawnPointInside;
	public Transform SpawnPointOutside;
	public CharacterController controller;
	public Transform GroundCheck;
	public LayerMask groundMask;
	float groundDistance = 0.4f;
	float mouseSensitivity = 200;
	float speed = 8;
	float gravity = -9.81f;
	float jumpHeight = 3;
	Vector3 velocity;
	float xRotation = 0;
	bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if(hit.gameObject.name=="contactpt"){
			controller.enabled = false;
			PlayerBody.position = SpawnPointInside.position;
			controller.enabled = true;
		}
		if(hit.gameObject.name=="contactpt2"){
			controller.enabled = false;
			PlayerBody.position = SpawnPointOutside.position;
			controller.enabled = true;
		}
	}

    // Update is called once per frame
    void Update()
    {
    	isGrounded = Physics.CheckSphere(GroundCheck.position,
    			groundDistance,groundMask);
    	if(isGrounded&&velocity.y<0){
    		velocity.y = -2;
    	}
    	// update rotation
        float mouseX = Input.GetAxis("Mouse X")*mouseSensitivity*Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y")*mouseSensitivity*Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation,-45.0f,45.0f);
        PlayerCamera.localRotation = Quaternion.Euler(xRotation,0,0);
        PlayerBody.Rotate(0,mouseX,0);
        // update motion
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = PlayerBody.right*x+PlayerBody.forward*z;
        controller.Move(move*speed*Time.deltaTime);
        //update jump
        if(Input.GetButtonDown("Jump")&&isGrounded){
        	velocity.y = Mathf.Sqrt(jumpHeight*(-1)*gravity);
        }
        // update gravity
        velocity.y += gravity*Time.deltaTime;
        controller.Move(velocity*Time.deltaTime);
    }
}
