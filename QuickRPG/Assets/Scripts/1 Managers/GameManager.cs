using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// game manager
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;


//anticipation, abstraction, adaptation

public enum GameState
{ 
    MAINMENU,
    CONNECTSERVER,
    MATCHMAKING,
    GAMESETUP,
    GAMEPLAY,
    GAMEEND,
    SAVE,
    LOAD,
    PAUSE,
    OPTIONS,
    CREDITS
}

public class GameManager : MonoBehaviour
{
    //singleton
    public static GameManager manager;

    //public var
    public LevelManager levelManager;
    public UIManager uiManager;

    public GameState gameState;

    //private
    private string datapathExt = "/save.dat";
    private string[] datapathRoute = new string[21]; // 20 file slots, 0 is for settings

    //gets/sets
    public GameState GameState { get { return gameState; } }

    private void Awake()
    {
        // handle singleton
        if (manager == null)
        {
            DontDestroyOnLoad(this.gameObject);
            manager = this; // setting this object to be THE singleton
        }
        else if (manager != this) // already exist's? DESTROY
        {
            Destroy(this.gameObject);
        }

        // set up datapath routes
        for (int i = 0; i >= 20; i++)
        {
            datapathRoute[i] = $"/{i}";
        }

        gameState = GameState.MAINMENU;
    }

    private void Start()
    {
        //load options
        Load(0);
    }
    private bool finishchange;
    private void Update()
    {
        ControlPauseGame();

        switch (gameState)
        {
            case GameState.MAINMENU:
                {
                    ChangeScene(GameState.MAINMENU.ToString()); // find scenes better somehow

                    uiManager.LoadTitleMenu(); // menus handled by UI and LevelManagers button controls

                    finishchange = false;
                    return;
                }
            case GameState.CONNECTSERVER:
                {
                    ChangeScene(GameState.CONNECTSERVER.ToString());

                    uiManager.LoadConnectServer(); // menus handled by UI and LevelManagers button controls

                    return;
                }
            case GameState.MATCHMAKING:
                {
                    ChangeScene(GameState.MATCHMAKING.ToString());

                    levelManager.EnterKeyRoomControl();

                    uiManager.LoadMatchmaking(); // menus handled by UI and LevelManagers button controls

                    return;
                }
            case GameState.GAMESETUP:
                {
                    ChangeScene(GameState.GAMESETUP.ToString());

                    //if (!levelManager.SetupController.Started) levelManager.SetupController.View.RPC("StartSetup", Photon.Pun.RpcTarget.All);

                    //levelManager.UpdateSetupHUD();

                    uiManager.LoadGameSetup(); // menus handled by UI and LevelManagers button controls

                    return;
                }
            case GameState.GAMEPLAY:
                {
                    ChangeScene(GameState.GAMEPLAY.ToString());

                    uiManager.LoadGameplay();

                    if (finishchange)
                    {
                        /*//start world
                        if (levelManager.ActiveGameWorld.ReadyToStart)
                        {
                            levelManager.ActiveGameWorld.StartWorld(levelManager.SetupController);

                        }

                        //go to next turn
                        if (levelManager.ActiveGameWorld.CurrentPlayersTurn == false)
                        {
                            levelManager.StartNextTurn();
                        }*/
                    }
                    if (!finishchange) finishchange = true;

                    return;
                }
            case GameState.GAMEEND:
                {
                    uiManager.LoadGameEnd();

                    return;
                }
            case GameState.SAVE:
                {
                    uiManager.LoadSaveMenu();

                    return;
                }
            case GameState.LOAD:
                {
                    uiManager.LoadLoadMenu();

                    return;
                }
            case GameState.PAUSE:
                {
                    uiManager.LoadPauseScreen();

                    return;
                }
            case GameState.OPTIONS:
                {
                    uiManager.LoadOptions();

                    return;
                }
            case GameState.CREDITS:
                {
                    uiManager.LoadCredits();

                    return;
                }
        }
    }

    private void ControlPauseGame()
    {
        if (gameState == GameState.MAINMENU) return;
        if (gameState == GameState.MATCHMAKING) return;
        if (gameState == GameState.GAMESETUP) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameState == GameState.PAUSE ||
                gameState == GameState.OPTIONS ||
                gameState == GameState.SAVE)
                levelManager.ChangeGameStateToUnpause();
            else
                levelManager.ChangeGameStateToPause();
        }
    }

    private void ChangeScene(string sceneName, LoadSceneMode sceneMode = LoadSceneMode.Single)
    {
        if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName(sceneName))
        {
            SceneManager.LoadScene(sceneName, sceneMode);
        }
    }

    public void StopTime()
    {
        if (Time.timeScale == 1) { Time.timeScale = 0; }
    }

    public void StartTime()
    {
        if (Time.timeScale == 0) { Time.timeScale = 1; }
    }

    public void ChangeState(GameState targetState)
    {
        gameState = targetState;
    }

    public void Save(int path)
    {
        if (path < 0 || path > datapathRoute.Length) { Debug.LogError($"ERR: tried saving to a datapath that is\noutside the range of registered paths"); return; }

        FileStream file;

        if (File.Exists(Application.persistentDataPath + datapathRoute[path] + datapathExt) == false) // if file doesn't exist create one
        {
            file = File.Create(Application.persistentDataPath + datapathRoute[path] + datapathExt);
        }
        else { file = File.Open(Application.persistentDataPath + datapathRoute[path] + datapathExt, FileMode.Open); }

        BinaryFormatter bf = new BinaryFormatter();
        SaveInfo savedInfo = new SaveInfo();

        if (path == 0) // save options
        {
            savedInfo.savedBrightness = levelManager.GetBrightnessSliderValue();
        }
        else
        { 
            savedInfo.scene = SceneManager.GetActiveScene().buildIndex;
            savedInfo.activeScreen = levelManager.activeScreen;
            savedInfo.gameState = gameState;
        }

        bf.Serialize(file, savedInfo);
        file.Close();

        levelManager.enableSaveLoadText(GameState.SAVE); // method only used here, Gamestate id is for conveniences
    }

    public void Load(int path)
    {
        if (path < 0 || path > datapathRoute.Length) { Debug.LogError($"ERR: tried loading from a datapath that is\noutside the range of registered paths"); return; }
        if (File.Exists(Application.persistentDataPath + datapathRoute[path] + datapathExt) == false) { Debug.LogError($"ERR: tried loading...\ndatapath does not exist"); return; }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + datapathExt, FileMode.Open);

        SaveInfo loadedInfo = (SaveInfo)bf.Deserialize(file);
        file.Close();

        if (path == 0) // load options
        {
            levelManager.LoadBrightnessValue(loadedInfo.savedBrightness);
        }
        else
        {
            SceneManager.LoadScene(loadedInfo.scene);
            levelManager.activeScreen = loadedInfo.activeScreen;
            gameState = loadedInfo.gameState;
        }

        levelManager.enableSaveLoadText(GameState.LOAD); 
    }

    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    public bool CheckRoute(int route)
    {
        return File.Exists(Application.persistentDataPath + datapathRoute[route] + datapathExt);
    }
}

[Serializable]
class SaveInfo
{
    public GameState gameState;
    public int activeScreen;
    public int scene;
    public int health;
    public float savedBrightness;
    public float savedVolume;
}

#region depricated code

/*private void Controls() // Global Controls
{
    // quick save/load
    if (Input.GetKeyDown(KeyCode.S))
    {
        Save(1);
    }
    else if (Input.GetKeyDown(KeyCode.L))
    {
        Load(1);
    }

    ControlPauseGame();
}*/
#endregion