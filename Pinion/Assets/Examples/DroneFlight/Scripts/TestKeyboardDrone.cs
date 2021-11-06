using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A shitty testing script that vaguely emulates drone controls.
// If you expand upon this for any real gameplay purposes, you are a dangerous person and should be locked away.
public class TestKeyboardDrone : MonoBehaviour
{
	[SerializeField]
	private Vector3 movementMultiplier = new Vector3(8, 300, 8); // This are completely eyeballed multipliers. Don't look for logic behind them, or this entire script, really.

	[SerializeField]
	private Transform cameraRoot = null;

	[SerializeField]
	private float cameraUp = 0.6f;
	[SerializeField]
	private float cameraBack = 1.5f;

	private Rigidbody attachedRigidbody = null;

	private void Start()
	{
		attachedRigidbody = this.GetComponent<Rigidbody>();

		if (attachedRigidbody == null)
		{
			attachedRigidbody = this.gameObject.AddComponent<Rigidbody>();
		}

		attachedRigidbody.useGravity = false;
		attachedRigidbody.drag = 0.9f;

		if (cameraRoot != null)
		{
			cameraRoot.transform.parent = null;
		}
	}

	private void FixedUpdate()
	{
		float vertical = Input.GetAxis("Vertical");
		float horizontal = Input.GetAxis("Horizontal");
		float altitude = Input.GetAxis("Mouse ScrollWheel");

		Vector3 velocity = new Vector3(0, altitude, vertical);
		velocity.Scale(movementMultiplier);

		attachedRigidbody.AddRelativeForce(velocity, ForceMode.Acceleration);
		attachedRigidbody.AddRelativeTorque(Vector3.up * horizontal, ForceMode.Acceleration);

		PlaceCamera();
	}

	private void PlaceCamera()
	{
		Vector3 newBehindPoint = this.transform.position;

		newBehindPoint += (this.transform.forward * -1) * cameraBack;
		newBehindPoint += this.transform.up * cameraUp;

		// cameraRoot.transform.position = Vector3.Lerp(cameraRoot.transform.position, newBehindPoint, Time.deltaTime * 5f);
		// Quaternion newLookAtRotation = Quaternion.LookRotation((this.transform.position - newBehindPoint).normalized, this.transform.up);
		// cameraRoot.transform.rotation = Quaternion.Lerp(cameraRoot.transform.rotation, newLookAtRotation, Time.deltaTime * 5f);

		cameraRoot.transform.position = newBehindPoint;
		Quaternion newLookAtRotation = Quaternion.LookRotation((this.transform.position - newBehindPoint).normalized, this.transform.up);
		cameraRoot.transform.rotation = newLookAtRotation;

	}
}
