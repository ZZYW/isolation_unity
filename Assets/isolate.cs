using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class isolate : MonoBehaviour {

	public GameObject cube;

	public bool emitted = false;
	public float noiseSeedValue = 200.0f;
	public ParticleSystem stars; //particle system created by unity
	public ParticleSystem.Particle[] regretParticleCopy; //stores particle info which can be accessed by us
	public List<isolationParticle> particleList  = new List<isolationParticle>();
	private float OscillationValue = 1.5f;
	public float noiseVariable1 = 8.0f;
	public float noiseVariable2 = 1000.0f;
	public float noiseVariable3 = 30.0f;
	
	// Use this for initialization
	void Start () {

		cube = GameObject.Find("Cube");
		stars = GetComponent<ParticleSystem> ();
		regretParticleCopy = new ParticleSystem.Particle[stars.maxParticles];
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if(!emitted){
			if (stars.particleCount == stars.maxParticles) {//check if we got all of the particles, 
				stars.GetParticles (regretParticleCopy); //regretParticleCopy got all data.
				for (int i=0; i < stars.maxParticles; i++) {
					particleList.Add(new isolationParticle(regretParticleCopy[i].position, regretParticleCopy[i].velocity));
				};
				emitted = true;
				print ("loop start!");
			}//if ends
		}else{
			//=====================
			//THE RAEL THING
			noiseSeedValue += Random.Range(-OscillationValue, OscillationValue);
			Vector3 mousePos = cube.transform.position;

			for(int i=0; i < stars.maxParticles; i++){



				Vector3 target = new Vector3 (particleList[i].position.x + noiseVariable1*(Mathf.PerlinNoise(noiseVariable2/noiseVariable3+particleList[i].position.y/noiseSeedValue,  0.0f)-0.5f),
				                              particleList[i].position.y + noiseVariable1*(Mathf.PerlinNoise(noiseVariable2/noiseVariable3+particleList[i].position.x/noiseSeedValue,  0.0f)-0.5f),
				                              0);

				
				float isolationR = 200;
				float margin = 50;

				if( Vector3.Distance(particleList[i].position, mousePos) < isolationR ){
					Vector3 go = mousePos - particleList[i].position;
					go.Normalize();
					go *= ofMap(Vector3.Distance(particleList[i].position, mousePos), 0, isolationR, -20, 0);
					float mappedAlpha = ofMap(Vector3.Distance(particleList[i].position, mousePos), 0, isolationR+60,255 , particleList[i].c.a);
					float mappedB = ofMap(Vector3.Distance(particleList[i].position, mousePos), 0, isolationR+60, 255 , 230);
					particleList[i].c =new Color(255,mappedB,mappedAlpha);
					target += go;
				}else if(Vector3.Distance(particleList[i].position, mousePos) >= isolationR && Vector3.Distance(particleList[i].position, mousePos) < isolationR + margin) {
					float mappedAlpha = ofMap(Vector3.Distance(particleList[i].position, mousePos), isolationR, isolationR+margin,255 , particleList[i].minA);
					particleList[i].c =new Color(255,255,255,mappedAlpha);
				}else{
					particleList[i].dim();
				}
				particleList[i].calculate(target);

			

			}//GIANT FOR LOOP ENDS

		
			//apply all calculated values to unity's particleSystem
			for (int i=0; i <  stars.maxParticles; i++) {
				regretParticleCopy [i].position = particleList [i].position;
				regretParticleCopy [i].velocity = particleList [i].velocity;
			 	regretParticleCopy[i].color = particleList[i].c;
			}

			
			//apply all calculated values to unity's particleSystem AGAIN
			stars.SetParticles (regretParticleCopy, stars.maxParticles);

		}


	
	}


	public class isolationParticle
	{
		public Vector3 position;
		public Vector3 target;
		private Vector3 acceleration;
		public Vector3 velocity;
		public Color c;
	
		float maxspeed;
		float maxforce;
		public float minA = 70.0f;

		public isolationParticle (Vector3 _position, Vector3 _velocity)
		{
		
			position = _position;
			target  = new Vector3(0,0,0);
			velocity = new Vector3(0,0,0);
			acceleration = new Vector3 (0, 0, 0);
			maxspeed = Random.Range(0.1f,2.0f);
			maxforce = Random.Range(0.01f, 0.06f);
			c = new Color(255,255,255,minA);
		}

		
		public void calculate (Vector3 _target)
		{
			target = _target;
			Vector3 desired = target - position;
			desired.Normalize ();
			desired *= maxspeed;
			Vector3 steer = desired - velocity;
			steer = Vector3.ClampMagnitude (steer, maxforce);
			acceleration += steer;

			velocity += acceleration;
			velocity = Vector3.ClampMagnitude(velocity, maxspeed);
			position += velocity;
			acceleration *= 0;
		}
		
		public void dim(){

			Color tempC = c;
			if(tempC.a > minA){
				tempC = new Color(tempC.r,tempC.g, tempC.b,tempC.a - 1);
				c = tempC;
			}
		
		}
	}


	//the powerful MAP function
	public float ofMap(float value, 
	                              float istart, 
	                              float istop, 
	                              float ostart, 
	                              float ostop) {
		return ostart + (ostop - ostart) * ((value - istart) / (istop - istart));
	}

}
