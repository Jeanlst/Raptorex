using UnityEngine;
using System.Collections;

public class Invisible : MonoBehaviour {

	// Use this for initialization
	void Start () {

		if (GetComponent<MeshRenderer> () != null) {
			GetComponent<MeshRenderer>().enabled = false;
		}

		foreach (Transform child in transform) {
			if (child.GetComponent<MeshRenderer> () != null) {
				child.GetComponent<MeshRenderer> ().enabled = false;
			}
		}
	}
}
