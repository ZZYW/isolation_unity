using UnityEngine;
using System.Collections;

public class trackParticle : MonoBehaviour {
	Vector3 position;
	Vector3 acceleration;
	Vector3 velocity;
	float maxspeed;
	float maxforce;

	// Use this for initialization
	void Start () {
		acceleration = new Vector3(0,0,0);
		velocity = new Vector3(0,0,0);
		maxspeed = 4;
		maxforce = 0.1;
	
	}
	
	// Update is called once per frame
	void Update (Vector3 temp_position) {
		position = temp_position;
		velocity += acceleration;
		velocity = Vector3.ClampMagnitude(velocity,maxspeed);
		position += velocity;
		acceleration = Vector3.Scale(acceleration, new Vector3(0,0,0));
	}

	void applyForce(Vector3 force) {
		acceleration += force;
	}

	void seek(Vector3 target) {
		Vector3 desired = target - position; 
		desired.Normalize ();
		desired = Vector3.Scale (desired, new Vector3 (maxspeed, maxspeed, maxspeed));
		Vector3 steer = desired - velocity;
		steer = Vector3.ClampMagnitude(steer, maxforce);
		applyForce(steer);
	}

}
