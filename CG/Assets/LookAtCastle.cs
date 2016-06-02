using UnityEngine;
using System.Collections;

public class LookAtCastle : MonoBehaviour {
	public GameObject m_Castle;
	public float m_TurnSpeed = 25;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		//transform.LookAt (m_Castle.transform);
		rotateTowards(m_Castle.transform.position);
	}

	private void rotateTowards(Vector3 to) 
	{
		Quaternion _lookRotation = Quaternion.LookRotation ((to - transform.position).normalized);

		//over time
		transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * m_TurnSpeed);

	}
}
