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

        // 맞은 부위의 부모, 부모의 부모까지 뒤져서 좀비 스크립트를 찾음
        ZombieAI zombie = other.GetComponentInParent<ZombieAI>();

        // 좀비 스크립트를 찾았다면?
        if (zombie != null)
        {
            zombie.TakeDamage(damage, transform.position);
            Destroy(gameObject); // 총알 삭제
        }

        // (참고) 만약 좀비가 아니라 벽에 맞았을 때 삭제하는 로직은 아래에 유지
        else if (!other.CompareTag("Player") && !other.CompareTag("Bullet"))
        {
            Destroy(gameObject);
        }
    }
}
