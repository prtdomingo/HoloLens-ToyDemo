using UnityEngine;
using System;
using System.Collections;


[RequireComponent(typeof(Gamepad_Client))]
[RequireComponent(typeof(Rigidbody))]
public class SimpleCharacterController: MonoBehaviour
{
	public		float				speed		= 10f;
	public		float				jumpSpeed	= 20f;

	private		Gamepad_Client		gamepad;	
	private		Rigidbody			rigid_body;
	private		bool				isFalling	= true;
	

	void Start()
	{
		gamepad		= GetComponent<Gamepad_Client>();
		rigid_body	= GetComponent<Rigidbody>();
	}


	void Update()
	{
		if(gamepad && rigid_body)
		{
			// target the first gamepad found
			int id = 0;

			// left thumb stick steers
			Vector3 velocity	= rigid_body.velocity;
					velocity.x = speed * gamepad.controllers[id].thumb_Lx;
					velocity.z = speed * gamepad.controllers[id].thumb_Ly;
			rigid_body.velocity = velocity;

			// A button jumps
			if(gamepad.controllers[id].button_A && !isFalling)
			{
				rigid_body.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
				isFalling = true;
			}

			// Triggers set vibration
			gamepad.SetVibrate(id, gamepad.controllers[id].trigger_L, gamepad.controllers[id].trigger_R);
		}
	}


	// when we touch something we're not falling anymore
	void OnCollisionStay(Collision collisionInfo)
	{
		isFalling = false;
	}
		  
}
