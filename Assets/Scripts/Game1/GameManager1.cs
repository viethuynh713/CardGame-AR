
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
    public List<Card1> myCard = new List<Card1>();
    private string[] suits = new string[] { "Club", "Diamond", "Spade", "Heart" };
    private string[] ranks = new string[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

    public int turn;
    private void Awake()
    {
        instance = this;
    }


    private void Start()
    {
        turn = 0;
        myCard.Clear();
        listCard.Clear();
        view = gameObject.GetComponent<PhotonView>();
        hits = new List<ARRaycastHit>();
        state = GameState.Waiting;
        roomNameTxt.text = PhotonNetwork.CurrentRoom.Name;
        countPlayer.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate("Table", Vector3.zero, Quaternion.identity);
            foreach(var s in suits)
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
                return Random.RandomRange(-1,2);
            });
            listCard.ForEach((x) =>
            {
                Debug.Log(x.rank + " "+ x.suit);
            });
            var json = JsonConvert.SerializeObject(listCard);
            Debug.Log(json);
            //view.RPC(nameof(InitMyCard), RpcTarget.All,json);
            foreach (var i in PhotonNetwork.CurrentRoom.Players)
            {
                Debug.Log(i.Key + "  //  " + i.Value.NickName);
            }
           
        }
        StartCoroutine(CreateTable());


    }

    [PunRPC]
    public void InitMyCard(string json)
    {
        Debug.Log(json);
        /*foreach(var c in JsonConvert.DeserializeObject<List<Card1>>(json))
        {
            Debug.Log(c.rank + " " + c.suit);
        }*/
    }
    IEnumerator CreateTable()
    {
        while(GameObject.FindGameObjectWithTag("Table") == null)
        {
            yield return null;
        }
        table = GameObject.FindGameObjectWithTag("Table");
    }
    /*private void Update()
    {

        Debug.Log(state.ToString());
        if (PhotonNetwork.IsMasterClient && state == GameState.Ready)
        {
            Vector2 screemPos = Camera.main.ViewportToScreenPoint(new Vector2(0.5f, 0.5f));
            if(arRaycatManager.Raycast(screemPos,hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes))
            {
                table.transform.position = hits[0].pose.position;
            }
        }
        if(state == GameState.Playing)
        {
            if ((isMasterTurn && PhotonNetwork.IsMasterClient) || (!isMasterTurn && !PhotonNetwork.IsMasterClient))
            {
                //if (Input.GetMouseButtonDown(0))
                //{
                //    Ray ray = ARcamera.ScreenPointToRay(Input.mousePosition);
                //    RaycastHit hit;
                //    if (Physics.Raycast(ray, out hit))
                //    {
                //        var cardSelected = hit.collider.GetComponent<Card>();
                //        Debug.Log(cardSelected);
                //        if (cardSelected != null)
                //        {
                //            cardSelected.view.RPC("FlipUp", RpcTarget.All);
                //            //cardSelected.Flip();
                //            //Debug.Log(target.rank + target.suit + "--" + cardSelected.rank + cardSelected.suit);
                //            StartCoroutine(CompareWithTargetCard(cardSelected));
                //        }
                //    }
                //}
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Began)
                    {
                        //Debug.Log("1211212121");
                        Ray ray = ARcamera.ScreenPointToRay(touch.position);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit))
                        {
                            var cardSelected = hit.collider.GetComponent<Card>();
                            Debug.Log(cardSelected);
                            if (cardSelected != null)
                            {
                                Debug.Log("Touch");
                                cardSelected.view.RPC("FlipUp", RpcTarget.All);
                                //cardSelected.Flip();
                                //Debug.Log(target.rank + target.suit + "--" + cardSelected.rank + cardSelected.suit);
                                StartCoroutine(CompareWithTargetCard(cardSelected));
                            }
                        }
                    }
                }
            }
        }
    }*/
    [PunRPC]
    public void ChangeTurn()
    {
        Debug.Log("Change turn");
        
    }
    

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            state = GameState.Ready;
            //view.RPC("ChangeState", RpcTarget.All, GameState.Ready);
        }
        notifyTxt.text = "Player " + newPlayer.UserId.Substring(0,3) + "joined";
        countPlayer.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        notifyTxt.text = "Player " + otherPlayer.UserId.Substring(0, 3) + "leave";
        countPlayer.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
    }
    public void StartBtn()
    {
        if(PhotonNetwork.IsMasterClient && state == GameState.Ready)
        {
            //state = GameState.Playing;
            view.RPC("ChangeState", RpcTarget.All, GameState.Playing);

            RandomListCard();
            table.GetComponent<InitBoard>().Init();
            var idx = UnityEngine.Random.RandomRange(0, listCard.Count);
            view.RPC("SetTargetCard", RpcTarget.All, listCard[idx].suit, listCard[idx].rank);
            //target = listCard[UnityEngine.Random.RandomRange(0, listCard.Count)];
            
        }
        else
        {
            notifyTxt.text = "Đừng vội =))";
        }
    }

    private void RandomListCard()
    {
        /*while(listCard.Count != 32)
        {
            string s = suits[UnityEngine.Random.RandomRange(0, suits.Length)];
            string r = ranks[UnityEngine.Random.RandomRange(0, ranks.Length)];
            var card = new Card(s, r);
            if (listCard.Contains(card)) continue;
            listCard.Add(card);
        }*/
    }
    
    [PunRPC]
    public void ChangeState(GameState st)
    {
        state = st;
        Debug.Log("Change state");
    }
    [PunRPC]
    public void EndGame()
    {
        notifyTxt.text = "You lose !!";
    }


}
