using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : Photon.MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    private Vector3 _moveDirection;
    private Rigidbody _rigidbody;
    private bool _canMove;

    public Vector3 MoveDirection
    {
        get { return _moveDirection; }
        private set { _moveDirection = value; }
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void RpcInitialize(PhotonTargets photonTargets = PhotonTargets.All)
    {
        this.photonView.RPC("LocalInitialize", photonTargets);
    }

    [PunRPC]
    public void LocalInitialize()
    {
        LocalSetCanMove(canMove: true);
        LocalEnableCollision(enable: true);

        Vector3 initPos = Vector3.zero;
        initPos.z = 5.0f;
        transform.position = initPos;

        MoveDirection = new Vector3(1.0f, 1.0f, 0.0f).normalized;
    }

    public void LocalMove()
    {
        if (_canMove == false)
        {
            return;
        }

        _rigidbody.velocity = _moveDirection * _moveSpeed;
    }

    public void LocalFinalize()
    {
    }

    private void OnCollisionEnter(Collision other)
    {
        BarController bar = other.gameObject.GetComponent<BarController>();
        if (bar)
        {
            if (photonView.isMine)
            {
                RpcReflect(other.contacts[0].normal);
            }
        }
        else
        {
            LocalReflect(inNormal: other.contacts[0].normal);
        }
    }

    public void RpcReflect(Vector3 inNormal, PhotonTargets photonTargets = PhotonTargets.All)
    {
        Debug.Log("Reflect Bar");
        this.photonView.RPC("LocalReflect", photonTargets, inNormal);
    }

    [PunRPC]
    public void LocalReflect(Vector3 inNormal)
    {
        MoveDirection = Vector3.Reflect(inDirection: MoveDirection.normalized, inNormal: inNormal);
    }

    public void RpcEnableCollision(bool enable, PhotonTargets photonTargets = PhotonTargets.All)
    {
        this.photonView.RPC("LocalEnableCollision", photonTargets, enable);
    }

    [PunRPC]
    private void LocalEnableCollision(bool enable)
    {
        var sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.enabled = enable;
    }

    public void RpcSetCanMove(bool canMove, PhotonTargets photonTargets = PhotonTargets.All)
    {
        this.photonView.RPC("LocalSetCanMove", photonTargets, canMove);
    }

    [PunRPC]
    public void LocalSetCanMove(bool canMove)
    {
        _canMove = canMove;
        if (canMove == false)
        {
            _rigidbody.velocity = Vector3.zero;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}