using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public void SelectGame(string kind)
    {
        kindOfGame = kind;
        nameofPlayer = nameTxt.text;
        //SceneManager.LoadScene("Lobby");
    }
}
