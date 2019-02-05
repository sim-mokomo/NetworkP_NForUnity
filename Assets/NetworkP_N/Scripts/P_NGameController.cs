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

        SetGameSequence(newGameSequence: GameSequence.GameStart);
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

        _gameSequence = GameSequence.Gaming;
    }

    private void Update()
    {
        switch (_gameSequence)
        {
            case GameSequence.GameStart:
                break;
            case GameSequence.Gaming:
                _barController.Move();
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
        _gameSequence = newGameSequence;
        OnChangeGameSequence?.Invoke(newGameSequence);
    }

    public void Finalize()
    {
        _barController.Finalize();
    }
}