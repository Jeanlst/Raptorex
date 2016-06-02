using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ShellExplosion : MonoBehaviour
{
	public LayerMask m_TankMask;                        // Used to filter what the explosion affects, this should be set to "Players".
	public ParticleSystem m_ExplosionParticles;         // Reference to the particles that will play on explosion.
	public AudioSource m_ExplosionAudio;                // Reference to the audio that will play on explosion.
	public float m_MaxDamage = 100f;                    // The amount of damage done if the explosion is centred on a tank.
	public float m_MaxLifeTime = 2f;                    // The time in seconds before the shell is removed.
	public float m_ExplosionRadius = 5f;                // The maximum distance away from the explosion tanks can be and are still affected.

	private void Start ()
	{
		// If it isn't destroyed by then, destroy the shell after it's lifetime.
		Destroy (gameObject, m_MaxLifeTime);


	}


	private void OnTriggerEnter (Collider other)
	{
		// Find the Health script associated with the game object.
		Health targetHealth = other.gameObject.GetComponent<Health> ();

		// If there is no Health script attached to the gameobject, do nothing.
		if (targetHealth) {

			// Calculate the amount of damage the target should take based on it's distance from the shell.
			float damage = CalculateDamage (other.transform.position);

			// Deal this damage to the tank.
			targetHealth.TakeDamage (damage);

		}
		// Unparent the particles from the shell.
		m_ExplosionParticles.transform.parent = null;

		// Play the particle system.
		m_ExplosionParticles.Play();

		// Play the explosion sound effect.
		m_ExplosionAudio.Play();

		// Once the particles have finished, destroy the gameobject they are on.
		Destroy (m_ExplosionParticles.gameObject, m_ExplosionParticles.duration);

		// Destroy the shell.
		Destroy (gameObject);
	}


	private float CalculateDamage (Vector3 targetPosition)
	{
		return 10;
	}
}