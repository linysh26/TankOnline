using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Tank : NetworkBehaviour {

	public bool isRunning = true;

	public int HP = 100;
	public float speed = 20.0f;
	public float speedX;
	public float speedY;

	//mark for navigation
	public GameObject mark;

    //status
    public int index = 2;               //坦克的状态

    //particle
	public ParticleSystem steam;        //状态正常
	public ParticleSystem smoke;        //破损
	public ParticleSystem fire;         //无法行动

    //fire
    public float bulletHealTime = 1.0f;       //冷却时间
    public float lastFireTime = 0;      //上次发射时间
    public int bulletATK = 20;          //普通攻击伤害

    //special fire
    public float rocketHealTime = 2.0f; //火箭攻击冷却时间
    public int rocketsCounter = 5;      //火箭指示物
    public int rocketsATK = 50;         //火箭攻击伤害

	//pibe
	public Transform[] parts;				//坦克部件

	//home
	public Vector3 home;

    void Start () {
		HP = 100;
		parts = this.GetComponentsInChildren<Transform> ();
		this.GetComponent<Rigidbody> ().isKinematic = false;

		//get home position
		home = this.transform.position;

		//initiate the target for navigation
		mark = Instantiate (Resources.Load ("mark")) as GameObject;
		mark.transform.position = this.transform.position;

		//initiate steam prefab
		GameObject Steam = Instantiate (Resources.Load ("Steam"), this.transform) as GameObject;
		steam = Steam.GetComponent<ParticleSystem> ();
		steam.transform.position = this.transform.position + new Vector3(0, 0.6f, 0);
		//initiate smoke prefab
		GameObject Smoke = Instantiate (Resources.Load ("Smoke"), this.transform) as GameObject;
		smoke = Smoke.GetComponent<ParticleSystem> ();
		smoke.transform.position = this.transform.position + new Vector3(0, 0.6f, 0);
		//initiate fire prefab
		GameObject Fire = Instantiate (Resources.Load ("FireMobile"), this.transform) as GameObject;
		fire = Fire.GetComponent<ParticleSystem> ();
		fire.transform.position = this.transform.position + new Vector3(0, 0.6f, 0);

		//set initial status of particle systems
		steam.Play ();
		smoke.gameObject.SetActive (false);
		fire.gameObject.SetActive (false);

		//camera and cursor
		speedY = 50.0f;
		speedX = 10.0f;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}
	
	void Update () {
		if (!isLocalPlayer) {
			return;
		}
		if (!isRunning) {
			return;
		}
        //移动
		float translationY = Input.GetAxis ("Horizontal");
		float translationX = Input.GetAxis ("Vertical");
		Vector3 nextPosition = this.transform.position;
		Vector3 forward = Camera.main.transform.forward;
		Vector3 right = Camera.main.transform.right;
		right.y = 0;
		forward.y = 0;
		nextPosition += translationX * forward + translationY * right;
		if (nextPosition != this.transform.position) {
			Move (nextPosition);
			CmdLookAt (nextPosition);
		}
		// cursor
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
		// camera
		translationX = Input.GetAxis ("Mouse X") * speedX;
		translationX *= Time.deltaTime;
		if (translationX != 0) {
			CmdRotateCamera (translationX);
			RotateCamera (translationX);
		}
        // attack
        if (Input.GetMouseButtonDown(0) && Time.time >= lastFireTime + bulletHealTime)
        {
            lastFireTime = Time.time;
			CmdAttack ();
        }
        else if (Input.GetMouseButtonDown(1) && rocketsCounter > 0 && Time.time >= lastFireTime + rocketHealTime)
        {
            lastFireTime = Time.time;
            CmdSpecialAttck();
        }
        // 破损
		if (HP < 50 && HP > 10 && index == 2) {
			Instantiate (Resources.Load ("Explosion5"), this.transform.position, Quaternion.identity);
			this.smoke.gameObject.SetActive (true);
			this.steam.gameObject.SetActive (false);
			index--;
		} else if (HP <= 10 && index == 1) {
			Instantiate (Resources.Load ("Explosion6"), this.transform.position, Quaternion.identity);
			this.fire.gameObject.SetActive (false);
			index--;
		} 
		else {
			steam.gameObject.SetActive (true);
			smoke.gameObject.SetActive (false);
			fire.gameObject.SetActive (false);
		}
    }
    // commands are sent from player objects on the client to player objects on the server
	public void Move(Vector3 nextPosition){
		NavMeshAgent NMAgent = mark.GetComponent<NavMeshAgent>();
		// 车身朝向
		parts [5].LookAt (new Vector3(nextPosition.x, parts[5].transform.position.y, nextPosition.z));
		// 车身移动
		NMAgent.SetDestination(nextPosition);
		this.transform.position = mark.transform.position;
	}
	[Command]
	public void CmdLookAt(Vector3 nextPosition){
		parts [5].LookAt (new Vector3(nextPosition.x, parts[5].transform.position.y, nextPosition.z));
	}

	//camera behaviour
	[Command]
	public void CmdRotateCamera(float translationX){
		//global mouse behaviours
		if (!Cursor.visible) {
			parts [1].transform.RotateAround (this.transform.position, Vector3.up, translationX * speedX);
		}
	}
	public void RotateCamera(float translationX){
		//global mouse behaviours
		if (!Cursor.visible) {
			parts [1].transform.RotateAround (this.transform.position, Vector3.up, translationX * speedX);
		}
	}
    // 通常弹
	[Command]
	public void CmdAttack()
    {
		//initiate bullet prefab
		GameObject bullet = Instantiate (Resources.Load ("bullet")) as GameObject;
		bullet.transform.position = new Vector3 (parts[1].position.x, parts[1].position.y, parts[1].position.z) + parts[1].forward * 3.0f;
		bullet.transform.rotation = parts [1].rotation;
		this.speed = speed;
		//set bullet atk
		CollisionController bulletCC = bullet.GetComponent<CollisionController>();
		bulletCC.ATK = bulletATK;
		//spawn bullet on client
		NetworkServer.Spawn (bullet);

		// set automatically detroy after 2.0 seconds
		Destroy (bullet, 2.0f);
    }
    // 火箭炮
	[Command]
	public void CmdSpecialAttck()
    {
		//initiate rocket prefab
		GameObject rocket = Instantiate (Resources.Load ("rocket")) as GameObject;

		//set rocket atk
		CollisionController rocketCC = rocket.GetComponent<CollisionController> ();
		rocketCC.ATK = rocketsATK;

        //移除一个火箭指示物，发动效果：这张卡在回合结束前可以发动一次火球
        rocketsCounter--;

		//spawn rocket on client
		NetworkServer.Spawn (rocket);

		//set automatically destroy
		Destroy (rocket, 2.0f);
    }

	// ClientRpc are sent from objects on server to client objects
	[ClientRpc]
	void RpcRespawn(){
		if (isLocalPlayer) {
			this.transform.position = home;
		}
	}
}
