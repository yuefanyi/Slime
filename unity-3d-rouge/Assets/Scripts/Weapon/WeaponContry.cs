using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponContry : MonoBehaviour
{
    [Header("攻击设置")]
    [Tooltip("基础攻击速度（每秒攻击次数）")]
    public float baseAttackSpeed = 1.0f; // 默认1秒攻击1次


    [Header("动画控制")]
    public Animator animator;
    [SerializeField] private string attackAnimationName = "Attack"; // 攻击动画名称

    private CharacterStats weaponStats;
    private CharacterStats heroStats;
    protected float nextAttackTime;
    private int PerformAttackType = 1;    //攻击触发方式 1定时 2定时协程
    public GameObject skillUiObj;

    //最终傷害
    protected HeroDamage finalState;

    // 当前实际攻速（考虑装备/技能加成）
    public float CurrentAttackSpeed { get; private set; }

    protected virtual void Start()
    {
        CurrentAttackSpeed = baseAttackSpeed;
        if (animator == null) animator = GetComponent<Animator>();
    }

    protected virtual void Update()
    {
        CheckWeaponAttack();
    }
    // 这个函数会在对象被 Destroy 时自动调用
    private void OnDestroy()
    {
        Destroy(skillUiObj);
    }
    // 设置攻速（可在外部调用）
    public void SetAttackSpeed(float newSpeed)
    {
        CurrentAttackSpeed = Mathf.Max(0.1f, newSpeed); // 限制最小速度

        // 更新动画速度
        if (animator != null)
        {
            // 计算动画速度乘数（基于基础攻速）
            float speedMultiplier = CurrentAttackSpeed / baseAttackSpeed;
            animator.SetFloat("AttackSpeedMultiplier", speedMultiplier);
        }
    }

    // 执行攻击动作
    protected virtual bool PerformAttack()
    {
        if (animator != null)
        {
            animator.Play(attackAnimationName, 0, 0f);
        }
        return false;
    }
    protected virtual IEnumerator PerformAttackIenum()
    {
        yield return null;
    }
    protected virtual void ChangeTx()
    {
        
    }
    //初始化武器数据
    public void SetWeaponState(CharacterStats _weaponState, CharacterStats _heroStats, int _performAttackType, GameObject _skillUiObj)
    {
        int oldQualityLevel = 1;
        if (weaponStats != null && weaponStats.qualityLevel > 1)
        {
            oldQualityLevel = weaponStats.qualityLevel;
        }
        weaponStats = _weaponState;
        weaponStats.qualityLevel = oldQualityLevel;
        heroStats = _heroStats;
        PerformAttackType = _performAttackType;
        if (_skillUiObj != null)
        {
            skillUiObj = _skillUiObj;
        }
        SetAttackSpeed(weaponStats.attackSpeed);
        UpdateFinalStats();
    }
    public void SetWeaponState(CharacterStats _weaponState)
    {
        weaponStats = _weaponState;
    }
    public CharacterStats GetWeaponStats()
    {
        return weaponStats;
    }
 
    //攻击检测
    private void CheckWeaponAttack()
    {
        if (Time.time < nextAttackTime) return;
        //测试用
        if (weaponStats.baseType == 201 && GameManager.instance.isSpace == false)
        {
            return;
        }

        switch (PerformAttackType)
        {
            case 1:
                if (PerformAttack())
                {
                    UpdateAttackCooldown();
                }
                break;
            case 2:
                UpdateAttackCooldown();
                StartCoroutine(PerformAttackIenum());
                break;

        }
    }
    void UpdateAttackCooldown()
    {
        float totalAttackSpeed = weaponStats.attackSpeed * (heroStats.attackSpeed/100f);
        float time = 1f / totalAttackSpeed;
        nextAttackTime = Time.time + time;
        bool isShake = true;
        if (weaponStats.baseType == 203)
        {
            isShake = false;
        }
        skillUiObj.GetComponent<SkillUi>().StartCooldown(time, time, isShake);
    }
    //获取伤害
    public HeroDamage GetDamage()
    {
        UpdateFinalType(heroStats);
        return finalState;
    }
    //更新最终属性
    public void UpdateFinalStats()
    {
        finalState = BattleManager.GetInstance().CalculateDamage(heroStats, weaponStats, DamageType.Ranged);
        ChangeTx();
        //更新ui 颜色
        if (skillUiObj != null)
        {
            GameManager.instance.ChangeColor(weaponStats.qualityLevel, skillUiObj.GetComponent<SkillUi>().GetMaterialInstance(), 5);
        }
        
    }
    //每次更新伤害是否暴击
    public void UpdateFinalType(CharacterStats _heroStats)
    {
        bool isCrit = Random.Range(0f, 1f) <= (_heroStats.critRate / 100f);
        finalState.type = isCrit ? 2 : 1;
    }

    public void CauseDamage(Collider _collider, HeroDamage _heroDamage)
    {
        if (_collider.CompareTag("Monster"))
        {
            _collider.GetComponent<MonsterChase>().TakeDamage(_heroDamage);

            Vector3 contactPoint = _collider.transform.Find("shangHaiTag").transform.position;
            GameManager.instance.SpawnAttackNumber(contactPoint, _heroDamage);
        }
    }

    //重置CD
    public void ResetCD()
    {
        nextAttackTime = 0;
        skillUiObj.GetComponent<SkillUi>().StartCooldown(0, 0, false);
    }
}
