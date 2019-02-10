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

    public void RpcInitialize(BarController myBar, GoalController myGoal,int playerId,PhotonTargets photonTargets = PhotonTargets.All)
    {
        int ballViewID = myBar.photonView.viewID;
        int goalViewID = myGoal.photonView.viewID;

        List<int> sendDatas = new List<int>();
        sendDatas.Add(ballViewID);
        sendDatas.Add(goalViewID);
        sendDatas.Add(playerId);

        photonView.RPC("LocalInitialize", photonTargets, sendDatas.ToArray());
    }

    [PunRPC]
    private void LocalInitialize(int[] initializeData)
    {
        int barViewID = initializeData[0];
        int goalViewID = initializeData[1];
        int playerID = initializeData[2];

        _myBar = PhotonView.Find(barViewID).GetComponent<BarController>();
        _myGoal = PhotonView.Find(goalViewID).GetComponent<GoalController>();
        _point = 0;
        _playerID = playerID;

        _myBar.LocalInitialize();
        _myGoal.LocalInitialize();
        
        _myGoal.OnGoalBall += () =>
        {
            var otherPlayerController = FindObjectsOfType<PlayerController>()
                .FirstOrDefault(p => p != this);
            otherPlayerController.RpcAddPoint(deltaPoint: +1);
        };
        
    }

    public void LocalMove()
    {
        _myBar.LocalMove();
    }

    public void LocalFinalize()
    {
        PhotonNetwork.Destroy(_myBar.gameObject);
    }

    public void RpcAddPoint(int deltaPoint,PhotonTargets photonTargets = PhotonTargets.All)
    {
        Debug.Log("Add Point !");
        this.photonView.RPC("LocalAddPoint",photonTargets,deltaPoint);
    }

    [PunRPC]
    private void LocalAddPoint(int deltaPoint)
    {
        _point += deltaPoint;
        OnAddPoint?.Invoke(_point);
    }

    public void RpcRename(string newObjName,PhotonTargets photonTargets = PhotonTargets.All)
    {
        photonView.RPC("LocalRename",photonTargets,newObjName);
    }

    [PunRPC]
    private void LocalRename(string newObjName)
    {
        gameObject.name = newObjName;
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}