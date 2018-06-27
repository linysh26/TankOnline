using UnityEngine;
using System.Collections;
using UnityEngine.Networking;


public class UserInterface : NetworkBehaviour {

	public float speedX;
	public float speedY;

	// Use this for initialization
	void Start () {
		speedY = 50.0f;
		speedX = 10.0f;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	void Update(){
		if (!isLocalPlayer)
			return;
		//global key behaviours
		if (Input.GetKeyDown(KeyCode.BackQuote)) {
			Cursor.visible = !Cursor.visible;
			if(Cursor.lockState == CursorLockMode.None)
				Cursor.lockState = CursorLockMode.Locked;
			else
				Cursor.lockState = CursorLockMode.None;
		}
		//global mouse behaviours
		if (!Cursor.visible) {
			float translationX = Input.GetAxis ("Mouse X") * speedX;
			translationX *= Time.deltaTime;
			this.transform.RotateAround (this.transform.parent.position, Vector3.up, translationX * speedX);
		}
	}

	void OnGUI(){
		
	}
}
