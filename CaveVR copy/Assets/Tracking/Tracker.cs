using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using SimpleJSON;

public static class Encoding {
	public static byte[] toByteArray(string str) {
		byte[] bytes = new byte[str.Length * sizeof(char)];
		System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
		return bytes;
	}
}

[System.Serializable]
public class Tracker_Obj {

	[HideInInspector]
	public string id;
	
	public float x;
	public float y;
	public float z;
	
	public float h;
	public float p;
	public float r;
	
	
	public Tracker_Obj(string id_) {
		id = id_;
	}
}	

public class Tracker : MonoBehaviour {
	
	UdpClient client;
	IPEndPoint ep;
	
	public Dictionary<string, Tracker_Obj> objs;
	public List<Tracker_Obj> serialized_objs;
	
	// Use this for initialization
	void Start () {
		
		objs = new Dictionary<string, Tracker_Obj>();
		serialized_objs = new List<Tracker_Obj>();
		
		// Servers recieve messages
		// Clients send messages
		
		Debug.Log ("Initializing Client");
		client = null;
		try { client = new UdpClient(42069); }
		catch (Exception e) { Console.WriteLine(e.ToString()); return;} 
		
		Debug.Log ("Requesting Tracker Access");
		ep = new IPEndPoint(IPAddress.Parse("129.161.12.88"), 42068);
		client.Connect(ep);
		
		byte[] msg = System.Text.Encoding.UTF8.GetBytes("ADD ME");
		client.Send(msg, msg.Length);	
		
		Thread recv = new Thread(new ThreadStart(Recv));
		recv.Start();
	}
	
	void Recv() {
		Debug.Log ("Listening");
		while (true) {
			try {
				if (client.Available > 0) {	
					string raw_msg = System.Text.Encoding.ASCII.GetString(client.Receive(ref ep));
					var msg = JSON.Parse (raw_msg);

					string code = msg["code"];

					if (code == "0") {
						Debug.Log ("No Objects Transmitted");
						return;
					}

					string id = msg["id"];
					
					if (!objs.ContainsKey(id)) {
						Tracker_Obj new_obj = new Tracker_Obj(id);
						objs[id] = new_obj;
						serialized_objs.Add(new_obj);
					}
					
					Tracker_Obj obj = objs[id];

					obj.x = float.Parse (msg["pos"][0]);
					obj.y = float.Parse (msg["pos"][1]);
					obj.z = float.Parse (msg["pos"][2]);
					
					obj.h = float.Parse (msg["hpr"][0]);
					obj.p = float.Parse (msg["hpr"][1]);
					obj.r = float.Parse (msg["hpr"][2]);

				}
			}
			catch {
				Debug.Log ("Error in message handling");
			}
		}
	}
}