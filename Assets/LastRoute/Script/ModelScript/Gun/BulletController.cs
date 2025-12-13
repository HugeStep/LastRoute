using UnityEngine;

public class BulletController : MonoBehaviour
{
    public int damage = 10;      // 데미지
    public float lifeTime = 10f;  // 총알 생존 시간 (너무 멀리 날아가는 것 방지)

    void Start()
    {
        // 일정 시간이 지나면 자동으로 총알 파괴
        Destroy(gameObject, lifeTime);
    }

    // Collider의 "Is Trigger"가 체크되어 있어야 작동합니다.
    void OnTriggerEnter(Collider other)
    {
        // 충돌한 물체의 태그가 "Enemy"(좀비)인지 확인
        if (other.CompareTag("Enemy"))
        {
            // TODO: 나중에 여기에 좀비 체력을 깎는 코드를 추가해야 합니다.
            // 예: other.GetComponent<ZombieHealth>().TakeDamage(damage);
            Debug.Log("좀비 명중! 데미지: " + damage);

            // 피 이펙트 생성 위치 (선택사항)
            // Instantiate(bloodEffectPrefab, transform.position, Quaternion.identity);

            Destroy(gameObject); // 총알 파괴
        }
        // 장애물이나 땅에 맞았을 때
        else if (other.CompareTag("Obstacle") || other.CompareTag("Ground"))
        {
            // 벽 맞은 이펙트 생성 위치 (선택사항)
            Destroy(gameObject); // 총알 파괴
        }
    }
}
