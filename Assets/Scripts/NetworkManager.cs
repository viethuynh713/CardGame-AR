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
    public Text menuNotifyTxt;
    private void Start() {
        if (!PhotonNetwork.IsConnected)

        {
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Connecting.....");
        }
        else
        {
            notifyTxt.text = "Connected";
        }
        if (instance == null)
            instance = this;
    }

    public override void OnConnected()
    {
    }
    public override void OnConnectedToMaster()
    {
        menuNotifyTxt.text = "Connected successfully";
        //notifyTxt.text = "Connected successfully";
    }
    public void JoinGameLobby(string NameLobby)
    {
        if(!PhotonNetwork.IsConnected)
        {
            
            notifyTxt.text = "Watting to connect server";
            StartCoroutine(WatingConnect(NameLobby));
            return;
        }
        TypedLobby lt = new TypedLobby(NameLobby, LobbyType.Default);
        PhotonNetwork.JoinLobby(lt);

    }
    IEnumerator WatingConnect(string NameLobby)
    {
        while (!PhotonNetwork.IsConnected)
        {
            yield return null;
        }
        Debug.Log("Connect rùi nè !!!");
        TypedLobby lt = new TypedLobby(NameLobby, LobbyType.Default);
        PhotonNetwork.JoinLobby(lt);

    }
    public override void OnJoinedLobby()
    {
        PhotonNetwork.NickName = SupperGameManager.instance.NameOfPlayer;
        notifyTxt.text = "Hello " + PhotonNetwork.NickName;
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
        if (!PhotonNetwork.IsConnected) return;
        int maxPlayer;
        switch (SupperGameManager.instance.KindOfGame)
        {
            case "Game0":
                maxPlayer = 2;
                break;
            case "Game1":
                maxPlayer = 4;
                if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Rank"))
                {
                    var hashtableProber = new ExitGames.Client.Photon.Hashtable();
                    hashtableProber["Rank"] = "";
                    PhotonNetwork.SetPlayerCustomProperties(hashtableProber);
                }
                else
                {
                    PhotonNetwork.LocalPlayer.CustomProperties["Rank"] = "";
                }
                break;
            default:
                maxPlayer = 0;
                break;
        }
        RoomOptions options = new RoomOptions()
        {
            MaxPlayers = (byte)maxPlayer,
            PublishUserId = true,
            EmptyRoomTtl = 100,


        };
        var typeLB = new TypedLobby(SupperGameManager.instance.KindOfGame, LobbyType.Default);
        PhotonNetwork.CreateRoom(name.text,options,typeLB);
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
        PhotonNetwork.LoadLevel(SupperGameManager.instance.KindOfGame);
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        Debug.Log("Left Room");
        //PhotonNetwork.LoadLevel("Menu");
    }
    public void Join(InputField name)
    {
        if (!PhotonNetwork.IsConnected) return;
        PhotonNetwork.JoinRoom(name.text);
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        notifyTxt.text = message;
    }
    public void JoinRandomRoom()
    {
        if (!PhotonNetwork.IsConnected) return;
        PhotonNetwork.JoinRandomRoom();
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        int maxPlayer;
        switch (SupperGameManager.instance.KindOfGame)
        {
            case "Game0":
                maxPlayer = 2;
                break;
            case "Game1":
                if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Rank"))
                {
                    var hashtableProber = new ExitGames.Client.Photon.Hashtable();
                    hashtableProber["Rank"] = "";
                    PhotonNetwork.LocalPlayer.SetCustomProperties(hashtableProber);
                }
                else
                {
                    PhotonNetwork.LocalPlayer.CustomProperties["Rank"] = "";
                }
                maxPlayer = 4;
                break;
            default:
                maxPlayer = 0;
                break;
        }
        RoomOptions options = new RoomOptions()
        {
            MaxPlayers = (byte)maxPlayer,
            PublishUserId = true,
            EmptyRoomTtl = 100

        };
        var typeLB = new TypedLobby(SupperGameManager.instance.KindOfGame, LobbyType.Default);
        PhotonNetwork.CreateRoom(null, options,typeLB);

    }


}
