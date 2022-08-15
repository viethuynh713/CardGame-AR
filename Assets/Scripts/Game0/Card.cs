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
    private void Awake()
    {
        cardState = CardState.Normal;
        view = gameObject.GetComponent<PhotonView>();
    }
    public Card(string suit,string rank)
    {
        this.suit = suit;
        this.rank = rank;
    }
    public void MoveTo(Vector3 pos)
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

    public override bool Equals(object other)
    {
        if(other.GetType().Equals(this.GetType()))
        {
            Card c = (Card)other;
            return (c.suit == this.suit && c.rank == this.rank);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return base.ToString();
    }
    public void HandleSelect()
    {
        if(cardState == CardState.Selected)
        {
            transform.DOLocalMoveZ(transform.localPosition.z + 0.05f, 0.5f);
        }
        else
        {
            transform.DOLocalMoveZ(transform.localPosition.z - 0.05f, 0.5f);
        }
    }
}
