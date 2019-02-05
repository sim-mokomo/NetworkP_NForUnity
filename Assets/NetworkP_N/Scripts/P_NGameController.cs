using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameSequence
{
    GameStart,
    Gaming,
    GameEnd,
}

public class P_NGameController : Photon.MonoBehaviour
{
    [SerializeField] private GameObject _barPreafb;
    [SerializeField] private List<GameObject> _barSpawnList;
    private BarController _barController;
    private GameSequence _gameSequence;
    private event Action<GameSequence> OnChangeGameSequence;

    [SerializeField] private GameObject _ballPrefab;
    private BallController _ballController;

    public void OnJoinedRoom()
    {
        _gameSequence = GameSequence.GameStart;
        
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

        RpcSetGameSequence(intNewGameSequence: (int)GameSequence.GameStart);
//        SetGameSequence(newGameSequence: GameSequence.GameStart);
    }

    /// <summary>
    /// プレイヤーが操作するバー等オブジェクトの生成を行う
    /// </summary>
    public void Initialize()
    {
        Room room = PhotonNetwork.room;

        int playerNumber = room.PlayerCount - 1;
        Transform barSpawnPos = _barSpawnList[playerNumber].transform;

        _barController = PhotonNetwork.Instantiate(
            prefabName: _barPreafb.name,
            position: barSpawnPos.position,
            rotation: barSpawnPos.rotation,
            group: 0).GetComponent<BarController>();
        _barController.Initialize();

        // 対戦者が現れた時にボールを生成する。
        if (PhotonNetwork.isMasterClient == false)
        {
            _ballController = PhotonNetwork.Instantiate(
                prefabName: _ballPrefab.name,
                position: _ballPrefab.transform.position,
                rotation: Quaternion.identity,
                group: 0).GetComponent<BallController>();
            _ballController.Initialize();
            SetGameSequence(GameSequence.Gaming);
        }
    }

    private void Update()
    {
        switch (_gameSequence)
        {
            case GameSequence.GameStart:
                break;
            case GameSequence.Gaming:
                _barController.Move();
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
        this.photonView.RPC("RpcSetGameSequence",PhotonTargets.All,intNewGameSequence);
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
        _barController.Finalize();
        _ballController.Finalize();
    }
}