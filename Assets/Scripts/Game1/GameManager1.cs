
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

    public int turn;
    private void Awake()
    {
        instance = this;
    }


    private void Start()
    {

        turn = 0;
        if(PhotonNetwork.LocalPlayer.UserId == PhotonNetwork.CurrentRoom.Players[turn].UserId)
        {
            notifyTxt.text = "Your Turn ...";
        }
        else
        {
            notifyTxt.text = "Turn of " + PhotonNetwork.CurrentRoom.Players[turn].NickName;
        }
        myCardList.Clear();
        listCard.Clear();
        view = gameObject.GetComponent<PhotonView>();
        hits = new List<ARRaycastHit>();
        state = GameState.Waiting;
        roomNameTxt.text = PhotonNetwork.CurrentRoom.Name;
        countPlayer.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate("Table", Vector3.zero, Quaternion.identity);           
        }
        StartCoroutine(CreateTable());

        TestFunc();
    }

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
        if(id == PhotonNetwork.LocalPlayer.UserId)
        {
            myCardList = JsonConvert.DeserializeObject<List<CardData>>(json);
            InstantiateMyCard();
        }

    }

    private void InstantiateMyCard()
    {
        foreach(var card in myCardList)
        {
            Vector2 screemPos = ARcamera.transform.position - new Vector3(0, 0.5f, -0.8f);
            var obj = PhotonNetwork.Instantiate("Red_PlayingCards_" + card.suit + card.rank, screemPos, Quaternion.identity);
            obj.transform.LookAt(ARcamera.transform);

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
        turn ++;
        if (turn > PhotonNetwork.CurrentRoom.PlayerCount) turn = 0;
        if (PhotonNetwork.LocalPlayer.UserId == PhotonNetwork.CurrentRoom.Players[turn].UserId)
        {
            notifyTxt.text = "Your Turn ...";
        }
        else
        {
            notifyTxt.text = "Turn of " + PhotonNetwork.CurrentRoom.Players[turn].NickName;
        }


    }
    

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            view.RPC(nameof(ChangeState), RpcTarget.All, GameState.Ready);
        }
        notifyTxt.text = "Player " + newPlayer.NickName + " joined";
        countPlayer.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        notifyTxt.text = "Player " + otherPlayer.NickName + " leave";
        countPlayer.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
    }
    public void StartBtn()
    {
        view.RPC(nameof(ChangeTurn), RpcTarget.All);
        /*if(PhotonNetwork.IsMasterClient && state == GameState.Ready)
        {
            view.RPC(nameof(ChangeState), RpcTarget.All, GameState.Playing);
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
            for(int i = 0; i< PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                var lstMyCard = listCard.GetRange(i * 12, 12);

                view.RPC(nameof(InitMyCardList), RpcTarget.All, JsonConvert.SerializeObject(lstMyCard),PhotonNetwork.CurrentRoom.Players[i + 1].UserId);

            }    
            var json = JsonConvert.SerializeObject(listCard);

        }
        else
        {
            notifyTxt.text = "Đừng vội =))";
        }*/
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


}
