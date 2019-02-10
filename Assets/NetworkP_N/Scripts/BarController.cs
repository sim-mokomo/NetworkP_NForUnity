using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarController : Photon.MonoBehaviour
{
    private Rigidbody _rigidbody;
    [SerializeField] private float _horizontalMoveSpeed;

    public void LocalInitialize()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void LocalMove()
    {
        float moveDeltaX = 0.0f;
        moveDeltaX = Input.GetAxis("Horizontal");
        Vector3 moveDirection = new Vector3(moveDeltaX, 0.0f, 0.0f);
        _rigidbody.velocity = moveDirection * _horizontalMoveSpeed;
    }

    public void LocalFinalize()
    {
    }

    public void RpcRename(string newObjName,PhotonTargets photonTargets=PhotonTargets.All)
    {
        photonView.RPC("LocalRename", photonTargets, newObjName);
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