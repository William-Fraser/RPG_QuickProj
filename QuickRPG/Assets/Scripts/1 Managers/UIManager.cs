using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Canvas titleMenu;
    public Canvas connectServer;
    public Canvas matchmaking;
    public Canvas gameSetup;
    public Canvas gamePlay;
    public Canvas gameEndMenu;
    public Canvas saveMenu;
    public Canvas loadMenu;
    public Canvas pause;
    public Canvas options;
    public Canvas credits;

    public void DisableAll()
    {
        titleMenu.enabled = false;
        connectServer.enabled = false;
        matchmaking.enabled = false;
        gameSetup.enabled = false;
        gamePlay.enabled = false;
        gameEndMenu.enabled = false;
        saveMenu.enabled = false;
        loadMenu.enabled = false;
        pause.enabled = false;
        options.enabled = false;
        credits.enabled = false;
    }

    public void LoadTitleMenu()
    {
        DisableAll();
        titleMenu.enabled = true;
    }
    public void LoadConnectServer()
    {
        DisableAll();
        connectServer.enabled = true;
    }

    public void LoadMatchmaking()
    {
        DisableAll();
        matchmaking.enabled = true;
    }

    public void LoadGameSetup()
    {
        DisableAll();
        gameSetup.enabled = true;
    }

    public void LoadGameplay()
    {
        DisableAll();
        gamePlay.enabled = true;
    }
    public void LoadGameEnd()
    {
        gameEndMenu.enabled = true;
    }

    public void LoadSaveMenu()
    {
        saveMenu.enabled = true;
    }

    public void LoadLoadMenu()
    {
        loadMenu.enabled = true;
    }

    public void LoadPauseScreen()
    {
        DisableAll();
        pause.enabled = true;
    }

    public void LoadOptions()
    {
        options.enabled = true;
    }

    public void LoadCredits()
    {
        credits.enabled = true;
    }
}


