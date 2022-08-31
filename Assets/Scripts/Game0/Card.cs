using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;

public enum CardState
{
    Normal,
    Selected,
    Disable
}
public class Card : MonoBehaviour
{
    public string suit;
    public string rank;
    public CardState cardState;
    public PhotonView view;
    public PhotonTransformView mytransform;
    private void Awake()
    {
        cardState = CardState.Normal;
        view = gameObject.GetComponent<PhotonView>();
        mytransform = gameObject.GetComponent<PhotonTransformView>();
    }
    public Card(string suit,string rank)
    {
        this.suit = suit;
        this.rank = rank;
    }
    public void MoveLocalTo(Vector3 pos)
    {
        transform.DOLocalMove(pos, 2f).SetEase(Ease.OutCubic);
    }
    [PunRPC]
    public void FlipDown()
    {
        Sequence sq = DOTween.Sequence();
        sq.Append(transform.DOLocalMoveY(0.4f, 0.2f));
        sq.Append(transform.DOLocalRotate(new Vector3(0,0,180), 0.2f));
        sq.Append(transform.DOLocalMoveY(0.2f, 0.2f));
    }
    [PunRPC]
    public void FlipUp()
    {
        Sequence sq = DOTween.Sequence();
        sq.Append(transform.DOLocalMoveY(0.4f, 0.2f));
        sq.Append(transform.DOLocalRotate(Vector3.zero, 0.2f));
        sq.Append(transform.DOLocalMoveY(0.2f, 0.2f));
    }
    [PunRPC]
    public void SetInitValue(string suit, string rank)
    {
        this.suit = suit;
        this.rank = rank;
        Debug.Log("SetParent");
        transform.localPosition = new Vector3(0,0.2f,0);
        transform.localEulerAngles = new Vector3(0, 0, 180);
        gameObject.transform.SetParent(GameManager.instance.table.transform);
    }
    [PunRPC]
    public void SetInitValueGame1(string suit, string rank,string id)
    {
        this.suit = suit;
        this.rank = rank;
        Debug.Log("SetParent");
        var point = GameObject.Find(id);
        if(point != null)gameObject.transform.SetParent(point.transform);
        
    }

  /*  public static bool operator ==(Card lhs, Card rhs)
    {
        if (lhs == null || rhs == null) return false;
        if (lhs.rank == rhs.rank && lhs.suit == rhs.suit) return true;
        return false;
    }
    public static bool operator !=(Card lhs,Card rhs)
    {
        if (lhs == null || rhs == null) return false;
        if (lhs.rank != rhs.rank || lhs.suit != rhs.suit) return true;
        return false;
    }*/
    public void HandleSelect()
    {
        if(cardState == CardState.Selected)
        {
            transform.DOLocalMoveZ(0.05f, 0.5f);
        }
        else
        {
            transform.DOLocalMoveZ(0, 0.5f);
        }
        var nameofPoint = GameManager1.instance.pointSpawn.name;
        view.RPC(nameof(setParent),RpcTarget.All, nameofPoint, cardState);
    }
    [PunRPC]
    public void setParent(string name,CardState state)
    {
        if(state == CardState.Disable)
        {
            transform.SetParent(PhotonView.Find(GameManager1.instance.table.GetPhotonView().ViewID).transform);
            Debug.Log(PhotonView.Find(GameManager1.instance.table.GetPhotonView().ViewID));
        }
        else
        {
            transform.SetParent(GameObject.Find(name).transform);

        }

    }

}
