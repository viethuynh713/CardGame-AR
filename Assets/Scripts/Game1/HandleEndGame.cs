using System.Collections;
using System.Collections.Generic;
using UnityEngine;using UnityEngine.UI;
using Photon.Pun;

public class HandleEndGame : MonoBehaviour
{
    public List<Image> images;
    public void Init()
    {
        foreach(var img in images )
        {
            img.gameObject.SetActive(false);
        }
    }
    public void SetRank()
    {
        int i = 0;
        foreach(var player in PhotonNetwork.CurrentRoom.Players)
        {
            images[i].gameObject.SetActive(true);
            Text[] txt = images[i].GetComponentsInChildren<Text>();
            txt[0].text = player.Value.NickName;
            txt[1].text = (string)player.Value.CustomProperties["Rank"];
        }
    }
}
