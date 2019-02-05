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
        this.photonView.RPC("RpcInitialize", PhotonTargets.All);
    }

    [PunRPC]
    public void RpcInitialize()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _photonTransformView = GetComponent<PhotonTransformView>();
    }

    public void Move()
    {
        float moveDeltaX = 0.0f;
        moveDeltaX = Input.GetAxis("Horizontal");

        _rigidbody.velocity = new Vector3(moveDeltaX, 0.0f, 0.0f) * _horizontalMoveSpeed * Time.deltaTime;
        _photonTransformView.SetSynchronizedValues(speed: _rigidbody.velocity, turnSpeed: 0.0f);
    }

    public void Finalize()
    {
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}