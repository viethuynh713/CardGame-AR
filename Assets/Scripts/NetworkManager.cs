using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager instance;
    private Dictionary<string,GameObject> listRoom = new Dictionary<string, GameObject>();
    public GameObject roomInfoUIPrefab;
    public Transform roomInfoUIParent;
    public Text notifyTxt;
    private void Awake() {
        instance = this;
        // DontDestroyOnLoad(this.gameObject);
    }
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        notifyTxt.text = "Connected to Lobby Successfully";
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        notifyTxt.text = message;
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Room List Updated");
        Debug.Log(roomList.Count);
        foreach (var room in roomList)
        {
            if(!listRoom.ContainsKey(room.Name))
            {
                GameObject roomInfoUI = Instantiate(roomInfoUIPrefab, roomInfoUIParent);
                roomInfoUI.GetComponent<RoomInfoUI>().SetRoomInfo(room.Name);
                listRoom.Add(room.Name, roomInfoUI);
            }
            else
            {   if(room.PlayerCount == 0)
                {
                    Destroy(listRoom[room.Name]);
                    listRoom.Remove(room.Name);
                }
            }

            
            // Debug.Log(roomInfoUI.transform.position.ToString());
        }
    }

    public void JoinRoom(string name)
    {
        PhotonNetwork.JoinRoom(name);
    }
    public void CreateRoom(InputField name)
    {
        RoomOptions options = new RoomOptions()
        {
            MaxPlayers = 2,
            IsVisible = true,
            PublishUserId = true

        };
        PhotonNetwork.CreateRoom(name.text,options,TypedLobby.Default);
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
        PhotonNetwork.LoadLevel("Game");
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        Debug.Log("Left Room");
        PhotonNetwork.LoadLevel("Start");
    }
    public void Join(InputField name)
    {
        PhotonNetwork.JoinRoom(name.text);
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        notifyTxt.text = message;
    }
    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 2,IsVisible = true, PublishUserId = true }, TypedLobby.Default);
        
    }


}
