using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class EnemyTracking : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player;

    public LayerMask isGround, isPlayer;

    public float health;

    //追蹤
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //攻擊
    public float timeToAttacks;
    bool Attacked;
    public GameObject projectile;

    //範圍
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        player = GameObject.Find("Pizza Boy").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        //偵測範圍與攻擊範圍
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, isPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, isPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patroling();       //如果玩家不在偵測範圍也不在攻擊範圍 → 啟動偵測系統
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();      //如果玩家在偵測範圍但不在攻擊範圍 → 追蹤玩家
        if (playerInAttackRange && playerInSightRange) AttackPlayer();      //如果玩家在偵測範圍且在攻擊範圍 → 攻擊玩家
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();       //設定並搜尋移動點

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }
    private void SearchWalkPoint()
    {
        //計算在範圍內的隨機點
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, isGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        //不要讓敵人移動
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!Attacked)
        {
            ///攻擊
            Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            rb.AddForce(transform.up * 8f, ForceMode.Impulse);
            ///攻擊

            Attacked = true;
            Invoke(nameof(ResetAttack), timeToAttacks);
        }
    }
    private void ResetAttack()
    {
        Attacked = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0) Invoke(nameof(DestroyEnemy), 0.5f);
    }
    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}



