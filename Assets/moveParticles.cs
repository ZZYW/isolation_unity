using UnityEngine;
using System.Collections;

public class moveParticles : MonoBehaviour {
	public GameObject hand;
	public trackParticle[] trackerHolder;


	void Start () {
		hand = GameObject.Find("Sphere");  //get hand position
		ParticleSystem stars = (ParticleSystem)GetComponent("ParticleSystem");
		ParticleSystem.Particle [] ParticleList = new ParticleSystem.Particle[stars.particleCount];
		trackParticle[] trackerHolder = new trackParticle[ParticleList.Length]; 
	}

	
	// Update is called once per frame
	void Update () {
		//get all existing particles' positio
		ParticleSystem stars = (ParticleSystem)GetComponent("ParticleSystem");
		ParticleSystem.Particle [] ParticleList = new ParticleSystem.Particle[stars.particleCount];
		stars.GetParticles(ParticleList);

		//steering
		for(int i = 0; i < ParticleList.Length; i++)
		{

			tracker.Update(ParticleList[i]);
			tracker.applyForce();
			tracker.seek(hand.transform.position);

//			float yPosition = Mathf.Sin(Time.time + Random.Range(0f, 0.01f)) * (Random.Range(0f, 0.05f));
//			float yPosition = Mathf.Sin(Time.time) * (Time.deltaTime);
		}       
		stars.SetParticles(ParticleList, stars.particleCount); //update all new particles' positions
	}




}



