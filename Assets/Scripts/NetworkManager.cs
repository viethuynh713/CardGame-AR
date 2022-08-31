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

    public GameObject menuPnl;
    public GameObject lobbyPnl;
    private void Awake() {
        Debug.Log(PhotonNetwork.NickName);
        if (!SupperGameManager.instance.isConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = "1.0";
            menuNotifyTxt.text = "Connecting.....";
        }
        else
        {
            menuNotifyTxt.text = "Connected";
        }
        if (instance == null)
            instance = this;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        //PlayerPrefs.SetString("connected", "false");
    }
    public override void OnConnectedToMaster()
    {
        if (!SupperGameManager.instance.isConnected)
        {
            //PlayerPrefs.SetString("connected", "true");
            menuNotifyTxt.text = "Connected successfully";
            PhotonNetwork.NickName = SupperGameManager.instance.NameOfPlayer.Length != 0 ? SupperGameManager.instance.NameOfPlayer : "Player " + Random.RandomRange(0, 100).ToString();
            SupperGameManager.instance.isConnected = true;
            SupperGameManager.instance.IniButton();


        }
    }
    public void JoinGameLobby(string NameLobby)
    {
        if(SupperGameManager.instance.isConnected)
        {
            menuPnl.SetActive(false);
            lobbyPnl.SetActive(true);
            TypedLobby lt = new TypedLobby(NameLobby, LobbyType.Default);
            PhotonNetwork.JoinLobby(lt);
        }
        else
        {
            menuNotifyTxt.text = "Waiting to connect server";

        }

    }
/*    IEnumerator WatingConnect(string NameLobby)
    {
        while (!PhotonNetwork.IsConnected)
        {
            yield return null;
        }
        Debug.Log("Connect rùi nè !!!");
        TypedLobby lt = new TypedLobby(NameLobby, LobbyType.Default);
        PhotonNetwork.JoinLobby(lt);

    }*/
    public override void OnJoinedLobby()
    {
        notifyTxt.text = "Welcome " + PhotonNetwork.NickName;
        foreach(var room in listRoom)
        {
            Destroy(room.Value);
        }
        listRoom.Clear();
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
        }
    }

    public void JoinRoom(string name)
    {
        PhotonNetwork.JoinRoom(name);
    }
    public void CreateRoom(InputField name)
    {
        if (!SupperGameManager.instance.isConnected) return;
        int maxPlayer;
        switch (SupperGameManager.instance.KindOfGame)
        {
            case "Game0":
                maxPlayer = 2;
                break;
            case "Game1":
                maxPlayer = 4;
/*                if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Rank"))
                {
                    var hashtableProber = new ExitGames.Client.Photon.Hashtable();
                    hashtableProber["Rank"] = "";
                    PhotonNetwork.SetPlayerCustomProperties(hashtableProber);
                }
                else
                {
                    PhotonNetwork.LocalPlayer.CustomProperties["Rank"] = "";
                }*/
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
        PhotonNetwork.CreateRoom(name.text != "" ?name.text : "Room " + Random.Range(0, 100).ToString(), options,typeLB);
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
  /*  public void Join(InputField name)
    {
        if (!PhotonNetwork.IsConnected) return;
        PhotonNetwork.JoinRoom((name.text != "" && name.text != null) ? name.text :"Room " + Random.RandomRange(0, 100).ToString());
    }*/
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        int maxPlayer;
        switch (SupperGameManager.instance.KindOfGame)
        {
            case "Game0":
                maxPlayer = 2;
                break;
            case "Game1":
               /* if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Rank"))
                {
                    var hashtableProber = new ExitGames.Client.Photon.Hashtable();
                    hashtableProber["Rank"] = "";
                    PhotonNetwork.LocalPlayer.SetCustomProperties(hashtableProber);
                }
                else
                {
                    PhotonNetwork.LocalPlayer.CustomProperties["Rank"] = "";
                }*/
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
        var roomName = "Room " + Random.Range(0, 100).ToString();
        var typeLB = new TypedLobby(SupperGameManager.instance.KindOfGame, LobbyType.Default);
        PhotonNetwork.CreateRoom(roomName, options, typeLB);
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
        var roomName = "Room " + Random.RandomRange(0, 100).ToString();
        var typeLB = new TypedLobby(SupperGameManager.instance.KindOfGame, LobbyType.Default);
        PhotonNetwork.CreateRoom(roomName, options,typeLB);

    }


}
