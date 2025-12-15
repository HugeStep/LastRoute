using UnityEngine;

public class FuelItem : MonoBehaviour
{
    [Header("설정")]
    public float fuelAmount = 10f;   // 획득할 연료 양
    public float rotateSpeed = 50f;  // 회전 속도

    // 효과음 변수 
    public AudioClip pickupSound;

    void Update()
    {
        // 아이템이 제자리에서 빙글빙글 돔
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. 플레이어인지 확인
        if (other.CompareTag("Player"))
        {
            // 2. 차량 상태 스크립트 가져오기 (부모 포함 검색)
            VehicleStatus status = other.GetComponent<VehicleStatus>();
            if (status == null) status = other.GetComponentInParent<VehicleStatus>();

            // 3. 스크립트가 있다면 연료 채우고 삭제
            if (status != null)
            {
                status.AddFuel(fuelAmount); // 연료 25 추가


                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                

                Destroy(gameObject); // 아이템 삭제
            }
        }
    }
}