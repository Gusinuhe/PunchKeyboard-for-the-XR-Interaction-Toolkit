using System.Collections;
using UnityEngine;

public class CollisionFeedbackController : MonoBehaviour
{
	//public SteamVR_TrackedObject TrackedObject;
	//private SteamVR_Controller.Device device;
	private const int KeyPressFeedbackStrength = 1500;
	private bool isColliding = false;

	void Start()
	{
		Key.keyPressed += KeyPressedHapticFeedback;
	}

	private void OnCollisionStay(Collision collision)
	{
		isColliding = true;
	}

	private void OnCollisionExit(Collision collision)
	{
		isColliding = false;
	}

	private void KeyPressedHapticFeedback()
	{
		if (isColliding)
		{
			StartCoroutine ("TriggerHapticFeedback", KeyPressFeedbackStrength);
		}
	}
	
	private void OnDisable()
	{
		Key.keyPressed -= KeyPressedHapticFeedback;
	}
	
	// TODO fix this
	/*
	private void Update()
	{
		device = SteamVR_Controller.Input ((int)TrackedObject.index);
	}

	private IEnumerator TriggerHapticFeedback(int strength)
	{
		device.TriggerHapticPulse(500);
		yield return new WaitForEndOfFrame();
		device.TriggerHapticPulse(2000);
		yield return new WaitForEndOfFrame();
	}
	*/
}