using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class ZombieAI : MonoBehaviour
{
    [Header("상태 설정")]
    public float maxHealth = 60f;
    public float detectRange = 20f;  // 플레이어 감지 범위
    public float attackRange = 2f;   // 공격 범위
    public float attackDamage = 10f; // 좀비 공격력
    public float attackRate = 1.5f;  // 공격 속도 (초)

    [Header("이펙트")]
    public GameObject bloodEffectPrefab; // 피 튀기는 이펙트
    public Transform hitPoint;           // 피가 튀길 기준 위치 (가슴 쪽 추천)

    private float currentHealth;
    private Transform playerTarget;
    private NavMeshAgent navAgent;
    private Animator animator;
    private float nextAttackTime = 0f;
    private bool isDead = false;

    // 래그돌 제어용 변수들
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private Collider mainCollider; // 좀비의 캡슐 콜라이더

    void Start()
    {
        currentHealth = maxHealth;
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        mainCollider = GetComponent<Collider>();

        // 플레이어(차량) 찾기 (Tag를 Vehicle로 설정했다고 가정)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }
        Debug.Log("좀비가 추적하는 대상 이름: " + playerObj.name);
        // 래그돌 초기화 (처음엔 꺼두기)
        InitRagdoll();
    }

    void Update()
    {
        if (isDead || playerTarget == null) return;

        // 플레이어와의 거리를 먼저 계산합니다.
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // 1. 플레이어가 감지 범위보다 멀리 있을 때 -> [IDLE 상태]
        if (distanceToPlayer > detectRange)
        {
            navAgent.isStopped = true; // 이동 멈춤

            // 애니메이션 모두 끄기 (IDLE로 돌아감)
            animator.SetBool("isChasing", false);
            animator.SetBool("isAttacking", false);
        }
        // 2. 공격 범위 안에 있을 때 -> [ATTACK 상태]
        else if (distanceToPlayer <= attackRange)
        {
            navAgent.isStopped = true; // 이동 멈춤 (공격해야 하니까)

            animator.SetBool("isChasing", false);
            animator.SetBool("isAttacking", true);

            RotateTowardsPlayer(); // 공격 중에는 플레이어를 바라봄

            // 공격 주기 체크
            if (Time.time >= nextAttackTime)
            {
                AttackPlayer();
                nextAttackTime = Time.time + attackRate;
            }
        }
        // 3. 감지 범위 안이고, 공격 범위보다는 멀 때 -> [CHASE 상태]
        else
        {
            navAgent.isStopped = false; // 이동 시작
            navAgent.SetDestination(playerTarget.position); // 목표 갱신

            animator.SetBool("isChasing", true);
            animator.SetBool("isAttacking", false);
        }
    }

    // 플레이어 바라보기
    void RotateTowardsPlayer()
    {
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    // 공격 실행
    void AttackPlayer()
    {
        // TODO: 여기에 차량 스크립트의 TakeDamage 함수를 호출해야 함
        // 예: playerTarget.GetComponent<CarHealth>().TakeDamage(attackDamage);
        Debug.Log("좀비가 차량을 공격함!");
    }

    // 데미지 입는 함수 (총알 스크립트에서 호출)
    public void TakeDamage(float amount, Vector3 hitPos)
    {
        if (isDead) return;

        currentHealth -= amount;

        // 피 이펙트 생성
        if (bloodEffectPrefab != null)
        {
            Instantiate(bloodEffectPrefab, hitPos, Quaternion.LookRotation(transform.position - playerTarget.position));
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 차량 충돌 처리
    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        // 차량(Vehicle)과 충돌했는가?
        if (collision.gameObject.CompareTag("Player"))
        {
            // 상대 속도 계산 (세게 부딪혔는지 확인)
            float impactSpeed = collision.relativeVelocity.magnitude;
            
            if (impactSpeed > 6f) // 시속 20 이상의 속도로 부딪히면
            {
                // TODO: 차량에 데미지 주기
                // collision.gameObject.GetComponent<CarHealth>().TakeDamage(10);
                Debug.Log("차량 로드킬! 좀비 사망.");

                Die(true, collision.relativeVelocity); // 충격량을 전달하며 사망
            }
        }
    }

    // 사망 처리
    void Die(bool isRoadKill = false, Vector3 impactForce = default)
    {
        isDead = true;

        // AI 및 애니메이션 끄기
        navAgent.enabled = false;
        animator.enabled = false;
        mainCollider.enabled = false; // 메인 캡슐 콜라이더 끄기 (래그돌과 겹치지 않게)

        // 래그돌 활성화 (물리 효과 켜기)
        EnableRagdoll();

        // 죽는 순간 좀비의 뼈대들을 아주 가볍게
        foreach (var rb in ragdollRigidbodies)
        {
            rb.mass = 0.2f; // 원래 5~10 정도인 무게를 0.5로 대폭 감소
            rb.linearDamping = 0.2f; // 공기 저항을 줘서 너무 멀리 안 날아가게 조절
            rb.angularDamping = 0.5f; // 회전 저항
        }

        // 차량에 치여 죽었다면 날아가게 힘을 줌
        if (isRoadKill)
        {
            // 래그돌의 골반(보통 첫번째 RB)에 힘을 가함
            ragdollRigidbodies[0].AddForce(impactForce * 1.5f, ForceMode.Impulse);
        }
        else
        {
            // 총 맞아 죽었으면 살짝 뒤로 밀림
            ragdollRigidbodies[0].AddForce(-transform.forward * 5f, ForceMode.Impulse);
        }

        // 5초 뒤 시체 삭제 (선택 사항)
        Destroy(gameObject, 10f);
    }

    // --- 래그돌 관련 헬퍼 함수 ---
    void InitRagdoll()
    {
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        foreach (var rb in ragdollRigidbodies)
        {
            if (rb.gameObject != gameObject) // 자기 자신의 리지드바디 제외
            {
                rb.isKinematic = true; // 평소엔 물리 끄기 (애니메이션이 제어)
            }
        }

        foreach (var col in ragdollColliders)
        {
            if (col.gameObject != gameObject)
            {
                col.enabled = false; // 평소엔 콜라이더 끄기
            }
        }
    }

    void EnableRagdoll()
    {
        foreach (var rb in ragdollRigidbodies)
        {
            rb.isKinematic = false; // 물리 켜기
        }
        foreach (var col in ragdollColliders)
        {
            col.enabled = true; // 콜라이더 켜기
        }
    }
}