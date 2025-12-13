using UnityEngine;

public class TurretShootingController : MonoBehaviour
{
    [Header("프리팹 할당")]
    public GameObject bulletPrefab;
    public GameObject muzzleFlashPrefab;
    public GameObject bulletCasePrefab;

    [Header("발사 위치 설정")]
    public Transform firePoint;
    public Transform shellEjectionPoint;

    [Header("성능 설정")]
    public float bulletSpeed = 100f;
    public float fireRate = 0.1f;
    public float shellEjectionForce = 5f;

    [Header("조작감 설정")]
    public float rotationSpeed = 5f;

    [Header("각도 제한 설정")]
    [Range(-90f, 0f)] public float minPitch = -20f;  // 위아래 제한 (아래)
    [Range(0f, 90f)] public float maxPitch = 60f;   // 위아래 제한 (위)
    // 좌우(Yaw) 제한 변수는 삭제했습니다 (360도 회전)

    [Header("충돌 감지 설정")]
    public LayerMask targetLayer; // 레이캐스트가 감지할 레이어 (땅, 적 등)

    private float nextFireTime = 0f;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
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

        // [핵심 수정] targetLayer에 포함된 물체만 인식합니다. (내 차는 무시됨)
        // 거리는 넉넉하게 1000f로 설정
        if (Physics.Raycast(ray, out hit, 1000f, targetLayer))
        {
            targetPoint = hit.point;
        }
        else
        {
            // 허공을 보면 그냥 레이 방향 멀리를 바라봄
            targetPoint = ray.GetPoint(1000f);
        }

        Vector3 direction = targetPoint - transform.position;

        if (direction != Vector3.zero)
        {
            // 1. 방향을 로컬 기준으로 변환 (트럭이 회전해도 정상 작동하도록)
            Vector3 localDirection = transform.parent.InverseTransformDirection(direction);
            Quaternion targetLocalRotation = Quaternion.LookRotation(localDirection);
            Vector3 euler = targetLocalRotation.eulerAngles;

            // 2. 각도 보정 (-180 ~ 180도)
            euler.x = NormalizeAngle(euler.x);
            euler.y = NormalizeAngle(euler.y);

            // 3. X축(위아래)만 제한 (차체를 뚫고 쏘지 않게)
            euler.x = Mathf.Clamp(euler.x, minPitch, maxPitch);

            // 4. Y축(좌우) 제한 삭제 -> 360도 회전 가능!

            // 5. Z축(기울기) 고정
            euler.z = 0f;

            // 6. 회전 적용
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
        if (muzzleFlashPrefab != null) Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation, firePoint);

        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            bullet.transform.Rotate(90f, 0f, 0f);

            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null) bulletRb.linearVelocity = firePoint.forward * bulletSpeed;
        }
        EjectShell();
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