using UnityEngine;
using TMPro;

public class VehicleStatus : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI fuelText;
    public TextMeshProUGUI ammoText;

    [Header("설정 & 연결")]

    public MonoBehaviour carController; // 사망시 차량 작동 불가능하게 위해서

    public GameObject explosionEffect; // 폭발 이펙트

    [Header("1. 체력 설정")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("2. 연료 설정")]
    public float maxFuel = 55f;
    public float currentFuel;
    public float driveDuration = 120f;
    private float fuelConsumptionRate;

    [Header("3. 탄약 설정")]
    public int maxAmmo = 40;
    public int currentAmmo;

    private Rigidbody rb;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        currentFuel = maxFuel;
        currentAmmo = maxAmmo;
        fuelConsumptionRate = maxFuel / driveDuration;

        UpdateUI();
    }

    void Update()
    {
        if (isDead) return;

        HandleFuel();
        UpdateUI();
    }

    void UpdateUI()
    {
        if (hpText != null) hpText.text = $"HP : {currentHealth:F0}";
        if (fuelText != null) fuelText.text = $"{currentFuel:F1}%";
        if (ammoText != null) ammoText.text = $"Ammo : {currentAmmo}";
    }

    void HandleFuel()
    {
        if (rb.linearVelocity.magnitude > 0.1f && currentFuel > 0)
        {
            currentFuel -= fuelConsumptionRate * Time.deltaTime;
            if (currentFuel <= 0) {
                currentFuel = 0;
                Die();
            }
        }
 
    }

    public void AddFuel(float amount)
    {
        currentFuel += amount;
        if (currentFuel > maxFuel) currentFuel = maxFuel;
    }

    public bool TryConsumeAmmo()
    {
        if (currentAmmo > 0)
        {
            currentAmmo--;
            return true;
        }
        return false;
    }

    public void AddAmmo(int amount)
    {
        currentAmmo += amount;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        if (!collision.gameObject.CompareTag("Enemy") &&
            !collision.gameObject.CompareTag("Bullet") &&
            !collision.gameObject.CompareTag("Road"))
        {
            float impactSpeed = collision.relativeVelocity.magnitude;

            if (impactSpeed >= 150f) TakeDamage(40);
            else if (impactSpeed >= 100f) TakeDamage(25);
            else if (impactSpeed >= 70f) TakeDamage(20);
            else if (impactSpeed >= 40f) TakeDamage(5);
            else if (impactSpeed >= 20f) TakeDamage(2);
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        UpdateUI();

        // 1. 연결된 차량 컨트롤러 끄기
        if (carController != null)
        {
            carController.enabled = false;
        }

        // 2. 포탑 회전 스크립트 끄기
        var turret = GetComponentInChildren<TurretShootingController>();
        if (turret != null) turret.enabled = false;

        // 3. 폭발 이펙트
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, transform.rotation);
        }

        Debug.Log("차량 폭발! 3초 뒤 게임 오버...");
        Invoke("ShowGameOverScreen", 3f);
    }

    void ShowGameOverScreen()
    {
        Debug.Log("GAME OVER SCREEN");
        if (GameManager.instance != null) GameManager.instance.OnGameOver();
    }
}