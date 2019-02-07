using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : Photon.MonoBehaviour
{
    private BarController _myBar;
    private GoalController _myGoal;
    private int _point;
    private int _playerID;
    public int PlayerId => _playerID;
    
    public event Action<int> OnAddPoint;

    public void Initialize(BarController myBar, GoalController myGoal,int playerId)
    {
        int ballViewID = myBar.photonView.viewID;
        int goalViewID = myGoal.photonView.viewID;

        List<int> sendDatas = new List<int>();
        sendDatas.Add(ballViewID);
        sendDatas.Add(goalViewID);
        sendDatas.Add(playerId);

        photonView.RPC("RpcInitialize", PhotonTargets.All, sendDatas.ToArray());
    }

    [PunRPC]
    private void RpcInitialize(int[] initializeData)
    {
        int barViewID = initializeData[0];
        int goalViewID = initializeData[1];
        int playerID = initializeData[2];

        _myBar = PhotonView.Find(barViewID).GetComponent<BarController>();
        _myGoal = PhotonView.Find(goalViewID).GetComponent<GoalController>();
        _point = 0;
        _playerID = playerID;

        _myBar.Initialize();
        _myGoal.OnGoalBall += () =>
        {
            var otherPlayerController = FindObjectsOfType<PlayerController>()
                .FirstOrDefault(p => p != this);
            otherPlayerController.AddPoint(deltaPoint: +1);
        };
    }

    public void Move()
    {
        _myBar.Move();
    }

    public void Finalize()
    {
    }

    public void AddPoint(int deltaPoint)
    {
        Debug.Log("Add Point !");
        this.photonView.RPC("RpcAddPoint",PhotonTargets.All,deltaPoint);
    }

    [PunRPC]
    private void RpcAddPoint(int deltaPoint)
    {
        _point += deltaPoint;
        OnAddPoint?.Invoke(_point);
    }

    public void Rename(string newObjName)
    {
        photonView.RPC("RpcRename",PhotonTargets.All,newObjName);
    }

    [PunRPC]
    private void RpcRename(string newObjName)
    {
        gameObject.name = newObjName;
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}