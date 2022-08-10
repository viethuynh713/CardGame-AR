using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PointSpawn : MonoBehaviour
{
    public PhotonView view;
    void Start()
    {
        view = GetComponent<PhotonView>();
    }

    public void ChangeName(string id)
    {
        view.RPC("ChangeNamePun", RpcTarget.All, id);
    }
    [PunRPC]
    public void ChangeNamePun(string id)
    {
        gameObject.name = id;
    }
}
