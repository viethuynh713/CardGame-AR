
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using System.Collections;
using DG.Tweening;

public enum GameState
{
    Waiting,
    Ready,
    Playing,
    End
}
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;
    [Header("UI")]
    [SerializeField] private Text notifyTxt;
    [SerializeField] private Text roomNameTxt;
    [SerializeField] private Text countPlayer;
    [SerializeField] private RawImage targetCardImg;

    [Header("Prefabs")]
    public GameObject table;

    [Header("AR component")]
    [SerializeField] private ARRaycastManager arRaycatManager;
    private List<ARRaycastHit> hits;
    [SerializeField] private Camera ARcamera;
    [Header("Network")]
    private PhotonView view;

    private GameState state;
    public List<Card> listCard;
    private Card target;
    private string[] suits = new string[] { "Club", "Diamond", "Spade", "Heart" };
    private string[] ranks = new string[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

    private bool isMasterTurn;

    private void Awake()
    {
        instance = this;
    }


    private void Start()
    {
        isMasterTurn = true;
        view = gameObject.GetComponent<PhotonView>();
        hits = new List<ARRaycastHit>();
        state = GameState.Waiting;
        //view.RPC("ChangeState", RpcTarget.All, GameState.Waiting);
        roomNameTxt.text = PhotonNetwork.CurrentRoom.Name;
        countPlayer.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate("Table", Vector3.zero, Quaternion.identity);
        }
        StartCoroutine(CreateTable());

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
                                cardSelected.FlipUp();
                                //cardSelected.Flip();
                                //Debug.Log(target.rank + target.suit + "--" + cardSelected.rank + cardSelected.suit);
                                StartCoroutine(CompareWithTargetCard(cardSelected));
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
        isMasterTurn = !isMasterTurn;
    }
    IEnumerator CompareWithTargetCard(Card selected)
    {
        yield return new WaitForSeconds(1.5f);
        if(target.suit == selected.suit && target.rank == selected.rank)
        {
            // state = GameState.End;
            view.RPC("ChangeState", RpcTarget.All, GameState.End);
            //Debug.Log("End Game");
            notifyTxt.text = "You win";
            view.RPC("EndGame", RpcTarget.Others);
        }
        else
        {
            selected.FlipDown();
            view.RPC("ChangeTurn", RpcTarget.All);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            state = GameState.Ready;
            //view.RPC("ChangeState", RpcTarget.All, GameState.Ready);
        }
        notifyTxt.text = "Player " + newPlayer.NickName + " joined";
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
            Debug.Log(target.rank + target.suit);
        }
        else
        {
            notifyTxt.text = "Đừng vội =))";
        }
    }

    private void RandomListCard()
    {
        while(listCard.Count != 32)
        {
            string s = suits[UnityEngine.Random.RandomRange(0, suits.Length)];
            string r = ranks[UnityEngine.Random.RandomRange(0, ranks.Length)];
            var card = new Card(s, r);
            if (listCard.Contains(card)) continue;
            listCard.Add(card);
        }
    }
    [PunRPC]
    public void SetTargetCard(string suit,string rank)
    {
        //-90/-100/0
        targetCardImg.GetComponent<RectTransform>().DORotate(new Vector3(0, -180, 0), 0);
        //targetCardImg.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-400, -400), 0);
        Sequence sq = DOTween.Sequence();
        sq.Append(targetCardImg.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-400, -400), 1.5f));
        sq.Insert(0,targetCardImg.GetComponent<RectTransform>().DOScale(new Vector3(3,3,3), 1.5f));
        sq.Append(targetCardImg.GetComponent<RectTransform>().DORotate(new Vector3(0, -90, 0), 0.2f));
        sq.AppendCallback(() =>
        {
            targetCardImg.texture = Resources.Load<Texture>("Image/" + suit + rank);
        });
        sq.Append(targetCardImg.GetComponent<RectTransform>().DORotate(new Vector3(0, 0, 0), 0.2f));
        //sq.PrependInterval(0.5f);
        sq.Append(targetCardImg.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-90, -100), 1));
        sq.Insert(2f, targetCardImg.GetComponent<RectTransform>().DOScale(new Vector3(1, 1, 1), 1));


        target = new Card(suit, rank);
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
