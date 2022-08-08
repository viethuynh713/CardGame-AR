using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CubeObject : MonoBehaviour 
{
    PhotonView view;
    private void Start() {
        view = GetComponent<PhotonView>();
    }
    // [PunRPC]
   public void ChangeColor(Color color)
   {

        if (color == GetComponent<MeshRenderer>().material.color)
        {
            return;
        } 
       GetComponent<MeshRenderer>().material.color = color;
       Debug.Log("Color Changed");
       view.RPC("ChangeColorAll", RpcTarget.All, color.r,color.g,color.b);
   }
    [PunRPC]
    public void ChangeColorAll(float r, float g, float b)
    {
        Color colorSelected = new Color(r,g,b);
        if (colorSelected == GetComponent<MeshRenderer>().material.color)
        {
            return;
        }
        GetComponent<MeshRenderer>().material.color = colorSelected;
    }
    // void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    // {
    //     if (stream.IsWriting)
    //     {
    //         stream.SendNext(transform.position);
    //     }
    //     else
    //     {
    //         Vector3 pos = (Vector3)stream.ReceiveNext();
    //         ChangePosition(pos);
            
    //     }
    // }
    public void ChangePosition(Vector3 pos){
        transform.position = pos;

    }

}
