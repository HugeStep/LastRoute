using System.Collections;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject zombiePrefab;   // 생성할 좀비 프리팹
    public Transform[] spawnPoints;   // 좀비가 나올 위치들 (여러 개 가능)

    [Header("옵션")]
    public int maxZombies = 5;        // 유지할 좀비 마리 수
    public float respawnDelay = 3f;   // 좀비가 죽고 나서 다시 생길 때까지 시간
    public float spawnRadius = 2f;    // 스폰 지점에서 약간 흩어져서 나오게 (겹침 방지)

    void Start()
    {
        // 게임 시작 시 maxZombies 만큼 생성
        for (int i = 0; i < maxZombies; i++)
        {
            SpawnZombie();
        }
    }

    // 좀비 생성 함수
    void SpawnZombie()
    {
        if (spawnPoints.Length == 0) return;

        // 1. 랜덤한 스폰 포인트 선택
        int index = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[index];

        // 2. 위치를 살짝 랜덤하게 섞음 (모두가 한 점에 겹치지 않게)
        Vector3 randomPos = Random.insideUnitSphere * spawnRadius;
        randomPos.y = 0; // 높이는 바닥에 고정
        Vector3 finalPos = spawnPoint.position + randomPos;

        // 3. 좀비 생성
        GameObject newZombie = Instantiate(zombiePrefab, finalPos, spawnPoint.rotation);

        // 4. 좀비에게 "너의 담당 스포너는 나야"라고 알려줌
        ZombieAI zombieScript = newZombie.GetComponent<ZombieAI>();
        if (zombieScript != null)
        {
            zombieScript.SetupSpawner(this);
        }
    }

    // 좀비가 죽었을 때 호출되는 함수 (ZombieAI가 호출함)
    public void OnZombieDied()
    {
        StartCoroutine(RespawnCoroutine());
    }

    // 딜레이 후 리스폰
    IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnZombie();
    }
}