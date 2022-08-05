using Photon.Pun;
using UnityEngine;

public class InitBoard : MonoBehaviour
{
    public GameObject pre;
    private void Start()
    {
        //Init();
    }
    public void Init()
    {
        for(int z = -1; z<3; z++)
        {
            for(int x = -3; x < 5; x++)
            {
                //Debug.Log("x " + ((x -1) * 0.2 + 0.1) + "// z" + ((z - 1) * 0.3 +0.15));
                var idx = (z + 1) * 8 + x + 3;
                var obj = PhotonNetwork.Instantiate("Red_PlayingCards_" + GameManager.instance.listCard[idx].suit + GameManager.instance.listCard[idx].rank, gameObject.transform.position, Quaternion.identity);
                
                obj.GetComponent<Card>().suit = GameManager.instance.listCard[idx].suit;
                obj.GetComponent<Card>().rank = GameManager.instance.listCard[idx].rank;
                obj.GetComponent<Card>().view.RPC("SetParent", RpcTarget.All);

                //obj.transform.position = new Vector3(0, 0.85f, 0);
                obj.GetComponent<Card>().MoveTo(new Vector3((float)((x - 1) * 0.2 + 0.1), 0.2f, (float)((z - 1) * 0.3 + 0.15)));
            }
        }
    }
}
