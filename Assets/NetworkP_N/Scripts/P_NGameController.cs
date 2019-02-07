using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameSequence
{
    GameStart,
    Gaming,
    GameEnd,
}

public class P_NGameController : Photon.MonoBehaviour
{
    private GameSequence _gameSequence;
    private event Action<GameSequence> OnChangeGameSequence;

    [SerializeField] private BallController _ballController;

    [SerializeField] private GameObject _playerContorllerPrefab;
    private PlayerController _playerController;

    [SerializeField] private GameObject _playerBarPrefab;
    [SerializeField] private List<Transform> _playerBarSpawnList;
    [SerializeField] private List<GoalController> _playerGoalList = new List<GoalController>(2);
    [SerializeField] private List<Text> _playerPointHudTextList;

    [SerializeField] private int _winRequiredVictoryNum;
    public event Action OnGameEnd;

    [SerializeField] private ResultHudController _resultHudController;
    
    public void OnJoinedRoom()
    {
        OnChangeGameSequence = null;
        OnChangeGameSequence += (GameSequence newGameSequence) =>
        {
            switch (newGameSequence)
            {
                case GameSequence.GameStart:
                    Initialize();
                    break;
                case GameSequence.Gaming:
                    break;
                case GameSequence.GameEnd:
                    Finalize();
                    break;
            }
        };

        RpcSetGameSequence((int) GameSequence.GameStart);
    }

    /// <summary>
    /// プレイヤーが操作するバー等オブジェクトの生成を行う
    /// </summary>
    public void Initialize()
    {
        Room room = PhotonNetwork.room;

        int playerId = room.PlayerCount - 1;

        _playerController = PhotonNetwork.Instantiate(
                prefabName: _playerContorllerPrefab.name,
                position: Vector3.zero,
                rotation: Quaternion.identity,
                group: 0)
            .GetComponent<PlayerController>();
        _playerController.Rename(newObjName: $"Player{playerId}Controller");

        Transform spawnTrans = _playerBarSpawnList[playerId];
        BarController playerBar = PhotonNetwork.Instantiate(
            prefabName: _playerBarPrefab.name,
            position: spawnTrans.position,
            rotation: spawnTrans.rotation,
            group: 0).GetComponent<BarController>();
        playerBar.Rename(newObjName: $"Player{playerId}Bar");

        _playerController.Initialize(
            myBar: playerBar
            , myGoal: _playerGoalList[playerId],
            playerId: playerId);

        _playerController.OnAddPoint += currentPoint =>
        {
            _ballController.Initialize();
            
            if (currentPoint >= _winRequiredVictoryNum)
            {
                OnGameEnd?.Invoke();
            }
            
            ApplyPlayerPointHudText(point: currentPoint, playerId: playerId);
        };
        
        // 対戦者が現れた時にボールを生成する。
        if (PhotonNetwork.isMasterClient == false)
        {
            _ballController.SetCanMove(true);
            _ballController.Initialize();
            SetGameSequence(GameSequence.Gaming);
        }

        ShowResultHud(false);
        _resultHudController.GoToHomeButton.onClick.RemoveAllListeners();
        _resultHudController.GoToHomeButton.onClick.AddListener(() =>
        {
            
        });
        OnGameEnd = null;
        OnGameEnd += () =>
        {
            ShowResultHud(true);
            _resultHudController.ShowGameResultHudText(winner:true);
            SetGameSequence(newGameSequence:GameSequence.GameEnd);
        };
    }

    private void Update()
    {
        switch (_gameSequence)
        {
            case GameSequence.GameStart:
                break;
            case GameSequence.Gaming:
                _playerController.Move();
                _ballController.Move();
                break;
            case GameSequence.GameEnd:
                break;
        }
    }

    /// <summary>
    /// ゲームシーケンスを変更する。新しいシーケンスを返すイベントが発火する。
    /// </summary>
    /// <param name="newGameSequence"></param>
    private void SetGameSequence(GameSequence newGameSequence)
    {
        int intNewGameSequence = (int) newGameSequence;
        this.photonView.RPC("RpcSetGameSequence", PhotonTargets.All, intNewGameSequence);
    }

    [PunRPC]
    private void RpcSetGameSequence(int intNewGameSequence)
    {
        GameSequence newGameSequence = (GameSequence) intNewGameSequence;
        _gameSequence = newGameSequence;
        OnChangeGameSequence?.Invoke(newGameSequence);
    }

    public void Finalize()
    {
        _playerController.Finalize();
        _ballController.Finalize();
        _ballController.SetCanMove(canMove:false);
        
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

    public void ApplyPlayerPointHudText(int point, int playerId)
    {
        this.photonView.RPC("RpcApplyPlayerPointHudText", PhotonTargets.All, point, playerId);
    }

    [PunRPC]
    private void RpcApplyPlayerPointHudText(int point, int playerId)
    {
        _playerPointHudTextList[playerId].text = $"Point:{point}";
    }

    public void ShowResultHud(bool show)
    {
        this.photonView.RPC("RpcShowResultHud",PhotonTargets.All,show);
    }

    [PunRPC]
    public void RpcShowResultHud(bool show)
    {
        _resultHudController.gameObject.SetActive(show);
    }

}