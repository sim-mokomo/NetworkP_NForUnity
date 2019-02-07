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
	public void ShowGameResultHudText(bool winner)
	{
		RpcShowGameResultHudText(winner: winner);
		this.photonView.RPC("RpcShowGameResultHudText",PhotonTargets.Others,!winner);
	}

	[PunRPC]
	private void RpcShowGameResultHudText(bool winner)
	{
		string resultText = (winner ? "勝ち" : "負け");
		_gameResultHudText.text = $"あなたの{resultText}です！";
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	}
}
