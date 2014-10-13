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
		tempPos = KinectTrackPoints.getNode(KinectTrackPoints.NodeType.LeftHand);

		this.transform.position = tempPos * 3.0f; //Vector3.Scale(tempPos, new Vector3 (1.5f, 1.5f, 0f)); 

//		print ("x"+ this.transform.position.x);
//		print ("y"+ this.transform.position.y);
//		print ("z"+ this.transform.position.z);

	}
}
