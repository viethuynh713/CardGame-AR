
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using System.Collections;
using Newtonsoft.Json;
public class GameManager1 : MonoBehaviourPunCallbacks
{
    public static GameManager1 instance;
    [Header("UI")]
    [SerializeField] private Text notifyTxt;
    [SerializeField] private Text roomNameTxt;
    [SerializeField] private Text countPlayer;

    [Header("Prefabs")]
    public GameObject table;


    [Header("AR component")]
    [SerializeField] private ARRaycastManager arRaycatManager;
    private List<ARRaycastHit> hits;
    [SerializeField] private Camera ARcamera;
    [Header("Network")]
    private PhotonView view;

    private GameState state;
    public List<CardData> listCard = new List<CardData>();
    public List<CardData> myCardList = new List<CardData>();
    private string[] suits = new string[] { "Club", "Diamond", "Spade", "Heart" };
    private string[] ranks = new string[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

    public Player currrentTurn;
    public bool isMyTurn;
    private List<string> playerID = new List<string>();
    private List<Card> listCardsSelected;
    private int turn;

    private void Awake()
    {
        instance = this;
    }


    private void Start()
    {
        listCardsSelected = new List<Card>();
        myCardList.Clear();
        listCard.Clear();
        turn = -1;
        //PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
        //PhotonNetwork.SetPlayerCustomProperties(new ExitGames.Client.Photon.Hashtable());
        view = gameObject.GetComponent<PhotonView>();
        hits = new List<ARRaycastHit>();
        //state = GameState.Waiting;
        state = GameState.Ready;
        roomNameTxt.text = PhotonNetwork.CurrentRoom.Name;
        countPlayer.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate("Table", Vector3.zero, Quaternion.identity);           
        }
        StartCoroutine(CreateTable());
        foreach(var player in PhotonNetwork.CurrentRoom.Players)
        {
            UpdateListPlayerID(player.Value.UserId);
        }
        playerID.Sort();
        //Debug.Log(PhotonNetwork.LocalPlayer.NickName + "//" + PhotonNetwork.LocalPlayer.UserId);
        //Debug.Log(PhotonNetwork.AuthValues.UserId);
        //TestFunc();
    }

  /*  private void NetworkingClient_EventReceived(ExitGames.Client.Photon.EventData obj)
    {
        throw new System.NotImplementedException();
        PhotonNetwork.RaiseEvent(1, 0, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendUnreliable);
    }*/

    private void TestFunc()
    {
        Vector3 screemPos = ARcamera.transform.position - ARcamera.transform.forward*3 ;
        Debug.Log(screemPos);
        var obj = PhotonNetwork.Instantiate("Red_PlayingCards_Club3", screemPos, Quaternion.identity);
        obj.transform.SetParent(table.transform);
        obj.transform.position = screemPos;
        obj.transform.rotation = new Quaternion(0.0f, ARcamera.transform.rotation.y, 0.0f, ARcamera.transform.rotation.w);

    }

    [PunRPC]
    public void InitMyCardList(string json,string id)
    {

        if(id == PhotonNetwork.AuthValues.UserId)
        {
            myCardList = JsonConvert.DeserializeObject<List<CardData>>(json);
            Debug.Log(json);
            InstantiateMyCard();
        }

    }

    private void InstantiateMyCard()
    {
        foreach(var card in myCardList)
        {
            Vector2 screemPos = ARcamera.transform.position - new Vector3(0, 0.5f, -0.8f);
            var obj = PhotonNetwork.Instantiate("Red_PlayingCards_" + card.suit + card.rank, screemPos, Quaternion.identity);
            obj.GetComponent<Card>().rank = card.rank;
            obj.GetComponent<Card>().suit = card.suit;
            //obj.transform.LookAt(ARcamera.transform);

        }
    }

    IEnumerator CreateTable()
    {
        while(GameObject.FindGameObjectWithTag("Table") == null)
        {
            yield return null;
        }
        table = GameObject.FindGameObjectWithTag("Table");
    }
    private void Update()
    {

        Debug.Log(state.ToString());
        if (PhotonNetwork.IsMasterClient && state == GameState.Ready)
        {
            Vector2 screemPos = Camera.main.ViewportToScreenPoint(new Vector2(0.5f, 0.5f));
            if (arRaycatManager.Raycast(screemPos, hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes))
            {
                table.transform.position = hits[0].pose.position;
            }
        }
        if (state == GameState.Playing)
        {
            if (isMyTurn)
            {
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Began)
                    {
                        Ray ray = ARcamera.ScreenPointToRay(touch.position);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit))
                        {
                            var cardSelected = hit.collider.GetComponent<Card>();
                            Debug.Log(cardSelected);
                            if (cardSelected != null && cardSelected.view.IsMine)
                            {
                                cardSelected.view.RPC("SelectCards", RpcTarget.All);
                                if (cardSelected.isSelected)
                                {
                                    listCardsSelected.Add(cardSelected);
                                }
                                else
                                {
                                    listCardsSelected.Remove(cardSelected);
                                }
                                Debug.Log("Quantity card is selected: " + listCardsSelected.Count);
                            }
                        }
                    }
                }
            }
        }
    }
    [PunRPC]
    public void ChangeTurn()
    {
        Debug.Log("Change turn");
        turn ++;
        if (turn >= playerID.Count) turn = 0;
        if (PhotonNetwork.AuthValues.UserId == playerID[turn])
        {

            notifyTxt.text = "Your Turn ...";
            isMyTurn = true;
        }
        else
        {
            //notifyTxt.text = "Turn of " + PhotonNetwork.CurrentRoom.Players[turn].NickName;
            isMyTurn = false;
            notifyTxt.text = "Wating ...";
        }


    }
    

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (state == GameState.Waiting)
        {
            playerID.Add(newPlayer.UserId);
            playerID.Sort();
        }
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            view.RPC(nameof(ChangeState), RpcTarget.All, GameState.Ready);
        }
        notifyTxt.text = "Player " + newPlayer.NickName + " joined";
        countPlayer.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
        
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        playerID.Remove(otherPlayer.UserId);
        playerID.Sort();
        if (state == GameState.Ready)
        {
            state = GameState.Waiting;
        }
        else
        {
            state = GameState.Ready;
        }
        notifyTxt.text = "Player " + otherPlayer.NickName + " leave";
        countPlayer.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
    }
    public void StartBtn()
    {
        if (GameState.Playing == state && isMyTurn)
        {
            TransformCardSelected();
            listCardsSelected.Clear();
            view.RPC(nameof(ChangeTurn), RpcTarget.All);
        }
        else if (PhotonNetwork.IsMasterClient && state == GameState.Ready)
        {
            view.RPC(nameof(ChangeState), RpcTarget.All, GameState.Playing);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            foreach (var s in suits)
            {
                foreach (var r in ranks)
                {
                    var card = new CardData();
                    card.rank = r;
                    card.suit = s;
                    listCard.Add(card);
                }
            }
            listCard.Sort(delegate (CardData x, CardData y)
            {
                return Random.RandomRange(-1, 2);
            });
            var player = PhotonNetwork.CurrentRoom.Players[1];
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                var lstMyCard = listCard.GetRange(i * 12, 13);
                //PhotonNetwork.CurrentRoom.Players[i + 1].get
                view.RPC(nameof(InitMyCardList), RpcTarget.All, JsonConvert.SerializeObject(lstMyCard), player.UserId);
                player = player.GetNext();

            }
            //var json = JsonConvert.SerializeObject(listCard);
            view.RPC(nameof(ChangeTurn), RpcTarget.All);

        }
        else
        {
            notifyTxt.text = "Đừng vội =))";
        }
    }
    /// <summary>
    /// Move card to target position
    /// </summary>
    private void TransformCardSelected()
    {
        
    }

    [PunRPC]
    public void ChangeState(GameState st)
    {
        state = st;
        notifyTxt.text = "Change state to " + st.ToString();
    }
    [PunRPC]
    public void EndGame()
    {
        notifyTxt.text = "You lose !!";
    }
    public void UpdateListPlayerID(string id,bool isAdd = true)
    {
        if(isAdd)
        {
            if (playerID.Contains(id))
                return;
            playerID.Add(id);
        }
        else
        {
            if (playerID.Contains(id))
                playerID.Remove(id);
        }
    }



}
