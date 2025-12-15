using UnityEngine;

public class AmmoItem : MonoBehaviour
{
    [Header("설정")]
    public int ammoAmount = 60; // 획득할 탄약 수
    public float rotateSpeed = 50f; // 아이템이 빙글빙글 돌 속도
    public AudioClip pickupSound; // 아이템 획득시 소리

    // 아이템이 제자리에서 빙글빙글 돌게 하는 효과
    void Update()
    {
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
    }

    // 차량과 닿았을 때 실행
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("부딪힘");
        // 부딪힌 게 플레이어(차량)인지 확인
        if (other.CompareTag("Player"))
        {
            // 차량의 상태 관리 스크립트(VehicleStatus)를 가져옴
            // (혹시 콜라이더가 자식 오브젝트에 있을 수 있으니 부모까지 찾음)
            VehicleStatus status = other.GetComponent<VehicleStatus>();
            if (status == null) status = other.GetComponentInParent<VehicleStatus>();

            // 3. 스크립트가 있다면 탄약 추가하고 아이템 삭제
            if (status != null)
            {
                status.AddAmmo(ammoAmount); // 60발 추가

                // 획득 효과음
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

                Destroy(gameObject); // 아이템 삭제
            }
        }
    }
}