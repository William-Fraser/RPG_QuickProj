using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class JoinRooms : MonoBehaviourPunCallbacks
{
    public override void OnJoinedRoom()
    {
        Debug.LogWarning($"Joinned room {PhotonNetwork.CurrentRoom.Name}");
        PhotonNetwork.LoadLevel("GAMESETUP");
        GameManager.manager.levelManager.ChangeGameStateToGameSetup(); // 4 hour fix, just writing the line :') 
    }
}
