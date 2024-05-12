using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.LogWarning("Starting Photon, Connecting to Photon Server");
    }

    public override void OnConnectedToMaster() // connecting to server
    {
        PhotonNetwork.JoinLobby();
        Debug.LogWarning("Photon Connected to Master Server");
    }

    public override void OnJoinedLobby() // connected to server
    {
        GameManager.manager.levelManager.ChangeGameStateToMatchmaking();
        Debug.LogWarning($"Photon Joined Server Lobby, Region: <{PhotonNetwork.CloudRegion}>");
    }
}
