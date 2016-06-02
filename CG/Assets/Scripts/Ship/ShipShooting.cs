using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShipShooting : MonoBehaviour
{
	public Rigidbody m_Shell;                   // Prefab of the shell.
	public Transform m_FireTransform;           // A child of the tank where the shells are spawned.
	public Slider m_AimSlider;                  // A child of the tank that displays the current launch force.
	public AudioSource m_ShootingAudio;         // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source.
	public AudioClip m_ChargingClip;            // Audio that plays when each shot is charging up.
	public AudioClip m_FireClip;                // Audio that plays when each shot is fired.
	public float m_MinLaunchForce = 15f;        // The force given to the shell if the fire button is not held.
	public float m_MaxLaunchForce = 30f;        // The force given to the shell if the fire button is held for the max charge time.
	public float m_MaxChargeTime = 0.75f;       // How long the shell can charge for before it is fired at max force.
	public GameObject m_Castle;					// Castle's gameobject
	public float m_Distance = 850f;				// Distance to shoot at the Castle
	public float m_Speed = 20000f;				// Projectile speed


	private string m_FireButton;                // The input axis that is used for launching shells.
	private float m_CurrentLaunchForce;         // The force that will be given to the shell when the fire button is released.
	private float m_ChargeSpeed;                // How fast the launch force increases, based on the max charge time.
	private bool m_Fired;                       // Whether or not the shell has been launched with this button press.
	private bool m_CanShoot;					// Whether or not the ship is in the distance to shoot


	private void OnEnable()
	{
		// When the tank is turned on, reset the launch force and the UI
		m_CurrentLaunchForce = m_MinLaunchForce;
		m_AimSlider.value = m_MinLaunchForce;
		m_CanShoot = true;
	}


	private void Start ()
	{
		// The fire axis is based on the player number.
		m_FireButton = "Fire";

		// The rate that the launch force charges up is the range of possible forces by the max charge time.
		m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
	}


	private void Update ()
	{
		// The slider should have a default value of the minimum launch force.
		m_AimSlider.value = m_MinLaunchForce;

		if (m_CanShoot && CanShoot ())
		{
			Fire ();
			m_CanShoot = false;
			StartCoroutine(Reload ());
		}

		/*
		// If the max force has been exceeded and the shell hasn't yet been launched..
		else if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
		{
			// ... use the max force and launch the shell.
			m_CurrentLaunchForce = m_MaxLaunchForce;
			Fire ();
		}
		// Otherwise, if the fire button has just started being pressed...
		else if (Input.GetButtonDown (m_FireButton))
		{
			// ... reset the fired flag and reset the launch force.
			m_Fired = false;
			m_CurrentLaunchForce = m_MinLaunchForce;

			// Change the clip to the charging clip and start it playing.
			m_ShootingAudio.clip = m_ChargingClip;
			m_ShootingAudio.Play ();
		}
		// Otherwise, if the fire button is being held and the shell hasn't been launched yet...
		else if (Input.GetButton (m_FireButton) && !m_Fired)
		{
			// Increment the launch force and update the slider.
			m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

			m_AimSlider.value = m_CurrentLaunchForce;
		}
		// Otherwise, if the fire button is released and the shell hasn't been launched yet...
		else if (Input.GetButtonUp (m_FireButton) && !m_Fired)
		{
			// ... launch the shell.
			Fire ();
		} */
	}


	private void Fire ()
	{
		// Set the fired flag so only Fire is only called once.
		m_Fired = true;

		// Create an instance of the shell and store a reference to it's rigidbody.
		Rigidbody shellInstance =
			Instantiate (m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

		StartCoroutine (SimulateProjectile (shellInstance));

		//shellInstance.velocity = (m_Castle.transform.position - shellInstance.transform.position).normalized * m_Speed;

		// Set the shell's velocity to the launch force in the fire position's forward direction.
		//shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward * 2; 
		//shellInstance.AddForce(shellInstance.transform.forward * m_Speed);

		//shellInstance.velocity = BallisticVelocity (m_FireTransform, m_FireTransform.rotation.eulerAngles.x);

		// Change the clip to the firing clip and play it.
		m_ShootingAudio.clip = m_FireClip;
		m_ShootingAudio.Play ();

		// Reset the launch force.  This is a precaution in case of missing button events.
		m_CurrentLaunchForce = m_MinLaunchForce;
	}

	private IEnumerator Reload () 
	{
		yield return new WaitForSeconds (2);
		m_CanShoot = true;
	}

	private bool CanShoot ()
	{
		Debug.Log (Vector3.Distance (transform.position, m_Castle.transform.position));
		Debug.DrawLine (transform.position, m_Castle.transform.position, Color.red);
		if (Vector3.Distance (transform.position, m_Castle.transform.position) <= m_Distance)
			return true;
		else
			return false;
	}

	Vector3 BallisticVelocity(Transform target, float angle)
	{
		Vector3 dir = target.position - m_FireTransform.position; // get Target Direction
		float height = dir.y; // get height difference
		dir.y = 0; // retain only the horizontal direction
		float dist = dir.magnitude; // get horizontal distance
		float a = angle * Mathf.Deg2Rad; // Convert angle to radians
		dir.y = dist * Mathf.Tan(a); // set dir to the elevation angle.
		dist += height / Mathf.Tan(a); // Correction for small height differences

		// Calculate the velocity magnitude
		float velocity = Mathf.Sqrt(dist * Physics.gravity.magnitude / Mathf.Sin(2 * a));
		return velocity * dir.normalized; // Return the velocity vector.
	}

	IEnumerator SimulateProjectile(Rigidbody shell)
	{
		// Calculate distance to target
		float target_Distance = Vector3.Distance(shell.transform.position, m_Castle.transform.position);

		float gravity = 9.8f;
		float firingAngle = 45;
		Debug.Log (firingAngle);
		// Calculate the velocity needed to throw the object to the target at specified angle.
		float projectile_Velocity = target_Distance / (Mathf.Sin(2 * firingAngle * Mathf.Deg2Rad) / gravity);
		projectile_Velocity *= 1.4f;

		// Extract the X  Y componenent of the velocity
		float Vx = Mathf.Sqrt(projectile_Velocity * 1.5f) * Mathf.Cos(firingAngle * Mathf.Deg2Rad);
		float Vy = Mathf.Sqrt(projectile_Velocity * 1.5f) * Mathf.Sin(firingAngle * Mathf.Deg2Rad);

		// Calculate flight time.
		float flightDuration = target_Distance / Vx;
		flightDuration *= 1.5f;

		// Rotate projectile to face the target.
		shell.transform.rotation = Quaternion.LookRotation(m_Castle.transform.position - shell.transform.position);

		float elapse_time = 0;

		while (elapse_time < flightDuration)
		{
			if (shell != null)
				shell.transform.Translate(0, (Vy - (gravity * elapse_time)) * Time.deltaTime, Vx * Time.deltaTime);

			elapse_time += Time.deltaTime;

			yield return null;
		}
	}  
}