using UnityEngine;
using System.Collections;

public class DontGoThroughThings : MonoBehaviour {

	// Careful when setting this to true - it might cause double
	// events to be fired - but it won't pass through the trigger
	public bool sendTriggerMessage = false;

	public LayerMask layerMask = -1; // make sure we aren't in this layer
	public float skinWidth = 0.1f; // probably doesn't need to be changed

	private float minimumExtent;
	private float partialExtent;
	private float sqrMinimumExtent;
	private Vector3 previousPositions;
	private Rigidbody myRigidBody;
	private Collider myCollider;

	// Use this for initialization
	void Start () {
		myRigidBody = GetComponent<Rigidbody> ();
		myCollider = GetComponent<Collider> ();
		previousPositions = myRigidBody.position;
		minimumExtent = Mathf.Min (Mathf.Min (myCollider.bounds.extents.x, myCollider.bounds.extents.y), myCollider.bounds.extents.z);
		partialExtent = minimumExtent * (1.0f - skinWidth);
		sqrMinimumExtent = minimumExtent * minimumExtent;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		// have we moved more than our minimum extent?
		Vector3 movementThisStep = myRigidBody.position - previousPositions;
		float movementSqrMagnitude = movementThisStep.sqrMagnitude;

		if (Mathf.Abs(movementThisStep.x - previousPositions.x)> sqrMinimumExtent || Mathf.Abs(movementThisStep.y - previousPositions.y)> sqrMinimumExtent
			|| Mathf.Abs(movementThisStep.z - previousPositions.z)> sqrMinimumExtent) {
			float movementMagnitude = Mathf.Sqrt (movementSqrMagnitude);
			RaycastHit hitInfo;

			// check for obstructions we might have missed
			if (Physics.Raycast (previousPositions, movementThisStep, out hitInfo, movementMagnitude, layerMask.value)) {
				if (!hitInfo.collider)
					return;

				if (hitInfo.collider.isTrigger)
					hitInfo.collider.SendMessage ("OnTriggerEnter", myCollider);

				if (!hitInfo.collider.isTrigger)
					myRigidBody.position = hitInfo.point - (movementThisStep / movementMagnitude) * partialExtent;
			}
		}

		previousPositions = myRigidBody.position;
	}
}
