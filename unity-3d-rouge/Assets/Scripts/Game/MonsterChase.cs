using UnityEngine;
using UnityEngine.AI;

public class MonsterChase : MonoBehaviour
{
    [Header("追击设置")]
    public Transform player;          // 玩家对象
    public float chaseDistance = 10f; // 开始追击的距离
    public float stopDistance = 2f;   // 停止追击的距离

    private NavMeshAgent agent;
    private Animator animator;

    void Start()
    {
        // 获取组件引用
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // 自动查找玩家（需给玩家设置"Player"标签）
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        agent.avoidancePriority = Random.Range(50, 90); // 随机优先级
        //agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 距离检测逻辑
        if (distanceToPlayer <= chaseDistance && distanceToPlayer > stopDistance)
        {
            ChasePlayer();
        }
        else
        {
            StopChase();
        }

        // 更新动画状态（可选）
        UpdateAnimation();
    }

    void ChasePlayer()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    void StopChase()
    {
        agent.isStopped = true;
    }

    void UpdateAnimation()
    {
        // 根据速度切换行走/站立动画
        if (animator != null)
        {
            bool isMoving = agent.velocity.magnitude > 0.1f;
            animator.SetBool("IsMoving", isMoving);
        }
    }

    // 可视化检测范围（场景视图可见）
    //void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, chaseDistance);

    //    Gizmos.color = Color.green;
    //    Gizmos.DrawWireSphere(transform.position, stopDistance);
    //}
}