
using DG.Tweening;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
public class GameManager1 : MonoBehaviourPunCallbacks
{
    public static GameManager1 instance;
    [Header("UI")]
    [SerializeField] private Text notifyTxt;
    [SerializeField] private Text roomNameTxt;
    [SerializeField] private Text countPlayer;
    [SerializeField] private GameObject endGamePnl;

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
    public GameObject pointSpawn;
    public Collider wall;

    public int countCard;
    public string rank;

    private void Awake()
    {
        instance = this;
    }
    public Dictionary<Player, string> listRank = new Dictionary<Player, string>();


    private void Start()
    {

        foreach(var c in GameObject.FindGameObjectsWithTag("Card"))
        {
            Destroy(c);
        }
        foreach(var c in GameObject.FindGameObjectsWithTag("PointSpawn"))
        {
            Destroy(c);
        }
        ableUndoListCard.Clear();
        listRank.Clear();
        endGamePnl.SetActive(false);
        posY = 0;
        rank = "1st";
        listCardsSelected = new List<Card>();
        myCardList.Clear();
        listCard.Clear();
        turn = -1;
        //PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
        //PhotonNetwork.SetPlayerCustomProperties(new ExitGames.Client.Photon.Hashtable());
        view = gameObject.GetComponent<PhotonView>();
        hits = new List<ARRaycastHit>();
        state = GameState.Waiting;
        roomNameTxt.text = PhotonNetwork.CurrentRoom.Name;
        countPlayer.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
 /*       if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate("Table", Vector3.zero, Quaternion.identity);           
        }*/
        //StartCoroutine(CreateTable());
        foreach(var player in PhotonNetwork.CurrentRoom.Players)
        {
            UpdateListPlayerID(player.Value.UserId);
        }
        playerID.Sort();
        Debug.Log(PhotonNetwork.AuthValues.UserId);
    }

  /*  private void NetworkingClient_EventReceived(ExitGames.Client.Photon.EventData obj)
    {
        throw new System.NotImplementedException();
        PhotonNetwork.RaiseEvent(1, 0, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendUnreliable);
    }*/


    [PunRPC]
    public void InitMyCardList(string json,string id)
    {

        if(id == PhotonNetwork.AuthValues.UserId)
        {
            myCardList = JsonConvert.DeserializeObject<List<CardData>>(json);
            countCard = myCardList.Count;
            Debug.Log(json);
            InstantiateMyCard();
        }

    }

    private void InstantiateMyCard()
    {
        Vector2 screemPos = Camera.main.ViewportToScreenPoint(new Vector2(0.5f, 0.2f));
        Ray ray = ARcamera.ScreenPointToRay(screemPos);
        RaycastHit hit;
        if(Physics.Raycast(ray,out hit))
        {
            wall = hit.collider;
            
            if(wall != null && wall.tag == "Wall")
            {
                pointSpawn = PhotonNetwork.Instantiate("PointSpawnCard", wall.transform.position, wall.transform.rotation);
                wall.enabled = false;
                pointSpawn.GetComponent<PointSpawn>().ChangeName(PhotonNetwork.AuthValues.UserId);
                pointSpawn.transform.eulerAngles = new Vector3(pointSpawn.transform.eulerAngles.x -80, pointSpawn.transform.eulerAngles.y, pointSpawn.transform.eulerAngles.z);
            }
        }
        if(pointSpawn != null)
        {
            for(int i = 0; i<myCardList.Count;i++)
            {
                var obj = PhotonNetwork.Instantiate("Red_PlayingCards_" + myCardList[i].suit + myCardList[i].rank, pointSpawn.transform.position,pointSpawn.transform.rotation);
                obj.GetComponent<Card>().view.RPC("SetInitValueGame1", RpcTarget.All, myCardList[i].suit, myCardList[i].rank, PhotonNetwork.AuthValues.UserId);
                obj.transform.localPosition = new Vector3(-0.16f + 0.03f * i, i * 0.001f, 0);
                
            }


        }
    }

    /*    IEnumerator CreateTable()
        {
            while(GameObject.FindGameObjectWithTag("Table") == null)
            {
                yield return null;
            }
            table = GameObject.FindGameObjectWithTag("Table");
        }*/
    public Card cardMove;
    private void Update()
    {
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
                            if (cardSelected != null && cardSelected.view.IsMine && cardSelected.cardState != CardState.Disable)
                            {
                                cardMove = cardSelected;
                                if (cardSelected.cardState == CardState.Normal)
                                {
                                    listCardsSelected.Add(cardSelected);
                                    cardSelected.cardState = CardState.Selected;
                                }
                                else if(cardSelected.cardState == CardState.Selected)
                                {
                                    listCardsSelected.Remove(cardSelected);
                                    cardSelected.cardState = CardState.Normal;
                                }
                                cardSelected.HandleSelect();
                                //Debug.Log("Quantity card is selected: " + listCardsSelected.Count);
                            }
                        }
                    }
                    if (touch.phase == TouchPhase.Moved)
                    {
                        Ray ray = ARcamera.ScreenPointToRay(touch.position);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit))
                        {
                            var cardSelected = hit.collider.GetComponent<Card>();
                            Debug.Log(cardSelected);
                            if (cardSelected != null && cardSelected.view.IsMine && cardSelected.cardState != CardState.Disable)
                            {
                                if(Vector3.Distance(cardMove.transform.localPosition,cardSelected.transform.localPosition)>0.01f)
                                {
                                    var temp = cardMove.transform.localPosition;
                                    cardMove.transform.localPosition = new Vector3(cardSelected.transform.localPosition.x, cardSelected.transform.localPosition.y, cardMove.transform.localPosition.z);
                                    cardSelected.transform.localPosition = new Vector3(temp.x, temp.y, cardSelected.transform.localPosition.z);
                                }
                                Debug.Log("Mouse move" + cardSelected.rank);
                            }
                        }
                    }
                }
                if (Input.touchCount == 2)
                {
                    pointSpawn.transform.position = wall.transform.position;
                    pointSpawn.transform.eulerAngles = new Vector3(wall.transform.eulerAngles.x -80, wall.transform.eulerAngles.y, wall.transform.eulerAngles.z);
                }
            }
        }
    }
    [PunRPC]
    public void ChangeTurn(bool isRevert = false)
    {
        Debug.Log("Change turn");
        if (isRevert)
        {
            turn = turn - 1 >= 0 ? turn - 1 : playerID.Count -1;
        }
        else
        {
            turn++;
            if (turn >= playerID.Count) turn = 0;
        }
        if (PhotonNetwork.LocalPlayer.UserId == playerID[turn])
        {
            if (countCard == 0)
            {
                notifyTxt.text = "Rank " + rank;
                isMyTurn = false;
                view.RPC(nameof(ChangeTurn), RpcTarget.Others, false);
                return;
            }
            notifyTxt.text = "Your Turn ...";
            isMyTurn = true;

        }
        else
        {
            isMyTurn = false;
            notifyTxt.text = "Wating ...";
        }
    }
    

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {

            playerID.Add(newPlayer.UserId);
            playerID.Sort();
        
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2)
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
        Debug.Log(PhotonNetwork.CurrentRoom.PlayerCount);
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
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
            view.RPC(nameof(ChangeTurn), RpcTarget.All,false);
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
            Player player = PhotonNetwork.MasterClient;          
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                var lstMyCard = listCard.GetRange(i * 13, 13);
                //PhotonNetwork.CurrentRoom.Players[i + 1].get
                view.RPC(nameof(InitMyCardList), RpcTarget.All, JsonConvert.SerializeObject(lstMyCard), player.UserId);
                player = player.GetNext();

            }
            //var json = JsonConvert.SerializeObject(listCard);
            view.RPC(nameof(ChangeTurn), RpcTarget.All,false);

        }
        else
        {
            notifyTxt.text = "Đừng vội =))";
        }
    }
    /// <summary>
    /// Move card to target position
    /// </summary>
    private float posY;
    Dictionary<Card,Vector3> ableUndoListCard = new Dictionary<Card, Vector3>();
    private void TransformCardSelected()
    {
        ableUndoListCard.Clear();
        var RandomPos = new Vector3(Random.RandomRange(-0.2f, 0.2f), posY, Random.RandomRange(-0.2f, 0.2f));
        for(int i = 0; i< listCardsSelected.Count; i++)
        {
            ableUndoListCard.Add(listCardsSelected[i], listCardsSelected[i].transform.localPosition);
            var newPos = table.transform.position + new Vector3(0, i * 0.0005f, 0.03f * i) + RandomPos;
            listCardsSelected[i].cardState = CardState.Disable;
            listCardsSelected[i].transform.DOMove(newPos, 2);
            listCardsSelected[i].transform.DORotate(new Vector3(0, 90, 0), 2);

        }
        posY += 0.01f;
        countCard -= listCardsSelected.Count;
        if(countCard == 0)
        {
            PhotonNetwork.LocalPlayer.CustomProperties["Rank"] = rank;
            view.RPC(nameof(ChoseRank), RpcTarget.All, PhotonNetwork.LocalPlayer,rank,false);

        }
    }
    public void UndoCard()
    {
        var index = turn - 1 >= 0 ? turn - 1 : playerID.Count -1;
        if(playerID[index] == PhotonNetwork.LocalPlayer.UserId)
        {
            foreach(var card in ableUndoListCard)
            {
                card.Key.transform.localPosition = new Vector3(card.Value.x,card.Value.y,card.Value.z - 0.05f);
                card.Key.transform.localEulerAngles = Vector3.zero;
                card.Key.cardState = CardState.Normal;
            }
            if(countCard == 0)
            {
                countCard += ableUndoListCard.Count;
                view.RPC(nameof(ChoseRank), RpcTarget.All,PhotonNetwork.LocalPlayer,rank,true);

            }
            else
            {
                countCard += ableUndoListCard.Count;
            }
            ableUndoListCard.Clear();
            view.RPC(nameof(ChangeTurn), RpcTarget.All, true);
        }
    }
    [PunRPC]
    public void ChangeState(GameState st)
    {
        state = st;
        notifyTxt.text = "Change state to " + st.ToString();
    }
    [PunRPC]
    public void ChoseRank(Player id,string r,bool isRevert)
    {
        if (!isRevert)
        {
            listRank[id] = r;
            notifyTxt.text = "Rank " + rank;
            if (countCard != 0)
            {
                switch (rank)
                {
                    case "1st":
                        rank = "2nd";
                        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
                        {
                            PhotonNetwork.LocalPlayer.CustomProperties["Rank"] = rank;
                            view.RPC(nameof(ChoseRank), RpcTarget.All, PhotonNetwork.LocalPlayer, rank,false);
                            view.RPC(nameof(EndGame), RpcTarget.All);
                        }
                        break;
                    case "2nd":
                        rank = "3th";
                        if (PhotonNetwork.CurrentRoom.PlayerCount == 3)
                        {
                            PhotonNetwork.LocalPlayer.CustomProperties["Rank"] = rank;
                            view.RPC(nameof(ChoseRank), RpcTarget.All, PhotonNetwork.LocalPlayer, rank,false);
                            view.RPC(nameof(EndGame), RpcTarget.All);
                        }
                        break;
                    case "3th":
                        rank = "4th";
                        if (PhotonNetwork.CurrentRoom.PlayerCount == 4)
                        {
                            PhotonNetwork.LocalPlayer.CustomProperties["Rank"] = rank;
                            view.RPC(nameof(ChoseRank), RpcTarget.All, PhotonNetwork.LocalPlayer, rank,false);
                            view.RPC(nameof(EndGame), RpcTarget.All);
                        }
                        break;
                    default:
                        rank = "...";
                        break;
                }
            }
        }
        else
        {
            listRank.Remove(id);
            rank = r;
        }
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

    [PunRPC]
    public void EndGame()
    {
        state = GameState.End;
        notifyTxt.text = "Endgame";
        PhotonNetwork.CurrentRoom.IsOpen = true;
        PhotonNetwork.CurrentRoom.IsVisible = true;
        endGamePnl.SetActive(true);
        endGamePnl.GetComponent<HandleEndGame>().Init();
        endGamePnl.GetComponent<HandleEndGame>().SetRank();
    }   
    public void RestartBtn()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            view.RPC(nameof(InitGame), RpcTarget.All);
        }
    }
    [PunRPC]
    public void InitGame()
    {
        listRank.Clear();
        ableUndoListCard.Clear();
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            state = GameState.Waiting;
        }
        else
        {
            state = GameState.Ready;
        }
        endGamePnl.SetActive(false);
        foreach(var sp in GameObject.FindGameObjectsWithTag("PointSpawn"))
        {
            Destroy(sp);
        }
        posY = 0;
        rank = "1st";
        listCardsSelected.Clear();
        myCardList.Clear();
        listCard.Clear();
        turn = -1;
        wall.enabled = true;
        
    }
    public void HomeBtn()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("Menu");
    }
}
