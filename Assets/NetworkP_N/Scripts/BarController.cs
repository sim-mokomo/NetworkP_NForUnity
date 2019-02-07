using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarController : Photon.MonoBehaviour
{
    private Rigidbody _rigidbody;
    [SerializeField] private float _horizontalMoveSpeed;
    private PhotonTransformView _photonTransformView;

    public void Initialize()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _photonTransformView = GetComponent<PhotonTransformView>();
    }

    public void Move()
    {
        float moveDeltaX = 0.0f;
        moveDeltaX = Input.GetAxis("Horizontal");
        Vector3 moveDirection = new Vector3(moveDeltaX, 0.0f, 0.0f);
        _rigidbody.velocity = moveDirection * _horizontalMoveSpeed;
    }

    public void Finalize()
    {
    }

    public void Rename(string newObjName)
    {
        photonView.RPC("RpcRename", PhotonTargets.All, newObjName);
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