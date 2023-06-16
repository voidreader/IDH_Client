using UnityEngine;
using System.Collections;


public class SelfDestruct : MonoBehaviour {
	ParticleSystem[] pss;
	public float selfdestruct_in = 4; // Setting this to 0 means no selfdestruct.
	private float timeAcc;
	//private float acc;
	//void Start () {
	//	ps = GetComponent<ParticleSystem>();

	//	if ( selfdestruct_in != 0){
	//		Destroy (gameObject, selfdestruct_in);
	//	}
	//}

	//private void Update()
	//{
	//	var main = ps.main;
	//	main.simulationSpeed = GameCore.timeScale;

	//	if( !ps.isPlaying )
	//		gameObject.SetActive(false);
	//}

	private void OnEnable()
	{
		if( pss == null )
			pss = GetComponentsInChildren<ParticleSystem>();

		timeAcc = 0;
		for(int i = 0; i < pss.Length; ++i)
			pss[i].Play();
	}

	public void Update()
	{
		timeAcc += Time.deltaTime * GameCore.timeScale;
		if (selfdestruct_in != -1 && timeAcc > selfdestruct_in )
		{
			gameObject.SetActive(false);
			return;
		}

		for (int i = 0; i < pss.Length; ++i)
		{
			//	pss[i].Simulate(Time.deltaTime * GameCore.timeScale);
			var main = pss[i].main;
			main.simulationSpeed = GameCore.timeScale;
		}
	}
}
