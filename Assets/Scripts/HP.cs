using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class HP : NetworkBehaviour {

	//reference
	public GameObject canvas;
	public GameObject healthPanel;
	private Text[] texts;
	private Slider hpSlider;

	//parameter
	public float offsetX = 0;
	public float offsetY = 1;
	public float offsetZ = 0;

	//define as synchronized parameter
	[SyncVar]
	public int current_hp;
	public int max_hp;

	//renderer
	private Renderer[] selfRenderer;
	private CanvasGroup canvasGroup;

	// Use this for initialization
	void Start () {
		// initialize canvas
		canvas = Instantiate(Resources.Load("canvas")) as GameObject;
		//initialize reference
		texts = new Text[2];
		healthPanel = Instantiate (Resources.Load ("hp")) as GameObject;
		healthPanel.transform.SetParent (canvas.transform, false);
		texts = healthPanel.GetComponentsInChildren<Text> ();
		hpSlider = healthPanel.GetComponentInChildren<Slider> ();

		//initialize parameters
		string name = "Player";
		max_hp = 100;
		current_hp = max_hp;
		texts [0].text = name;
		texts [1].text = max_hp + "/" + max_hp;

		//initialize something about renderer
		selfRenderer = this.GetComponentsInChildren<Renderer> ();
		canvasGroup = this.GetComponent<CanvasGroup> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (hpSlider == null) {
			Debug.Log ("NULL");
			return;
		}
		hpSlider.value = (float)current_hp / (float)max_hp;
		texts[1].text = current_hp + "/" + max_hp;

		Vector3 worldPos = new Vector3 (transform.position.x + offsetX, transform.position.y + offsetY, transform.position.z + offsetZ);
		Vector3 screenPos = Camera.main.WorldToScreenPoint (worldPos);
		healthPanel.transform.position = new Vector3 (screenPos.x, screenPos.y, screenPos.z);
	}

	void OnGUI(){
		
	}

	public void getDamage(int damage){
		if (!isServer)
			return;
		
		current_hp -= damage;
		current_hp = current_hp < 0 ? 0 : current_hp;
		if (current_hp == 0) {
			healthPanel.SetActive (false);
		}
	}

	public void Restart(){
		current_hp = max_hp;
		healthPanel.SetActive (true);
	}
}
