
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using System.Collections;
using DG.Tweening;
using System;
using UnityEngine.XR.ARSubsystems;

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
    [SerializeField] private Button restartBtn;
    [SerializeField] private Button playBtn;

    [Header("Prefabs")]
    public GameObject table;

    [Header("AR component")]
    [SerializeField] private ARRaycastManager arRaycastManager;
    private List<ARRaycastHit> hits;
    [SerializeField] private Camera ARcamera;
    [SerializeField] private ARAnchorManager arAnchorManager;
    [SerializeField] private ARPlaneManager arPlaneManager;
    [Header("Network")]
    private PhotonView view;

    private GameState state;
    public List<Card> listCard;
    private Card target;
    private string[] suits = new string[] { "Club", "Diamond", "Spade", "Heart" };
    private string[] ranks = new string[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

    private bool isMasterTurn;
    private bool isWating;
    private List<Card> listAllCard = new List<Card>();
    private bool checkAnchor = false;

    [SerializeField] private GameObject cameraOffset;
    private TrackableId anchorPosID;
    private bool isSetAnchor = false;
    [SerializeField]private GameObject flagPrefab;

    private void Awake()
    {
        if(instance == null)
        instance = this;
    }
    [PunRPC]
    public void SetCameraOffset(byte[] id1, byte[] id2)
    {
        anchorPosID = new TrackableId(BitConverter.ToUInt64(id1), BitConverter.ToUInt64(id2));
        notifyTxt.text = "Set ID successfully";
    }
    private void Start()
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            playBtn.gameObject.SetActive(false);
        }
        restartBtn.gameObject.SetActive(false);
        isMasterTurn = true;
        view = gameObject.GetComponent<PhotonView>();
        hits = new List<ARRaycastHit>();
        state = GameState.Waiting;
        roomNameTxt.text = PhotonNetwork.CurrentRoom.Name;
        countPlayer.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
        ClearOldCard();
        arPlaneManager.planesChanged += ArPlaneManager_planesChanged;


    }
    
    private void ArPlaneManager_planesChanged(ARPlanesChangedEventArgs obj)
    {
/*        Debug.Log("Gen plane");
        if (!isSetAnchor)
        {
            var anchorPos = arAnchorManager.GetAnchor(anchorPosID);
            if (anchorPos != null)
            {
                cameraOffset.transform.position = anchorPos.transform.position;
                cameraOffset.transform.rotation = anchorPos.transform.rotation;

                Instantiate(flagPrefab, anchorPos.transform);
                notifyTxt.text += " - OK";

                isSetAnchor = true;
            }
            *//*            else
                        {
                            notifyTxt.text = "AnchorPos null";
                        }*//*
        }*/
    }

    IEnumerator CreateTable()
    {
        while (GameObject.FindGameObjectWithTag("Table") == null)
        {
            yield return null;
        }
        table = GameObject.FindGameObjectWithTag("Table");
    }
    private void Update()
    {
        if (!isSetAnchor)
        {
            var anchorPos = arAnchorManager.GetAnchor(anchorPosID);
            if (anchorPos != null)
            {
                cameraOffset.transform.position = anchorPos.transform.position;
                cameraOffset.transform.rotation = anchorPos.transform.rotation;
                Instantiate(flagPrefab, anchorPos.transform);
                notifyTxt.text += " - OK";
                isSetAnchor = true;
            }

        }
        if (PhotonNetwork.IsMasterClient && !checkAnchor)
        {
            if (arRaycastManager.Raycast(Camera.main.ViewportToScreenPoint(new Vector2(0.5f, 0.5f)), hits, TrackableType.Planes))
            {

                var hitPose = hits[0].pose;
                var hitTrackableId = hits[0].trackableId;
                var hitPlane = arPlaneManager.GetPlane(hitTrackableId);
                var anchor = arAnchorManager.AttachAnchor(hitPlane, hitPose);

                if (anchor == null)
                {
                    notifyTxt.text = "Error creating anchor.";
                }
                else
                {
                    anchorPosID = anchor.trackableId;
                    notifyTxt.text = "Creating anchor.";
                    checkAnchor = true;
                    SetCameraOffset(BitConverter.GetBytes(anchorPosID.subId1), BitConverter.GetBytes(anchorPosID.subId2));
                }
            }

        }
        //Debug.Log(state.ToString());
        if (PhotonNetwork.IsMasterClient && state == GameState.Ready)
        {
            Vector2 screemPos = Camera.main.ViewportToScreenPoint(new Vector2(0.5f, 0.5f));
            if (arRaycastManager.Raycast(screemPos, hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes))
            {
                table.transform.position = hits[0].pose.position;
            }
        }
        if (state == GameState.Playing)
        {
            
            if ((isMasterTurn && PhotonNetwork.IsMasterClient) || (!isMasterTurn && !PhotonNetwork.IsMasterClient))
            {
//#if UNITY_EDITOR
/*                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = ARcamera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        var cardSelected = hit.collider.GetComponent<Card>();
                        Debug.Log(cardSelected);
                        if (cardSelected != null)
                        {
                            cardSelected.view.RPC("FlipUp", RpcTarget.All);
                            //cardSelected.Flip();
                            //Debug.Log(target.rank + target.suit + "--" + cardSelected.rank + cardSelected.suit);
                            StartCoroutine(CompareWithTargetCard(cardSelected));
                        }
                    }
                }*/
//#endif
                if (!isWating && Input.touchCount > 0)
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
                            if (cardSelected != null)
                            {
                                cardSelected.view.RPC("FlipUp", RpcTarget.MasterClient);
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
        if ((isMasterTurn && PhotonNetwork.IsMasterClient) || (!isMasterTurn && !PhotonNetwork.IsMasterClient))
        {
            notifyTxt.text = "Your turn ...";
        }
        else
        {
            notifyTxt.text = "...";
        }
    }
    IEnumerator CompareWithTargetCard(Card selected)
    {
        isWating = true;
        yield return new WaitForSeconds(1f);
        if (target.rank == selected.rank && target.suit == selected.suit)
        {

            view.RPC("ChangeState", RpcTarget.All, GameState.End);
            //Debug.Log("End Game");
            notifyTxt.text = "You win";
            if (PhotonNetwork.IsMasterClient)
                restartBtn.gameObject.SetActive(true);
            view.RPC(nameof(EndGame), RpcTarget.Others);
        }
        else
        {
            //selected.FlipDown();
            selected.view.RPC("FlipDown", RpcTarget.MasterClient);
            view.RPC("ChangeTurn", RpcTarget.All);
        }
        isWating = false;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            state = GameState.Ready;
            //view.RPC("ChangeState", RpcTarget.All, GameState.Ready);
        }
        notifyTxt.text = "Player " + newPlayer.NickName + " joined";
        countPlayer.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
        view.RPC(nameof(SetCameraOffset), newPlayer, BitConverter.GetBytes(anchorPosID.subId1), BitConverter.GetBytes(anchorPosID.subId2));
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {

        notifyTxt.text = "Player " + otherPlayer.NickName + "leave";
        view.RPC(nameof(ClearOldCard), RpcTarget.All);
        state = GameState.Waiting;
        playBtn.gameObject.SetActive(true);
        countPlayer.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/" + PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
    }
    public void StartBtn()
    {
        if (PhotonNetwork.IsMasterClient && state == GameState.Ready)
        {
            //state = GameState.Playing;
            view.RPC("ChangeState", RpcTarget.All, GameState.Playing);

            RandomListCard();
            table.GetComponent<InitBoard>().Init();
            var idx = UnityEngine.Random.RandomRange(0, listCard.Count);
            view.RPC(nameof(SetTargetCard), RpcTarget.All, listCard[idx].suit, listCard[idx].rank);
            playBtn.gameObject.SetActive(false);
            //target = listCard[UnityEngine.Random.RandomRange(0, listCard.Count)];
            Debug.Log(target.rank + target.suit);
        }
        else 
        {
            notifyTxt.text = "Waiting others player join";
        }
    }

    private void RandomListCard()
    {
        foreach(var r in ranks)
        {
            foreach(var s in suits)
            {
                listAllCard.Add(new Card(s, r));
            }
        }
        listCard.Clear();
        while (listCard.Count != 32)
        {
            var card = listAllCard[UnityEngine.Random.RandomRange(0, listAllCard.Count)];
            listCard.Add(card);
            listAllCard.Remove(card);
        }
        listAllCard.Clear();
    }
    [PunRPC]
    public void SetTargetCard(string suit, string rank)
    {
        //Animation
        targetCardImg.GetComponent<RectTransform>().DORotate(new Vector3(0, -180, 0), 0);
        Sequence sq = DOTween.Sequence();
        sq.Append(targetCardImg.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-400, -400), 1.5f));
        sq.Insert(0, targetCardImg.GetComponent<RectTransform>().DOScale(new Vector3(3, 3, 3), 1.5f));
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
        if(st == GameState.Ready)
        {

            notifyTxt.text = "All player are ready";
        }
    }
    [PunRPC]
    public void EndGame()
    {
        notifyTxt.text = "You lose !!";
        if (PhotonNetwork.IsMasterClient)
            restartBtn.gameObject.SetActive(true);

    }
    public void GotoMenu()
    {
        //if (state == GameState.Playing) return;
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("Menu");
    }
    public void RestartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            restartBtn.gameObject.SetActive(false);
            isMasterTurn = true;
            listCard.Clear();
            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                view.RPC(nameof(ChangeState), RpcTarget.All, GameState.Ready);
                //state = GameState.Ready;
            }
            else
            {
                //state = GameState.Waiting;
                view.RPC(nameof(ChangeState), RpcTarget.All, GameState.Waiting);
                playBtn.gameObject.SetActive(true);
            }

            view.RPC(nameof(ClearOldCard), RpcTarget.All);
        }

    }
    [PunRPC]
    public void ClearOldCard()
    {
        foreach (var card in GameObject.FindGameObjectsWithTag("Card"))
        {
            Destroy(card);
        }
    }


}
