using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultHudController : Photon.MonoBehaviour
{

	[SerializeField] private Text _gameResultHudText;
	[SerializeField] private Button _goToHomeButton;
	public Button GoToHomeButton => _goToHomeButton;

	/// <summary>
	/// ゲームのリザルトUIを表示。勝者が実行する。
	/// </summary>
	/// <param name="winner"></param>
	public void RpcShowGameResultHudText(bool winner,PhotonTargets photonTargets = PhotonTargets.Others)
	{
		this.photonView.RPC("LocalShowGameResultHudText",photonTargets,winner);
	}

	[PunRPC]
	public void LocalShowGameResultHudText(bool winner)
	{
		string resultText = (winner ? "勝ち" : "負け");
		_gameResultHudText.text = $"あなたの{resultText}です！";
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	}
}
