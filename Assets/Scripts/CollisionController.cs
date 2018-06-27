using UnityEngine;
using System.Collections;

public class CollisionController : MonoBehaviour {

	public float speed = 10.0f;
	public int ATK = 10;
	public Collision collision;
	// Use this for initialization
	void Start () {
		
	}

	void Update(){
		this.GetComponent<Rigidbody> ().velocity = this.transform.TransformDirection (Vector3.forward) * speed;
	}

	void OnCollisionEnter(Collision collision){
		if (collision.gameObject.GetComponent<HP> () != null) {
			Tank tank = collision.gameObject.GetComponent<Tank> ();
			HP hp = tank.GetComponent<HP> ();
			hp.getDamage (ATK);
			if (hp.current_hp > 0) {
				tank.HP -= ATK;
			} 
			else {
				tank.isRunning = false;
				Instantiate (Resources.Load ("Explosion1"), tank.transform.position, Quaternion.identity);
			}
		}
		Destroy (this.gameObject);
		Instantiate (Resources.Load ("Explosion8"), this.transform.position, Quaternion.identity);
	}
}
