using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDaShuChase : MonsterChase
{
    [Header("移动设置")]
    private float attackRange = 3f;   // 发动冲刺攻击的范围

    [Header("冲刺技能设置")]
    private float dashSpeed = 4f;           // 冲刺速度
    private float dashDuration = 2f;        // 冲刺持续时间
    private float dashCooldown = 10f;        // 冲刺冷却时间

    private bool isDashing = false;
    private float lastDashTime = -999f; // 初始化为一个很小的值，确保第一次可以立即使用

    protected override void Update()
    {
        if (player == null || isDashing) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 攻击范围检测 - 使用冲刺技能
        if (distanceToPlayer <= attackRange && CanDash())
        {
            DashAttack();
            return; // 使用冲刺技能后直接返回，不执行其他移动逻辑
        }

        // 距离检测逻辑
        if (distanceToPlayer <= chaseDistance)
        {
            base.ChasePlayer();
        }
        else
        {
            base.StopChase();
        }

        // 更新动画状态
        base.UpdateAnimation();
    }

    // 判断是否可以冲刺
    protected virtual bool CanDash()
    {
        return Time.time - lastDashTime >= dashCooldown;
    }

    // 获取冲刺冷却剩余时间
    public float GetDashCooldownRemaining()
    {
        float timeSinceLastDash = Time.time - lastDashTime;
        return Mathf.Max(0f, dashCooldown - timeSinceLastDash);
    }

    // 是否正在冷却
    public bool IsDashOnCooldown()
    {
        return GetDashCooldownRemaining() > 0f;
    }

    // 冲刺攻击
    protected void DashAttack()
    {
        if (Time.time - lastDashTime < dashCooldown) return;

        StartCoroutine(DashAttackCoroutine());

    }

    private IEnumerator DashAttackCoroutine()
    {
        isDashing = true;
        lastDashTime = Time.time;

        // 停止当前所有移动
        base.StopChase();

        // 面向玩家（锁定Y轴）
        Vector3 playerPosition = player.position;
        Vector3 lookAtPosition = new Vector3(playerPosition.x, transform.position.y, playerPosition.z);
        transform.LookAt(lookAtPosition);

        var obj = GameManager.instance.AddPrefab("Weapon/TongYong/ZhuangJiTx", BattleManager.GetInstance().TeXiaoObj.transform, transform.position, transform.rotation);
        base.animator.SetFloat("Speed", 0);
        yield return new WaitForSeconds(1f);

        // 计算冲刺方向（锁定Y轴）
        Vector3 currentPosition = transform.position;
        Vector3 dashDirection = (lookAtPosition - currentPosition).normalized;

        // 计算目标位置，保持Y轴不变
        Vector3 targetPosition = currentPosition + dashDirection * dashSpeed * dashDuration;
        targetPosition.y = currentPosition.y; // 锁定Y轴

        // 使用DOTween实现冲刺
        transform.DOMove(targetPosition, dashDuration)
            .SetEase(Ease.Linear)
            .OnComplete(OnDashComplete);

        base.animator.SetBool("isRun", true);

        yield return new WaitForSeconds(dashDuration);

        base.animator.SetBool("isRun", false);
        yield return new WaitForSeconds(3f);

        GameManager.instance.DestroyPrefab(obj);

        base.ChasePlayer();

    }
    protected void OnDashComplete()
    {
        isDashing = false;

        // 这里可以添加冲刺结束后的逻辑，比如检测是否击中玩家
        //CheckDashHit();
    }
}
