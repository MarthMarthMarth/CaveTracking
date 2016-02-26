using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;


// Data Classes
public class Motion {
	public List<Orientation> steps;
	private int frame;
	public bool loop;

	public Motion() {
		steps = new List<Orientation>();
		frame = 0;
		loop = true;
	}

	public void Animate(GameObject obj) {
		frame = (frame + 1) % steps.Count;
		obj.transform.position = steps[frame].pos;
		obj.transform.rotation = steps[frame].rot;
	}
}
public class Orientation {
	public Vector3 pos;
	public Quaternion rot;
	public Orientation(Transform transform) {
		pos = transform.position;
		rot = transform.rotation;
	}
}
	
// Monobehavior
public class MotionControl : MonoBehaviour {
	public int frame;
	public bool recording;
	public Motion record, animation;

	void Start() {
		record = null;
		animation = null;
		frame = 0;
	}

	void Update() {
		if (animation != null) {
			animation.Animate(gameObject);
		}
	}
}

// Editor
[CustomEditor(typeof(MotionControl))]
public class MotionEditor : Editor {

	public Dictionary<String, Motion> motions;
	public MotionControl motionControl; 

	void OnEnable() {
		motions = new Dictionary<String, Motion>();
		motionControl = (MotionControl)target;
	}

	public override void OnInspectorGUI() {

		foreach (KeyValuePair <string, Motion> entry in motions) {
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label(entry.Key);
			if (GUILayout.Button("Play", GUILayout.Width(50))) {
				motionControl.animation = entry.Value;
			}
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.Space();

		if(GUILayout.Button("New", GUILayout.Width(50))) {
			MotionBuilder.Show(this);
		}
	}
}

// Motion Builder Window
public class MotionBuilder : EditorWindow {
	
	MotionEditor editor;
	Motion motion;
	string name;
	bool recording;

	void OnEnable() {
		motion = new Motion();
		recording = false;
		maxSize = new Vector2(225, 50);
	}

	void Update() {
		// Record the frames of motion
		if (recording) {
			motion.steps.Add(new Orientation(editor.motionControl.gameObject.transform));
		}
	}

	void OnGUI() {

		GUI.Label(new Rect(5, 4, 80, 18), "Enter a name:");

		// Assign name from text field
		name = EditorGUI.TextField(new Rect(85, 4, 125, 15), name);
	
		if (!recording) {
			// Begin Recording
			if (GUI.Button(new Rect(5, 28, 50, 18), "Record")) {
				motion = new Motion();
				recording = true;
			}
			// Allow Submission of a Recording
			if (motion.steps.Count > 0) {
				if (GUI.Button(new Rect(60, 28, 50, 18), "Submit")) {
					editor.motions.Add(name, motion);
					editor.motionControl.animation = null;
					this.Close();
				}
			}
		}
		else {
			// End Recording
			if (GUI.Button(new Rect(5, 28, 50, 18), "Stop")) {
				recording = false;
				editor.motionControl.animation = motion;
			}
		}
		// Cancel Motion Creation
		if (GUI.Button(new Rect(170, 28, 50, 18), "Cancel")) {
			editor.motionControl.animation = null;
			this.Close();
		}
	}

	public static void Show(MotionEditor _editor) {
		MotionBuilder window = (MotionBuilder)EditorWindow.GetWindow(typeof(MotionBuilder));
		window.editor = _editor;
	}
}

