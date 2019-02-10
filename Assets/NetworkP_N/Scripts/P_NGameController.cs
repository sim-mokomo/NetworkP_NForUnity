using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameSequence
{
    GameStart,
    Battle,
    GameEnd,
}

public class P_NGameController : Photon.MonoBehaviour
{
    private GameSequence _gameSequence;
    private event Action<GameSequence> OnChangeGameSequence;

    [SerializeField] private BallController _ballController;
    [SerializeField] private GameObject _playerContorllerPrefab;
    [SerializeField] private GameObject _playerBarPrefab;
    [SerializeField] private List<Transform> _playerBarSpawnList;
    [SerializeField] private List<GoalController> _playerGoalList = new List<GoalController>(2);
    [SerializeField] private List<Text> _playerPointHudTextList;
    [SerializeField] private int _winRequiredVictoryNum;
    [SerializeField] private ResultHudController _resultHudController;

    public event Action OnGameEnd;
    public event Action OnBattleStart;
    public event Action OnFinishFinalize;

    private int _playerIndex;
    private BarController _playerBarController;
    private PlayerController _playerController;

    public void OnJoinedRoom()
    {
        OnFinishFinalize = null;
        OnFinishFinalize += () =>
        {
            _resultHudController.gameObject.SetActive(false);
            PhotonNetwork.LeaveRoom();
        };

        OnChangeGameSequence = null;
        OnChangeGameSequence += (GameSequence newGameSequence) =>
        {
            switch (newGameSequence)
            {
                case GameSequence.GameStart:
                    LocalInitialize();
                    break;
                case GameSequence.Battle:

                    // RPC通信による初期化はプレイヤーが揃ってから行う
                    _playerBarController.RpcRename(newObjName: $"Player{_playerIndex}Bar");
                    _playerController.RpcRename(newObjName: $"Player{_playerIndex}Controller");
                    _playerController.RpcInitialize(
                        myBar: _playerBarController
                        , myGoal: _playerGoalList[_playerIndex],
                        playerId: _playerIndex);

                    _playerController.OnAddPoint += currentPoint =>
                    {
                        _ballController.RpcEnableCollision(false);
                        _ballController.RpcSetCanMove(false);

                        if (currentPoint >= _winRequiredVictoryNum)
                        {
                            OnGameEnd?.Invoke();
                        }

                        RpcApplyPlayerPointHudText(point: currentPoint, playerId: _playerIndex);
                    };

                    RpcApplyPlayerPointHudText(point: 0, playerId: _playerController.PlayerId);
                    _ballController.LocalInitialize();
                    OnBattleStart?.Invoke();
                    break;
                case GameSequence.GameEnd:
                    Debug.Log("Game End");
                    _ballController.RpcSetCanMove(false);
                    break;
            }
        };

        Room room = PhotonNetwork.room;
        _playerIndex = room.PlayerCount - 1;

        if (room.PlayerCount == room.MaxPlayers)
        {
            var cp = room.CustomProperties;
            cp["WaitPlayer"] = false;
            room.SetCustomProperties(cp);
        }

        LocalShowResultHud(false);
        LocalSetGameSequence((int) GameSequence.GameStart);
    }

    /// <summary>
    /// プレイヤーが操作するバー等オブジェクトの生成を行う
    /// </summary>
    public void LocalInitialize()
    {
        _playerController = PhotonNetwork.Instantiate(
                prefabName: _playerContorllerPrefab.name,
                position: Vector3.zero,
                rotation: Quaternion.identity,
                group: 0)
            .GetComponent<PlayerController>();

        Transform spawnTrans = _playerBarSpawnList[_playerIndex];
        _playerBarController = PhotonNetwork.Instantiate(
            prefabName: _playerBarPrefab.name,
            position: spawnTrans.position,
            rotation: spawnTrans.rotation,
            group: 0).GetComponent<BarController>();

        _resultHudController.GoToHomeButton.onClick.RemoveAllListeners();
        _resultHudController.GoToHomeButton.onClick.AddListener(LocalFinalize);
        OnGameEnd = null;
        OnGameEnd += () =>
        {
            RpcShowResultHud(show: true);
            _resultHudController.LocalShowGameResultHudText(winner: true);
            _resultHudController.RpcShowGameResultHudText(winner: false, photonTargets: PhotonTargets.Others);
            RpcSetGameSequence(newGameSequence: GameSequence.GameEnd);
        };

        // ゲームスタート
        Room room = PhotonNetwork.room;
        if (room.PlayerCount == room.MaxPlayers)
        {
            RpcSetGameSequence(GameSequence.Battle);
        }
    }

    private void Update()
    {
        switch (_gameSequence)
        {
            case GameSequence.GameStart:
                break;
            case GameSequence.Battle:
                _playerController.LocalMove();
                _ballController.LocalMove();
                break;
            case GameSequence.GameEnd:
                break;
        }
    }

    /// <summary>
    /// ゲームシーケンスを変更する。新しいシーケンスを返すイベントが発火する。
    /// </summary>
    /// <param name="newGameSequence"></param>
    private void RpcSetGameSequence(GameSequence newGameSequence, PhotonTargets photonTargets = PhotonTargets.All)
    {
        int intNewGameSequence = (int) newGameSequence;
        this.photonView.RPC("LocalSetGameSequence", photonTargets, intNewGameSequence);
    }

    [PunRPC]
    private void LocalSetGameSequence(int intNewGameSequence)
    {
        GameSequence newGameSequence = (GameSequence) intNewGameSequence;
        _gameSequence = newGameSequence;
        OnChangeGameSequence?.Invoke(newGameSequence);
    }

    public void LocalFinalize()
    {
        _playerController.LocalFinalize();
        PhotonNetwork.Destroy(_playerController.gameObject);
        _ballController.LocalFinalize();
        OnFinishFinalize?.Invoke();
    }
    
    public void RpcApplyPlayerPointHudText(int point, int playerId, PhotonTargets photonTargets = PhotonTargets.All)
    {
        this.photonView.RPC("LocalApplyPlayerPointHudText", photonTargets, point, playerId);
    }

    [PunRPC]
    private void LocalApplyPlayerPointHudText(int point, int playerId)
    {
        _playerPointHudTextList[playerId].text = $"Point:{point}";
    }

    public void RpcShowResultHud(bool show, PhotonTargets photonTargets = PhotonTargets.All)
    {
        this.photonView.RPC("LocalShowResultHud", photonTargets, show);
    }

    [PunRPC]
    public void LocalShowResultHud(bool show)
    {
        _resultHudController.gameObject.SetActive(show);
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

}