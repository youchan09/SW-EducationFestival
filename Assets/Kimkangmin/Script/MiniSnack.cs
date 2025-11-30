using UnityEngine;
using System.Collections.Generic;

public class MiniSnack : MonoBehaviour
{
    public Transform player;
    public float speed = 4f;
    public float damage = 10f;

    // 🐍 추가
    public float separationDistance = 1.2f;
    public float separationForce = 3f;

    // ✨ 쿨타임 변수
    public float attackCooldown = 3f; 
    private float lastAttackTime; 

    void Update() 
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return;

        // 1️⃣ 기본 플레이어 추적 방향 계산
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        // 2️⃣ 근처의 다른 MiniSnake들과 거리 유지
        Vector3 separation = Vector3.zero;
        MiniSnack[] allSnakes = FindObjectsOfType<MiniSnack>();

        foreach (MiniSnack other in allSnakes) 
        {
            if (other == this) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < separationDistance) 
            {
                Vector3 away = (transform.position - other.transform.position).normalized;
                separation += away * (separationDistance - dist);
            }
        }

        // 3️⃣ 방향 결합 (플레이어 추적 + 밀어내기)
        Vector3 finalDir = (dirToPlayer + separation * separationForce).normalized;

        // 4️⃣ 실제 이동
        transform.position += finalDir * speed * Time.deltaTime;
        transform.up = finalDir; 
    }

    private void OnTriggerStay(Collider other) 
    {
        if (!other.CompareTag("Player")) return;

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                Debug.Log($"⚔️ MiniSnack이 {damage} 데미지 입힘!");
                playerHealth.TakeDamage(damage);
            }
            else
            {
                Debug.LogError("❌ Player에 Health 스크립트 없음!");
            }

            lastAttackTime = Time.time;
        }
    }
}