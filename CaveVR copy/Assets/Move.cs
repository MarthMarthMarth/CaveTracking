using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour {

	public Tracker tracker;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Debug.Log(tracker.objs["Wand"].x);
		transform.position = new Vector3(1000 * tracker.objs["Wand"].x, 1000 * tracker.objs["Wand"].y, 1000 * tracker.objs["Wand"].z);
	}
}
