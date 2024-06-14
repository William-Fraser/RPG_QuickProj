using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using Photon.Chat.Demo;

// controls general elements non-specific to this program, i.e: top-menus(static), online setup and options

public enum Region
{
    CENTRAL_ASIA,
    AUSTRALIA,
    CANADA_EAST,
    EUROPE,
    SOUTHAMERICA,
    US_WEST
}

public class LevelManager : MonoBehaviour
{
    public PlayerController player;

    public Image rightEquipmentImage;
    public Text rightEquipmentDisplayName;
    public Image leftEquipmentImage;
    public Text leftEquipmentDisplayName;

    [HideInInspector]
    public int activeScreen;
    public Canvas creditCanvas;

    [Space(20)]
    [Tooltip("any button that calls load")]
    public Button[] allLoadButtons;

    [Header("Online")]
    public TMP_InputField hostInput;
    public TMP_InputField joinInput;

    [Header("Options")]
    [Tooltip("The Background Image in SystemContants")]
    public Image brightnessImage;
    public Slider brightnessSlider;

    //private
    private Color alphaValueColour;
    private bool fadeSave;
    private bool fadeLoad;
    private float textFadeWaitTime = 1.5f;
    [Space]
    [SerializeField] private Text saveText;
    [SerializeField] private Text loadText;
    [Space]
    [SerializeField] private GameObject prefab_InfoPopup; // used to create popUpInfo/could be a class of popup info instanced to an object instead
    [SerializeField] private GameObject prefab_SetupController;

    //online
    private bool readyToJoinOnline;
    private Dictionary<Region, string> regionDex;
    private Region currentRegion;
    private GameState pausedGamestate;

    [Header("Setup")]
    [SerializeField] private GameObject stateButtonGroup;

    [Header("GameWorld")]
    [SerializeField] private TextMeshProUGUI mapSize;
    [SerializeField] private TextMeshProUGUI playersNum;

    private bool tempHostbool;

    private PhotonView view;

    public Dictionary<Region, string> RegionDex { get { return regionDex; } }
    public Region CurrentRegion { get { return currentRegion; } }
    public PhotonView View { get { return view; } }

    private void Start()
    {
        foreach (Button button in allLoadButtons)
        {
            EnableButtonIfFileExists(button);
        }

        readyToJoinOnline = true;
        tempHostbool = true;

        currentRegion = Region.CANADA_EAST;
        CreateRegionDex();

        view = GetComponent<PhotonView>();

        PhotonNetwork.OfflineMode = true;
    }

    public void Update()
    {
        Debug.LogWarning($"networking client server: <{PhotonNetwork.NetworkingClient.Server}>, isready: <{PhotonNetwork.IsConnectedAndReady}>");

        if (fadeSave || fadeLoad)
        { FadeSaveLoadText(); }
    }

    #region Gameplay
    public void SetPlayerObject(PlayerController player)
    {
        player.transform.parent = transform;
        this.player = player;
    }
    public void SetObjectColor(GameObject gameObject, Color colour)
    {
        gameObject.GetComponent<MeshRenderer>().material.color = colour;
    }
    #endregion

    #region UI Input, Buttons

    //buttons
    public void ChangeGameStateToMainMenu()
    {
        GameManager.manager.ChangeState(GameState.MAINMENU);
        //if (activeGameWorld != null) Destroy(ActiveGameWorld.gameObject);
        //if (setupController!=null) Destroy(setupController.gameObject);
        tempHostbool = true;

        PhotonNetwork.Disconnect();

        PhotonNetwork.OfflineMode = true;
    }

    public void ChangeGameStateToConnectServer()
    {
        GameManager.manager.ChangeState(GameState.CONNECTSERVER);

        PhotonNetwork.OfflineMode = false;
    }

    public void ChangeGameStateToMatchmaking()
    {
        GameManager.manager.ChangeState(GameState.MATCHMAKING);
    }

    public void ChangeGameStateToGameSetup()
    {
        GameManager.manager.ChangeState(GameState.GAMESETUP);

        /*setupController = GameManager.manager.levelManager.InstantiateFromConnectionType(prefab_SetupController, Vector3.zero).GetComponent<GameSetupController>();

        setupController.transform.parent = this.transform;

        setupController.isHost = tempHostbool;*/
    }

    public void ChangeGameStateToGamePlay()
    {
        GameManager.manager.ChangeState(GameState.GAMEPLAY);
    }

    public void ChangeGameStateToDemo()
    {
        GameManager.manager.ChangeState(GameState.DEMO);
    }

    public void ChangeGameStateToGameEnd()
    {
        GameManager.manager.ChangeState(GameState.GAMEEND);
    }

    public void ChangeGameStateToSave()
    {
        GameManager.manager.ChangeState(GameState.SAVE);
    }

    public void ChangeGameStateToLoad()
    {
        GameManager.manager.ChangeState(GameState.LOAD);
    }

    public void ChangeGameStateToPause()
    {
        if (GameManager.manager.GameState == GameState.OPTIONS) { Save(0); } // save options

        if (GameManager.manager.GameState == GameState.PAUSE ||
            GameManager.manager.GameState == GameState.OPTIONS ||
            GameManager.manager.GameState == GameState.SAVE)
                pausedGamestate = GameManager.manager.GameState;

        GameManager.manager.ChangeState(GameState.PAUSE);
    }

    public void ChangeGameStateToUnpause()
    {
        if (GameManager.manager.GameState == GameState.OPTIONS) { Save(0); } // save options

        GameManager.manager.ChangeState(pausedGamestate);
    }

    public void ChangeGameStateToOptions()
    {
        GameManager.manager.ChangeState(GameState.OPTIONS);
    }

    public void ChangeGameStateToCredits()
    {
        GameManager.manager.ChangeState(GameState.CREDITS);
    }

    //gamemode buttons
    public void EndTurn()
    { 
        
    }
    #endregion

    #region UI Feedback
    public void EnableSaveLoadText(GameState saveOrLoad)
    {
        if (saveOrLoad == GameState.SAVE)
        {
            saveText.CrossFadeAlpha(1, .1f, true);
            StartCoroutine(WaitToFadeText(GameState.SAVE)); // crossfade alpha like in fadeSaveLoad
        }
        else if (saveOrLoad == GameState.LOAD)
        {
            loadText.CrossFadeAlpha(1, .1f, true);
            StartCoroutine(WaitToFadeText(GameState.LOAD));
        }
    }

    public void FadeSaveLoadText()
    {
        if (fadeSave)
        {
            saveText.CrossFadeAlpha(0, 3, true); fadeSave = false;
        }
        if (fadeLoad)
        {
            loadText.CrossFadeAlpha(0, 3, true); fadeLoad = false;
        }
        if (Time.time <= 1)
        {
            saveText.CrossFadeAlpha(0, .1f, true);
            loadText.CrossFadeAlpha(0, .1f, true);
        }
    }

    public void CreatePopUp(string message, Vector3 popUpPos, Color color)
    {
        Debug.Log($"Creating Popup: {message} at <{popUpPos}>, coloured {color}");
        InfoPopUp popUp;
        popUp = Instantiate(prefab_InfoPopup, new Vector3(popUpPos.x, popUpPos.y + 5, popUpPos.z), Quaternion.identity).GetComponent<InfoPopUp>();
        popUp.SetUp(message, color);
    }
    
    public void CreatePopUp(string message, Vector3 popUpPos, Color color, bool checkProximity, bool disableWhenClose, float proxDistance, Transform proxTarget, bool scrolling)
    {
        Debug.Log($"Creating Popup: {message} at <{popUpPos}>, coloured {color}");
        InfoPopUp popUp;
        popUp = Instantiate(prefab_InfoPopup, new Vector3(popUpPos.x, popUpPos.y + 5, popUpPos.z), Quaternion.identity).GetComponent<InfoPopUp>();
        popUp.SetUp(message, color, checkProximity, disableWhenClose, proxDistance, proxTarget, scrolling);
    }

    public void EnableButtonIfFileExists(Button button)
    {
        foreach (string letter in button.name.Split())
        {
            if (int.TryParse(letter, out int result))
            {
                if (GameManager.manager.CheckRoute(result))
                {
                    button.interactable = true;
                }
            }
        }
    }

    public void JumpCanvasAlphaTo(float value, Canvas inputCanvas)
    {
        Image[] canvasImages = inputCanvas.gameObject.GetComponentsInChildren<Image>();
        Text[] canvasText = inputCanvas.gameObject.GetComponentsInChildren<Text>();

        if (canvasImages.Length >= 1)
        {
            for (int i = 0; i < canvasImages.Length; i++)
            {
                Image image = inputCanvas.gameObject.GetComponentsInChildren<Image>()[i];
                alphaValueColour = new Color(image.color.r, image.color.g, image.color.b, value);
                inputCanvas.gameObject.GetComponentsInChildren<Image>()[i].color = alphaValueColour;
                Debug.LogWarning($"fading {image.gameObject.name} to {value}");
            }
        }

        if (canvasText.Length >= 1)
        {
            for (int i = 0; i < canvasText.Length; i++)
            {
                Text text = inputCanvas.gameObject.GetComponentsInChildren<Text>()[i];
                alphaValueColour = new Color(text.color.r, text.color.g, text.color.b, value);
                inputCanvas.gameObject.GetComponentsInChildren<Text>()[i].color = alphaValueColour;
                Debug.LogWarning($"fading {text.gameObject.name} to {value}");
            }
        }
    }
    #endregion

    #region Misc. Commands
    public void Save(int path)
    {
        GameManager.manager.Save(path);
    }

    public void Load(int path)
    {
        GameManager.manager.Load(path);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ScrollCredits()
    { 
        //if (GameManager.manager.gameState)
    }
    #endregion

    #region Online methods
    public void CreateRegionDex()
    {
        regionDex = new Dictionary<Region, string>();

        regionDex.Add(Region.CENTRAL_ASIA, "ASIA");
        regionDex.Add(Region.AUSTRALIA, "AU");
        regionDex.Add(Region.CANADA_EAST, "CAE");
        regionDex.Add(Region.EUROPE, "EU");
        regionDex.Add(Region.SOUTHAMERICA, "SA");
        regionDex.Add(Region.US_WEST, "USW");
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(hostInput.text);
        tempHostbool = true;
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
        tempHostbool = false;
    }

    public void EnterKeyRoomControl()
    {
        if (joinInput.text.Length > 0 && hostInput.text.Length > 0) return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (joinInput.text.Length > 0) JoinRoom();

            if (hostInput.text.Length > 0) CreateRoom();
        }
    }

    public GameObject InstantiateFromConnectionType(GameObject instancedObject, Vector3 position)
    {
        GameObject inst;

        if (PhotonNetwork.IsConnectedAndReady)
        {
            inst = PhotonNetwork.Instantiate(instancedObject.name, position, Quaternion.identity);
        }
        else //instatiate like normal
        {
            inst = Instantiate(instancedObject, position, Quaternion.identity);
        }

        return inst;
    }

    //game setup
    /*public void UpdateSetupHUD()
    {
        setupController.mapSizeText = mapSize;
        setupController.numOfPlayersText = playersNum;

        setupController.UpdateSetupHUD();
    }*/

    /*public void IncreaseMapSize() // change to difficulty
    {
        setupController.View.RPC("IncreaseMapSize", RpcTarget.All);
    }

    public void DecreaseMapSize()
    {
        setupController.View.RPC("DecreaseMapSize", RpcTarget.All);
    }

    public void AddUserPlayer()
    {
        setupController.View.RPC("AddUserPlayer", RpcTarget.All);
    }

    public void RemoveUserPlayer()
    {
        setupController.View.RPC("RemoveUserPlayer", RpcTarget.All);
    }*/

    public void SetupGameStateButtonsSetActive(bool setActive)
    {
        stateButtonGroup.SetActive(setActive);
    }

    /*public void ChangeGameStateToGameStart()
    {
        setupController.StartGame();
    }

    public void SetGameWorld(WorldController gameWorld)
    {
        activeGameWorld = gameWorld;
    }*/
    #endregion

    #region Options
    public float GetBrightnessSliderValue()
    {
        return brightnessSlider.value;
    }
    public void LoadBrightnessValue(float brightnessValue)
    {
        brightnessSlider.value = brightnessValue; // set slider to slider value in case it's not
        SetBrightness(brightnessValue);
    }
    public void SetBrightness(float brightnessValue)
    {
        // sets the alpha of an image to the value of a slider
        Color newAlpha = brightnessImage.color;
        newAlpha.a = brightnessValue;
        brightnessImage.color = newAlpha;
    }
    #endregion

    //IEnum
    IEnumerator WaitToFadeText(GameState fade)
    {
        yield return new WaitForSeconds(textFadeWaitTime);
        if (fade == GameState.SAVE)
            fadeSave = true;
        else if (fade == GameState.LOAD)
            fadeLoad = true;
    }
}

