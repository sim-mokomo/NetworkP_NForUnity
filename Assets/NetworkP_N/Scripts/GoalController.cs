using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalController : Photon.MonoBehaviour {

    public event Action OnGoalBall;

	private void OnTriggerEnter(Collider other)
	{
		if (photonView.isMine == false)
			return;
		
		BallController ball = other.gameObject.GetComponent<BallController>();
		if (ball && ball.photonView.isMine)
		{
			Debug.Log("Goal!");
			OnGoalBall?.Invoke();
		}
	}
	
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	}
	
	public void LocalInitialize()
	{
		OnGoalBall = null;
		Debug.Log("goal Initialize");
	}

}
