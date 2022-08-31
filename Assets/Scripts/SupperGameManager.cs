using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using DG.Tweening;

public class SupperGameManager : MonoBehaviour
{
    public static SupperGameManager instance;
    [SerializeField] private UnityEngine.UI.InputField nameInF;
    private string nameofPlayer;
    public bool isConnected;
    public RectTransform[] listBtn;
    public string KindOfGame;
    public string NameOfPlayer
    {
        get { return nameofPlayer; }
    }
        
    private void Awake()
    {
        if(PhotonNetwork.NickName != "")
        {
            nameInF.text = PhotonNetwork.NickName;
        }
        if (instance == null)
        {
            //isConnected = (PlayerPrefs.HasKey("connected") && (PlayerPrefs.GetString("connected") == "true")) ? true : false;
            isConnected = false;
            nameofPlayer = nameInF.text;
            instance = this;
            //DontDestroyOnLoad(instance);
        }
    }
    private void Start()
    {
        nameInF.onEndEdit.AddListener((name) =>
        {
            if (name.Length != 0)
            {
                PhotonNetwork.NickName = name;
                nameofPlayer = name;
                Debug.Log(name.Length);
            }
        });
        if (!isConnected)
        {
            foreach (var btn in listBtn)
            {
                btn.DOAnchorPos3DY(0, 0);
                btn.DOScale(0, 0);
            }
        }
    }
    public void SelectGame(string kind)
    {
        KindOfGame = kind;
        /*if(nameofPlayer == null || PhotonNetwork.NickName != nameofPlayer)
            nameofPlayer = nameInF.text;*/
        //SceneManager.LoadScene("Lobby");
    }
    public void BackMenu()
    {
        KindOfGame = "";
    }
    public void IniButton()
    {
        int i = 0;
        foreach(var btn in listBtn)
        {
            btn.DOAnchorPos3DY(-25f + i*-115f, 1).SetEase(Ease.OutBounce);
            btn.DOScale(1, 0.1f);
            i++;
        }
    }
}
