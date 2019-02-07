using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalController : Photon.MonoBehaviour {

    public event Action OnGoalBall;

	private void OnCollisionEnter(Collision other)
	{
		BallController ball = other.gameObject.GetComponent<BallController>();
		if (ball)
		{
			Debug.Log("Goal!");
			OnGoalBall?.Invoke();
		}
	}
	
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	}
}
