using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SupperGameManager : MonoBehaviour
{
    public static SupperGameManager instance;
    [SerializeField] private UnityEngine.UI.InputField nameTxt;
    private string kindOfGame;
    private string nameofPlayer;

    public string KindOfGame
    {
        get { return kindOfGame; }
    }
    public string NameOfPlayer
    {
        get { return nameofPlayer; }
    }
        
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(instance);
    }
    private void Start()
    {
        if (nameofPlayer != null)
            nameTxt.text = nameofPlayer;
    }
    public void SelectGame(string kind)
    {
        kindOfGame = kind;
        if(nameofPlayer == null || PhotonNetwork.NickName != nameofPlayer)
            nameofPlayer = nameTxt.text;
        //SceneManager.LoadScene("Lobby");
    }
}
