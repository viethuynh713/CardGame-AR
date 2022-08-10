using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;

public class Card : MonoBehaviour
{
    public string suit;
    public string rank;
    public bool isSelected;
    public PhotonView view;
    private void Awake()
    {
        isSelected = false;
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

    public void SetInitValueGame1(string suit, string rank)
    {
        this.rank = rank;
        this.suit = suit;

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

    [PunRPC]
    public void SelectCards()
    {
        if(isSelected)
        {
            isSelected = false;
        }
        else
        {
            isSelected = true;
        }
    }
}
