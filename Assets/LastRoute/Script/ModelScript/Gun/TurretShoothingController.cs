using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TurretShootingController : MonoBehaviour
{
    [Header("프리팹 할당")]
    public GameObject bulletPrefab;
    public GameObject muzzleFlashPrefab;
    public GameObject bulletCasePrefab;

    [Header("발사 위치 설정")]
    public Transform firePoint;           // 실제 총알이 생성되는 위치 (충돌 방지 위해 약간 앞)
    public Transform muzzleFlashPoint;    // 화염 이펙트가 나올 위치 (총구 끝)
    public Transform shellEjectionPoint;  // 탄피 배출구 위치

    [Header("성능 설정")]
    public float bulletSpeed = 100f;
    public float fireRate = 0.1f;
    public float shellEjectionForce = 5f;

    [Header("사운드 설정")]
    public AudioClip fireSound;
    [Range(0f, 1f)] public float volume = 0.5f;

    [Header("조작감 설정")]
    public float rotationSpeed = 5f;

    [Header("각도 제한 설정")]
    [Range(-90f, 0f)] public float minPitch = -20f;
    [Range(0f, 90f)] public float maxPitch = 60f;

    [Header("충돌 감지 설정")]
    public LayerMask targetLayer;

    private float nextFireTime = 0f;
    private Camera mainCamera;
    private AudioSource audioSource;
    public VehicleStatus vehicleStatus; // 차량 상태 스크립트 연결

    void Start()
    {
        mainCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();
        vehicleStatus = GetComponentInParent<VehicleStatus>();
    }

    void Update()
    {
        RotateTurret();

        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void RotateTurret()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, 1000f, targetLayer))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(1000f);
        }

        Vector3 direction = targetPoint - transform.position;

        if (direction != Vector3.zero)
        {
            Vector3 localDirection = transform.parent.InverseTransformDirection(direction);
            Quaternion targetLocalRotation = Quaternion.LookRotation(localDirection);
            Vector3 euler = targetLocalRotation.eulerAngles;

            euler.x = NormalizeAngle(euler.x);
            euler.y = NormalizeAngle(euler.y);

            euler.x = Mathf.Clamp(euler.x, minPitch, maxPitch);
            euler.z = 0f;

            Quaternion clampedLocalRotation = Quaternion.Euler(euler);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, clampedLocalRotation, Time.deltaTime * rotationSpeed);
        }
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f) return angle - 360f;
        return angle;
    }

    void Shoot()
    {
        if (vehicleStatus != null)
        {
            if (vehicleStatus.TryConsumeAmmo() == false)
            {
                // 탄약 없으면 발사 안 함
                return;
            }
        }
        // 1. 소리 재생
        PlayFireSound();

        // 2. 이펙트 생성 (수정된 부분)
        // 만약 muzzleFlashPoint를 따로 넣지 않았다면, 그냥 firePoint에서 나오게 예외 처리
        Transform flashPos = (muzzleFlashPoint != null) ? muzzleFlashPoint : firePoint;

        if (muzzleFlashPrefab != null && flashPos != null)
        {
            // 부모를 flashPos로 설정하여 총이 움직여도 화염이 따라다니게 함
            Instantiate(muzzleFlashPrefab, flashPos.position, flashPos.rotation, flashPos);
        }

        // 3. 총알 발사 (FirePoint 사용)
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            bullet.transform.Rotate(90f, 0f, 0f);

            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null) bulletRb.linearVelocity = firePoint.forward * bulletSpeed;
        }

        // 4. 탄피 배출
        EjectShell();
    }

    void PlayFireSound()
    {
        if (fireSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(fireSound, volume);
        }
    }

    void EjectShell()
    {
        if (bulletCasePrefab == null || shellEjectionPoint == null) return;
        GameObject shell = Instantiate(bulletCasePrefab, shellEjectionPoint.position, shellEjectionPoint.rotation);
        Rigidbody shellRb = shell.GetComponent<Rigidbody>();
        if (shellRb != null)
        {
            Vector3 forceDirection = shellEjectionPoint.right + shellEjectionPoint.up + (shellEjectionPoint.forward * 0.5f);
            shellRb.AddForce(forceDirection.normalized * shellEjectionForce, ForceMode.Impulse);
            float randomTorque = Random.Range(-300f, 300f);
            shellRb.AddTorque(new Vector3(randomTorque, randomTorque, randomTorque));
        }
        Destroy(shell, 5f);
    }
}