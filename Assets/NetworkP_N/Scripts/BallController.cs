using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : Photon.MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    private Vector3 _moveDirection;
    private Rigidbody _rigidbody;
    private PhotonTransformView _photonTransformView;

    public Vector3 MoveDirection
    {
        get { return _moveDirection; }
        private set
        {
            _moveDirection = value;
//            _photonTransformView.SetSynchronizedValues(speed: _rigidbody.velocity, turnSpeed: 0.0f);
        }
    }

    public void Initialize()
    {
        this.photonView.RPC("RpcInitialize", PhotonTargets.All);
    }

    [PunRPC]
    public void RpcInitialize()
    {
        RpcEnableCollision(enable: true);
        _rigidbody = GetComponent<Rigidbody>();
        _photonTransformView = GetComponent<PhotonTransformView>();

        Vector3 initPos = Vector3.zero;
        initPos.z = 5.0f;
        transform.position = initPos;

        MoveDirection = new Vector3(1.0f,1.0f,0.0f).normalized;
    }

    public void Move()
    {
        _rigidbody.velocity = _moveDirection * _moveSpeed;
    }

    public void Finalize()
    {
    }

    private void OnCollisionEnter(Collision other)
    {
        RpcReflect(inNormal:other.contacts[0].normal);
//        Refrect(inNormal: other.contacts[0].normal);
    }


    public void Refrect(Vector3 inNormal)
    {
        Debug.Log("Reflect Bar");
        this.photonView.RPC("RpcReflect", PhotonTargets.All, inNormal);
    }

    [PunRPC]
    private void RpcReflect(Vector3 inNormal)
    {
        MoveDirection = Vector3.Reflect(inDirection: MoveDirection.normalized, inNormal: inNormal);
    }

    public void EnableCollision(bool enable)
    {
        this.photonView.RPC("RpcEnableCollision", PhotonTargets.All, enable);
    }

    [PunRPC]
    private void RpcEnableCollision(bool enable)
    {
        var sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.enabled = enable;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}