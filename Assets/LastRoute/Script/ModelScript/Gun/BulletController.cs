using UnityEngine;

public class BulletController : MonoBehaviour
{
    public int damage = 10;       // 데미지
    public float lifeTime = 5f;   // 총알 생존 시간 (너무 멀리 날아가는 것 방지)

    public GameObject wallHitEffect;

    private Rigidbody rb;

    void Start()
    {
        // 일정 시간이 지나면 자동으로 총알 파괴
        Destroy(gameObject, lifeTime);

        rb = GetComponent<Rigidbody>();

        // 총알은 매우 빠르므로 물리 연산을 연속적 설정
        // (벽을 뚫고 지나가는 터널링 현상 방지)
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    // Collider의 "Is Trigger"가 체크되어 있어야 작동합니다.
    void OnTriggerEnter(Collider other)
    {
        // 1. 맞은 대상이 좀비인지 확인 (부모 객체인 ZombieAI 검색)
        ZombieAI zombie = other.GetComponentInParent<ZombieAI>();

        // 좀비를 맞췄다면?
        if (zombie != null)
        {
            zombie.TakeDamage(damage, transform.position);
            Destroy(gameObject); // 총알 삭제
            return; // 여기서 함수 종료 (벽 이펙트가 나오지 않게 함)
        }

        // 2. 좀비도 아니고, 플레이어도 아니고, 다른 총알도 아니라면? (즉, 벽이나 바닥)
        else if (!other.CompareTag("Player") && !other.CompareTag("Bullet"))
        {
            // 이펙트 프리팹이 연결되어 있다면 생성
            if (wallHitEffect != null)
            {
                // Instantiate(이펙트, 위치, 회전)
                // 위치: 총알이 닿은 현재 위치
                // 회전: 총알이 날아온 반대 방향(-transform.forward)을 보게 해서 파편이 튀는 느낌을 줌
                GameObject effect = Instantiate(wallHitEffect, transform.position, Quaternion.LookRotation(-transform.forward));

                // 생성된 이펙트는 2초 뒤에 삭제 (찌꺼기가 남지 않게)
                Destroy(effect, 2f);
            }

            // 총알 삭제
            Destroy(gameObject);
        }
    }
}