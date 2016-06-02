using UnityEngine;
using System.Collections;

public class ShipMovement : MonoBehaviour {
	public float m_Speed = 100f;
	public float m_TurnSpeed = 150f;
	public GameObject m_Castle;

	private Vector3 m_StartPosition;
	private Vector3 m_RoamRadius;

	private string m_MovementAxisName;
	private string m_TurnAxisName;
	private Rigidbody m_Rigidbody;
	private float m_MovementInputValue;
	private float m_TurnInputValue;

	private Vector3 m_Waypoint;



	private void Awake ()
	{
		m_Rigidbody = GetComponent<Rigidbody> ();
	}

	private void OnEnable ()
	{
		// When the ship is turned on, make sure it's not kinematic.
		m_Rigidbody.isKinematic = false;

		// Also reset the input values.
		m_MovementInputValue = 0f;
		m_TurnInputValue = 0f;
	}

	private void OnDisable ()
	{
		// When the ship is turned off, set it to kinematic so it stops moving.
		m_Rigidbody.isKinematic = true;
	}


	// Use this for initialization
	private void Start () 
	{
		m_StartPosition = transform.position;
		m_MovementAxisName = "Vertical";
		m_TurnAxisName = "Horizontal";
		m_Waypoint = m_Castle.transform.position;
	}

	// Update is called once per frame
	private	void Update () 
	{
		// Store the value of both input axes.
		m_MovementInputValue = Input.GetAxis (m_MovementAxisName);
		m_TurnInputValue = Input.GetAxis (m_TurnAxisName);

	}

	private void FixedUpdate ()
	{
		// Adjust the rigidbodies position and orientation in FixedUpdate.
		Move ();
		Turn ();


	}

	private void Move ()
	{
		// Create a vector in the direction the tank is facing with a magnitude based on the input, speed and the time between frames.
		Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;

		// Apply this movement to the rigidbody's position.
		m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
	}


	private void Turn ()
	{
		// Determine the number of degrees to be turned based on the input, speed and time between frames.
		float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;

		// Make this into a rotation in the y axis.
		Quaternion turnRotation = Quaternion.Euler (0f, turn, 0f);

		// Apply this rotation to the rigidbody's rotation.
		m_Rigidbody.MoveRotation (m_Rigidbody.rotation * turnRotation);
	}
		
}
