using UnityEngine;
using System.Collections;

public class moveMe : MonoBehaviour {
	public Vector3 tempPos;
	public KinectTrackPoints.NodeType bindNode;

	// Use this for initialization
	void Start () {
	}
	// Update is called once per frame
	void Update () {
		tempPos = KinectTrackPoints.getNode(KinectTrackPoints.NodeType.Head)*2.0f;

		this.transform.position = Vector3.Scale(tempPos, new Vector3 (0f, 0f, 5f)); 

//		print ("x"+ this.transform.position.x);
//		print ("y"+ this.transform.position.y);
//		print ("z"+ this.transform.position.z);

	}
}
