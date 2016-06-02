using UnityEngine;
using System.Collections;

public class FloatBehaviour : MonoBehaviour {

	public float waterLevel = 4;
	public float floatHeight = 2;
	public float bounceDamp = 0.05f;
	public Vector3 buoyancyCenterOffSet;

	private float forceFactor;
	private Vector3 actionPoint;
	private Vector3 upLift;
	
	// Update is called once per frame
	void Update () {
		actionPoint = transform.position + transform.TransformDirection (buoyancyCenterOffSet);
		forceFactor = 1f - ((actionPoint.y - waterLevel) / floatHeight);

		if (forceFactor > 0f) {
			upLift = -Physics.gravity * (forceFactor - GetComponent<Rigidbody> ().velocity.y * bounceDamp) * GetComponent<Rigidbody> ().mass;
			GetComponent<Rigidbody> ().AddForceAtPosition (upLift, actionPoint);
		}
	}
}