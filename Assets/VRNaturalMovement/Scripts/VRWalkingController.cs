﻿using UnityEngine;
using VR = UnityEngine.VR;

[RequireComponent(typeof(CapsuleCollider))]
public class VRWalkingController : MonoBehaviour
{
	readonly float eyeHeight = 1.75f; // OVRPlugin.eyeHeight
	readonly Vector3 neckToEye = new Vector3(0, 0.075f, 0.08f); // new Vector3(0, OVRPlugin.eyeHeight - OVRManager.profile.neckHeight, OVRPlugin.eyeDepth)

	new Transform camera;

    Quaternion lastRotation;
    Vector3 lastPosition;
	
	void Start()
	{
		camera = GetComponentInChildren<Camera>().transform;
		camera.parent.transform.localPosition = Vector3.up * eyeHeight;
		Recenter();
	}
		
	float velocity;

	void Update()
	{
		UpdateMovement(); 
		UpdateKeyboard();
		UpdateCollider();
	}

	void UpdateKeyboard()
	{
		if (Input.GetKeyDown(KeyCode.Space)) Recenter();
		if (Input.GetKey(KeyCode.Escape)) Application.Quit();
#if UNITY_EDITOR
		// for debugging
		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) transform.position += transform.rotation * Vector3.forward * Time.deltaTime * 10;
		if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) transform.position += transform.rotation * Vector3.left * Time.deltaTime * 10;
		if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) transform.position += transform.rotation * Vector3.right * Time.deltaTime * 10;
		if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) transform.position += transform.rotation * Vector3.back * Time.deltaTime * 10;
		transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * 100 * Time.deltaTime);
#endif
	}

	void Recenter()
	{
		VR.InputTracking.Recenter();
		transform.rotation = Quaternion.Euler(Vector3.up * 90);
	}

	void UpdateMovement()
	{
		// compensate the vertical movement of the head made by nodding 
		var rotPositionChange = camera.localRotation * neckToEye - lastRotation * neckToEye;
		var rotCompensation = Mathf.Abs(rotPositionChange.y);
		
		var yChange = camera.localPosition.y - lastPosition.y;
		
		var desiredVelocity = Time.deltaTime > 0 ? (Mathf.Abs(yChange) - rotCompensation) * 5 / Time.deltaTime : 0;	
		velocity = Mathf.Lerp(velocity, desiredVelocity, Time.deltaTime * 5);
		
		var lookDirection = camera.localRotation * Vector3.forward;
		var moveDirection = new Vector3(lookDirection.x, 0, lookDirection.z).normalized;
		var move = moveDirection * velocity * Time.deltaTime;

		transform.position += transform.rotation * move;
		
		lastPosition = camera.localPosition;
		lastRotation = camera.localRotation;
	}

	void UpdateCollider()
	{
		var cc = ((CapsuleCollider)GetComponent<Collider>());
		var center = cc.center;

		// if no obstacles on the way move collider together with camera
		if (Mathf.Abs(GetComponent<Rigidbody>().velocity.x) < 0.001f && Mathf.Abs(GetComponent<Rigidbody>().velocity.z) < 0.001f)
			center = camera.localPosition;

		var height = camera.localPosition.y + eyeHeight;
		center.y = height / 2;
		cc.center = Vector3.Lerp(cc.center, center, Time.deltaTime);
		cc.height = cc.center.y * 2;
	} 
}
