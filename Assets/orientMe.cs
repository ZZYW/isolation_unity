using UnityEngine;
using System.Collections;

public class orientMe : MonoBehaviour {

	public KinectTrackPoints.NodeType bindNode;
	Vector3 cameraOrigin;
	Vector3 headOrigin;
	Vector3 headTemp;
	Vector3 headDis;
	bool getHeadOrigin;
	ArrayList heads = new ArrayList();

	// Use this for initialization
	void Start () {
		cameraOrigin = this.transform.position;
		getHeadOrigin = false;
//		print ("cameraOrigin" + cameraOrigin);
	}
	
	// Update is called once per frame
	void Update () {
		headTemp = KinectTrackPoints.getNode(KinectTrackPoints.NodeType.Head);
		if (getHeadOrigin == false) {
						headOrigin = headTemp;
						getHeadOrigin = true;
				}
		heads.Add (headTemp);
		headDis = headTemp - headOrigin;
//		print (headDis);
		this.transform.position = cameraOrigin + headDis; 
	}

}
