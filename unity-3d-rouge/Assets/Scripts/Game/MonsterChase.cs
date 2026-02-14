using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MonsterChase : MonoBehaviour
{
    [Header("追击设置")]
    public Transform player;          // 玩家对象
    public float chaseDistance = 10f; // 开始追击的距离
    public float stopDistance = 2f;   // 停止追击的距离
    private MonsterInfoCfg monsterInfoCfg;
    private CharacterStats monsterStats = new CharacterStats();

    private NavMeshAgent agent;
    protected Animator animator;
    //受击特效用
    private float flashDuration = 0.1f; // 颜色闪白时长
    public Color flashColor = Color.red; // 闪白颜色
    public float shakeIntensity = 0.5f; // 屏幕震动强度
    public float shakeDuration = 0.2f; // 屏幕震动时长
    private Material[] originalMaterials;
    private Color[] originalColors;
    private Renderer[] renderers;
    private bool isFlashing = false;
    private bool isHitStunned = false;
    private float currentSpeed; // 存储原始速度
    private float currentAngularSpeed; // 存储原始角速度

    private Collider enemyCollider;

    [Header("死亡动画设置")]

    private float shrinkDuration = 0.25f;    // 缩放时间
    public float rotationSpeed = 720f;       // 旋转速度（度/秒）
    public Ease movementEase = Ease.OutQuad; // 运动缓动类型
    public Ease fadeEase = Ease.OutCubic;    // 淡出缓动类型
    public ParticleSystem deathEffect;       // 死亡特效

    private GameObject hpUiObj = null;

    void Start()
    {
        // 获取组件引用
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider>();
        // 自动查找玩家（需给玩家设置"Player"标签）
        if (player == null)
        {
            player = BattleManager.GetInstance().GetHeroObj().transform;
        }
        agent.avoidancePriority = Random.Range(50, 90); // 随机优先级
        agent.speed = monsterInfoCfg.moveSpeed;
        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;
            if (originalMaterials[i].HasProperty("_BaseColor"))
            {
                originalColors[i] = originalMaterials[i].GetColor("_BaseColor");
            }
        }
    }

    protected virtual void Update()
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

    protected void ChasePlayer()
    {
        if (agent != null && agent.isActiveAndEnabled && player != null && isHitStunned==false)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
    }

    protected void StopChase()
    {
        if (agent != null)
        {
            agent.isStopped = true;
        }
    }

    protected void UpdateAnimation()
    {
        // 根据速度切换行走/站立动画
        if (animator != null)
        {
            //Debug.Log(agent.velocity.magnitude);
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    public void InitMonsterStats(MonsterInfoCfg _monsterInfoCfg)
    {
        monsterInfoCfg = _monsterInfoCfg;
        int hpNumber = (int)((float)_monsterInfoCfg.hp * ((float)BattleManager.GetInstance().levelManagement.CurrentGkLevel * 1f + 1));
        monsterStats.maxHealth = hpNumber;
        monsterStats.health = hpNumber;
        monsterStats.baseDamage = _monsterInfoCfg.attack;
        isHitStunned = false;
        transform.localScale = Vector3.one;
        DisableComponents(true);
        //if (_monsterInfoCfg.type == 2)
        //{
        //    BattleManager.GetInstance().AddHpUi(this.gameObject);
        //}
        BattleManager.GetInstance().AddHpUi(this.gameObject);
    }
    public void TakeDamage(HeroDamage _heroDamage)
    {
        //受击效果
        ApplyHitEffects(_heroDamage);
        if (_heroDamage.type == 1)
        {
            monsterStats.health -= _heroDamage.finalDamage;
        }
        else
        {
            monsterStats.health -= _heroDamage.criticalDamage;
        }
        //更新血条
        if (hpUiObj != null)
        {
            float number = (float)monsterStats.health / (float)monsterStats.maxHealth;
            hpUiObj.GetComponent<Hp>().UpdateHpUi(number);
        }
        //统计伤害
        BattleManager.GetInstance().DamageDone(_heroDamage);

        if (monsterStats.health <= 0)
        {
            //死亡
            Death(true);
        }
    }
    //怪物死亡
    private IEnumerator DeathRoutine(bool isJingYan)
    {
        // 销毁血条
        DesHpUiObj();
        // 步骤1: 禁用所有非必要组件
        DisableComponents(false);
        //在怪物数组里面移除
        BattleManager.GetInstance().RemoveMonsterList(gameObject);
        //播放死亡效果
        Sequence deathSequence = DOTween.Sequence();

        //// 3.1 向上跳跃效果
        //deathSequence.Append(transform.DOJump(
        //    transform.position + Vector3.up * 0.5f,
        //    jumpPower,
        //    1,
        //    deathDuration
        //).SetEase(movementEase));

        // 3.3 缩放效果
        deathSequence.Append(transform.DOScale(
            Vector3.zero,
            shrinkDuration
        ).SetEase(Ease.InBack));

        ////3.2 同时旋转
        //deathSequence.Join(transform.DORotate(
        //    new Vector3(0, rotationSpeed, 0),
        //    shrinkDuration,
        //    RotateMode.LocalAxisAdd
        //));

        yield return deathSequence.WaitForCompletion();
        //生成死亡特效
        var txObj = GameManager.instance.AddPrefab("Weapon/TongYong/DeathTx", BattleManager.GetInstance().TeXiaoObj.transform, transform.position, transform.rotation);
        //生成经验值
        if (isJingYan)
        {
            BattleManager.GetInstance().AddJingYan(transform.position, monsterInfoCfg.jbNumber);
            if (monsterInfoCfg.type == 2)
            {
                Vector3 vector3 = new Vector3(transform.position.x,  0.6f, transform.position.z);
                BattleManager.GetInstance().AddJingYingAward(vector3);
            }
        }
        yield return new WaitForSeconds(1f);
        //销毁死亡特效
        GameManager.instance.DestroyPrefab(txObj);
  
        GameManager.instance.DestroyPrefab(gameObject);
        yield return null;
    }
    public void Death(bool isJingYan)
    {
        StartCoroutine(DeathRoutine(isJingYan));
    }

    // 销毁血条
    public void DesHpUiObj()
    {
        if (hpUiObj != null)
        {
            hpUiObj.GetComponent<Hp>().HpDeath();
            hpUiObj.SetActive(false);
            //Destroy(hpUiObj);
            GameManager.instance.DestroyPrefab(hpUiObj);
            hpUiObj = null;
        }
        else
        {
            Debug.LogError("3333333");
        }
    }

    public void InitHpUiObj(GameObject _hpUiObj)
    {
        hpUiObj = _hpUiObj;
    }

    private void DisableComponents(bool _isBool)
    {
        // 禁用碰撞器和物理
        if (enemyCollider != null) enemyCollider.enabled = _isBool;
        // 禁用AI和移动脚本
        if (agent != null) agent.enabled = _isBool;
        //禁用动画
        if (animator != null)
        {
            animator.enabled = _isBool;
        }

        if (originalColors != null && originalColors != null)
        {
            //// 恢复原始颜色
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.SetColor("_BaseColor", originalColors[i]);
            }

            isFlashing = false;
        }
  

        // 禁用其他可能的组件
        //MonoBehaviour[] components = GetComponents<MonoBehaviour>();
        //foreach (var comp in components)
        //{
        //    if (comp != this && comp != animator)
        //    {
        //        comp.enabled = false;
        //    }
        //}
    }

    //受击效果  
    public void ApplyHitEffects(HeroDamage _heroDamage)
    {
        if (_heroDamage.stop > 0)
        {
            StartCoroutine(HitStunEffect(_heroDamage.stop));
        }
        StartCoroutine(FlashColor());
    }
    // 单个怪物的受击停顿效果
    private IEnumerator HitStunEffect(float _time)
    {
        if (isHitStunned) yield break;

        isHitStunned = true;
        bool wasStopped = agent.isStopped;
        // 保存当前移动状态
        if (agent != null && agent.isActiveAndEnabled)
        {
            // 完全停止怪物移动
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // 等待停顿时间
        yield return new WaitForSeconds(_time);

        // 恢复移动
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = wasStopped;
        }

        isHitStunned = false;
    }
    // 颜色闪白效果
    private IEnumerator FlashColor()
    {
        if (isFlashing) yield break;

        isFlashing = true;

        // 应用闪白材质
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.SetColor("_BaseColor", flashColor);
        }

        yield return new WaitForSeconds(flashDuration);

        // 恢复原始颜色
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.SetColor("_BaseColor", originalColors[i]);
        }

        isFlashing = false;
    }

    private float lastDamageTime = 0f;
    private float damageInterval = 1f; // 伤害间隔为1秒

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 检查是否达到了伤害间隔时间
            if (Time.time >= lastDamageTime + damageInterval)
            {
                other.GetComponent<PlayerContry>().PlayerApplyHitEffects(monsterStats.baseDamage);
                lastDamageTime = Time.time; // 更新最后一次伤害时间
            }
        }
    }

}