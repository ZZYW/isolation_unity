using UnityEngine;
using System.Collections;

public class moveMe : MonoBehaviour {
	public Vector3 pos;
	public KinectTrackPoints.NodeType bindNode;

	// Use this for initialization
	void Start () {
	}
	// Update is called once per frame
	void Update () {
		this.transform.position = KinectTrackPoints.getNode(KinectTrackPoints.NodeType.Head);
//		pos.x = Input.mousePosition.x;
//		pos.y = Input.mousePosition.y;
//		print ("x"+pos.x);
//		print ("y"+pos.y);
//		print ("z"+pos.z);
//		transform.position = pos;
	}
}
