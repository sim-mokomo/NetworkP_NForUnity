using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : Photon.MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    private Vector3 _moveDirection;
    private Rigidbody _rigidbody;
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
        _moveDirection = Vector3.right + Vector3.up;
    }

    public void Move()
    {
        _rigidbody.velocity = _moveDirection * _moveSpeed * Time.deltaTime;
        _photonTransformView.SetSynchronizedValues(speed: _rigidbody.velocity, turnSpeed: 0.0f);
    }

    public void Finalize()
    {
    }

    private void OnCollisionEnter(Collision other)
    {
        if (photonView.isMine == false)
            return;
        Refrect(inNormal: other.contacts[0].normal);
    }

    public void Refrect(Vector3 inNormal)
    {
        Debug.Log("Reflect Bar");
        this.photonView.RPC("RpcReflect", PhotonTargets.All, inNormal);
    }

    [PunRPC]
    private void RpcReflect(Vector3 inNormal)
    {
        _moveDirection = Vector3.Reflect(inDirection: _moveDirection, inNormal: inNormal);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}