using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class ZombieAI : MonoBehaviour
{
    [Header("상태 설정")]
    public float maxHealth = 60f;
    public float detectRange = 20f;
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackRate = 1.5f;

    [Header("이펙트")]
    public GameObject bloodEffectPrefab;
    public Transform hitPoint;

    private float currentHealth;
    private Transform playerTarget;
    private NavMeshAgent navAgent;
    private Animator animator;
    private float nextAttackTime = 0f;
    private bool isDead = false;

    // 거리 계산을 정확하게 하기 위한 플레이어 콜라이더
    private Collider playerCollider;

    // 래그돌 관련
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private CapsuleCollider mainCollider;

    // 자신이 생성된 스포너를 기억하는 변수
    private ZombieSpawner mySpawner;

    void Start()
    {
        currentHealth = maxHealth;
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        mainCollider = GetComponent<CapsuleCollider>();

        // 1. 태그로 플레이어 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        // 2. 만약 태그로 못 찾았으면, 씬 전체에서 VehicleStatus를 가진 녀석을 직접 찾음 (보험용)
        if (playerObj == null)
        {
            VehicleStatus vs = FindFirstObjectByType<VehicleStatus>();
            if (vs != null) playerObj = vs.gameObject;
        }

        if (playerObj != null)
        {
            playerTarget = playerObj.transform;

            // 자식 오브젝트까지 뒤져서 콜라이더를 찾아냄 (보닛 앞 공격 판정용)
            playerCollider = playerObj.GetComponentInChildren<Collider>();
        }

        InitRagdoll();
    }

    public void SetupSpawner(ZombieSpawner spawner)
    {
        mySpawner = spawner;
    }

    void Update()
    {
        if (isDead || playerTarget == null) return;

        // --- 거리 계산 로직 (표면 기준) ---
        float currentDistance;

        if (playerCollider != null)
        {
            // 플레이어 콜라이더 표면 중 가장 가까운 점 찾기
            Vector3 closestPoint = playerCollider.ClosestPoint(transform.position);
            currentDistance = Vector3.Distance(transform.position, closestPoint);
        }
        else
        {
            // 콜라이더 없으면 중심점 기준
            currentDistance = Vector3.Distance(transform.position, playerTarget.position);
        }

        // --- 상태 결정 ---
        if (currentDistance > detectRange)
        {
            navAgent.isStopped = true;
            animator.SetBool("isChasing", false);
            animator.SetBool("isAttacking", false);
        }
        else if (currentDistance <= attackRange)
        {
            navAgent.isStopped = true;
            animator.SetBool("isChasing", false);
            animator.SetBool("isAttacking", true);
            RotateTowardsPlayer();

            if (Time.time >= nextAttackTime)
            {
                AttackPlayer();
                nextAttackTime = Time.time + attackRate;
            }
        }
        else
        {
            navAgent.isStopped = false;
            navAgent.SetDestination(playerTarget.position);
            animator.SetBool("isChasing", true);
            animator.SetBool("isAttacking", false);
        }
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    // [핵심] 자신, 부모, 자식, 루트까지 싹 다 뒤져서 스크립트 찾는 함수
    VehicleStatus FindVehicleStatusRecursively(GameObject targetObj)
    {
        if (targetObj == null) return null;

        // 1. 그 오브젝트 자체에 있는지 확인
        VehicleStatus status = targetObj.GetComponent<VehicleStatus>();

        // 2. 자식들 중에 있는지 확인
        if (status == null) status = targetObj.GetComponentInChildren<VehicleStatus>();

        // 3. 부모들 중에 있는지 확인
        if (status == null) status = targetObj.GetComponentInParent<VehicleStatus>();

        // 4. (최후의 수단) 최상위 루트로 올라가서 거기서부터 다시 자식들을 싹 다 뒤짐
        // (스크립트가 형제 오브젝트나 엉뚱한 곳에 숨어있을 때 찾아냄)
        if (status == null && targetObj.transform.root != null)
        {
            status = targetObj.transform.root.GetComponentInChildren<VehicleStatus>();
        }

        return status;
    }

    void AttackPlayer()
    {
        if (playerTarget == null) return;

        // 위에서 만든 강력한 탐색 함수 사용
        VehicleStatus carStatus = FindVehicleStatusRecursively(playerTarget.gameObject);

        if (carStatus != null)
        {
            carStatus.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(float amount, Vector3 hitPos)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (bloodEffectPrefab != null)
        {
            Instantiate(bloodEffectPrefab, hitPos, Quaternion.LookRotation(transform.position - playerTarget.position));
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player") || collision.transform.root.CompareTag("Player"))
        {
            float impactSpeed = collision.relativeVelocity.magnitude;

            if (impactSpeed > 5f)
            {
                // 충돌한 부위(범퍼, 바퀴 등)를 기준으로 스크립트를 강력하게 탐색
                VehicleStatus carStatus = FindVehicleStatusRecursively(collision.gameObject);

                if (carStatus != null)
                {
                    carStatus.TakeDamage(5f);
                }

                Die(true, collision.relativeVelocity);
            }
        }
    }

    public void Die(bool isRoadKill = false, Vector3 impactForce = default)
    {
        if (isDead) return;
        isDead = true;

        if (GameManager.instance != null) GameManager.instance.AddScore(100);

        if (mySpawner != null)
        {
            mySpawner.OnZombieDied();
        }

        if (navAgent != null) navAgent.enabled = false;
        if (animator != null) animator.enabled = false;

        EnableRagdoll();

        if (mainCollider != null) mainCollider.enabled = false;

        foreach (var rb in ragdollRigidbodies)
        {
            rb.mass = 0.2f;
            rb.linearDamping = 0.2f;
            rb.angularDamping = 0.5f;
        }

        if (isRoadKill)
        {
            if (ragdollRigidbodies.Length > 0)
                ragdollRigidbodies[0].AddForce(impactForce * 1.5f, ForceMode.Impulse);
        }
        else
        {
            if (ragdollRigidbodies.Length > 0)
                ragdollRigidbodies[0].AddForce(-transform.forward * 5f, ForceMode.Impulse);
        }

        Destroy(gameObject, 10f);
    }

    void InitRagdoll()
    {
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        foreach (var rb in ragdollRigidbodies)
        {
            if (rb.gameObject != gameObject) rb.isKinematic = true;
        }
        foreach (var col in ragdollColliders)
        {
            if (col.gameObject != gameObject) col.enabled = false;
        }
    }

    void EnableRagdoll()
    {
        foreach (var rb in ragdollRigidbodies)
        {
            rb.isKinematic = false;
        }
        foreach (var col in ragdollColliders)
        {
            if (col.gameObject != gameObject) col.enabled = true;
        }
    }
}