using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PUNNetworkController : MonoBehaviour
{

	[SerializeField] private Text connectConditionHud;
	
	private void Start()
	{
		PhotonNetwork.autoJoinLobby = true;
		bool successToConnect = PhotonNetwork.ConnectUsingSettings(gameVersion: "1.0v");
		if (successToConnect == false)
		{
			Debug.Log("Server connection failure");
			connectConditionHud.text = "Server connection failure";
		}
	}

	public void OnJoinedLobby()
	{
		Debug.Log("Joined Lobby");
		connectConditionHud.text = "Joined Lobby";
		
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.MaxPlayers = 2;
		roomOptions.IsOpen = true;

		PhotonNetwork.JoinOrCreateRoom(roomName: "test",
			roomOptions: null,
			typedLobby: TypedLobby.Default);
	}

	public void OnJoinedRoom()
	{
		Debug.Log("Joined Room");
		connectConditionHud.text = "Joined Room";
	}
}
