using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomInfoUI : MonoBehaviour
{
    // Start is called before the first frame update
    public Button joinBtn;
    public Text roomNameTxt;

    public void SetRoomInfo(string roomName)
    {
        roomNameTxt.text = roomName;
        
        joinBtn.onClick.AddListener(() => {
            NetworkManager.instance.JoinRoom(roomName);
        });
    }

}
