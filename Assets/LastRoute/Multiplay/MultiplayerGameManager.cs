using UnityEngine;
using Photon.Pun;

public class MultiplayerGameManager : MonoBehaviourPunCallbacks
{
    public GameObject player2CamPrefab; // 플레이어 2를 위한 별도 카메라 (필요하다면)

    void Start()
    {
        // 방장(Player 1)인 경우에만 트럭을 생성
        if (PhotonNetwork.IsMasterClient)
        {
            // "ElectricTruck"은 Resources 폴더에 있는 프리팹 이름이어야 함
            PhotonNetwork.Instantiate("Electric Truck - Ready To Drive Variant", new Vector3(0, 0, 0), Quaternion.identity);
        }
    }
}