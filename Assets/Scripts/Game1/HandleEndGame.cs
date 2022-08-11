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
        Debug.Log(GameManager1.instance.listRank.Count);
        /*foreach(var player in PhotonNetwork.CurrentRoom.Players)
        {
            images[i].gameObject.SetActive(true);
            Text[] txt = images[i].GetComponentsInChildren<Text>();
            Debug.Log(txt.Length + ":" + txt[1].name + " , " + txt[0].name);
            txt[0].text = player.Value.NickName;
            txt[1].text = (string)player.Value.CustomProperties["Rank"];
            Debug.Log(player.Value.NickName + " Rank:" + (string)player.Value.CustomProperties["Rank"]+" //" + player.Value.CustomProperties.ContainsKey("Rank"));
            i++;

        }*/
        foreach(var r in GameManager1.instance.listRank)
        {
            images[i].gameObject.SetActive(true);
            Text[] txt = images[i].GetComponentsInChildren<Text>();
            Debug.Log(txt.Length + ":" + txt[1].name + " , " + txt[0].name);
            txt[0].text = r.Key.NickName;
            txt[1].text = r.Value;
            i++;

        }
    }
}
