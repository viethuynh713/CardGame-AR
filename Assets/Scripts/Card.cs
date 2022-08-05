using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;

public class Card : MonoBehaviour
{
    public string suit;
    public string rank;
    public PhotonView view;
    private void Start()
    {
        view = gameObject.GetComponent<PhotonView>();
        transform.localPosition = new Vector3(0,0.85f,0);
        transform.localEulerAngles = new Vector3(0, 0, 180);
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
    public void Flip()
    {
        Debug.Log("Flip....,.,.,.");
        Sequence sq = DOTween.Sequence();
        sq.Append(transform.DOLocalMoveY(1f, 0.2f));

        if(transform.localEulerAngles.z >179)
        {
            sq.Append(transform.DOLocalRotate(Vector3.zero, 0.2f));
        }
        else
        {
            sq.Append(transform.DOLocalRotate(new Vector3(0,0,180), 0.2f));
        }

        sq.Append(transform.DOLocalMoveY(0.85f, 0.2f));
        //view.RPC("Flip", RpcTarget.Others);
    }
    [PunRPC]
    public void SetParent()
    {
        gameObject.transform.SetParent(GameManager.instance.table.transform);
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
}
