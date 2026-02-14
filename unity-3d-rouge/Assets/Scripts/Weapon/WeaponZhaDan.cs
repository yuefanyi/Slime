using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponZhaDan : WeaponContry
{
    private string txPath = "Weapon/ZhaDan/ZhaDanTx";       //炸弹特效
    private string stPath = "Weapon/ZhaDan/ZhaDanSt";       //炸弹实体


    // 使用DOTween实现跳跃动画
    private float jumpDuration = 0.5f; // 跳跃持续时间
    [SerializeField] private float jumpPower = 1.5f;      // 跳跃高度
    Vector3 targetPosition1;
    protected override void Start()
    {
        base.Start();
    }

    //protected override IEnumerator PerformAttackIenum()
    //{
    //    // 品质等级
    //    int qualityLevelNumber = BattleManager.GetInstance().GetWeaponPinZhi(GetWeaponStats().baseType, GetWeaponStats().qualityLevel);
    //    for (int i = 0; i < qualityLevelNumber; i++)
    //    {
    //        //计算伤害
    //        HeroDamage heroDamage = base.GetDamage();
    //        GameObject obj = BattleManager.GetInstance().GetRandomMonsterInRange(transform.position, 10);
    //        if (obj == null)
    //        {
    //            //Debug.Log("未找到"+ heroDamage.range.ToString());
    //            ResetCD();
    //            continue;
    //        }
    //        base.skillUiObj.GetComponent<SkillUi>().Shake(0.3f, 0.3f);
    //        // 获取目标位置
    //        Vector3 targetPosition = obj.transform.position;
    //        targetPosition1 = targetPosition;
    //        // 实例化炸弹
    //        var projectile = GameManager.instance.AddPrefab(stPath, BattleManager.GetInstance().TeXiaoObj.transform, transform.position, transform.rotation);
    //        //projectile.GetComponent<ShanDian>().InitShanDian(heroDamage, null, 1);
            
    //        projectile.transform.DOJump(targetPosition, jumpPower, 1, jumpDuration)
    //        .SetEase(Ease.Linear);

    //        yield return new WaitForSeconds(jumpDuration);
    //        //销毁炸弹实体
    //        GameManager.instance.DestroyPrefab(projectile);

    //        var txObj = GameManager.instance.AddPrefab(txPath, BattleManager.GetInstance().TeXiaoObj.transform, targetPosition, Quaternion.identity);

    //        int monsterLayerMask = 1 << LayerMask.NameToLayer("Monster");

    //        Collider[] hitColliders = Physics.OverlapSphere(targetPosition, finalState.range, monsterLayerMask, QueryTriggerInteraction.Collide);
    //        // 遍历所有检测到的碰撞体
    //        foreach (var collider in hitColliders)
    //        {
    //            CauseDamage(collider, heroDamage);
    //        }

    //        yield return new WaitForSeconds(1f);

    //        if (txObj != null)
    //        {
    //            GameManager.instance.DestroyPrefab(txObj);
    //        }
    //        //销毁特效
    //        yield return new WaitForSeconds(0.2f);
    //    }
    //}

    protected override IEnumerator PerformAttackIenum()
    {
        // 品质等级
        int qualityLevelNumber = BattleManager.GetInstance().GetWeaponPinZhi(GetWeaponStats().baseType, GetWeaponStats().qualityLevel);
        bool isShake = false;
        for (int i = 0; i < qualityLevelNumber; i++)
        {
            //计算伤害
            HeroDamage heroDamage = base.GetDamage();
            GameObject obj = BattleManager.GetInstance().GetRandomMonsterInRange(transform.position, 10);
            if (obj == null)
            {
                //Debug.Log("未找到"+ heroDamage.range.ToString());
                ResetCD();
                continue;
            }
            if (isShake == false)
            {
                base.skillUiObj.GetComponent<SkillUi>().Shake(0.3f, 0.3f);
                isShake = true;
            }
            // 获取目标位置
            Vector3 targetPosition = obj.transform.position;
            targetPosition1 = targetPosition;
            // 实例化炸弹
            var projectile = GameManager.instance.AddPrefab(stPath, BattleManager.GetInstance().TeXiaoObj.transform, transform.position, transform.rotation);
            //projectile.GetComponent<ShanDian>().InitShanDian(heroDamage, null, 1);

            projectile.transform.DOJump(targetPosition, jumpPower, 1, jumpDuration)
            .SetEase(Ease.Linear);

            yield return new WaitForSeconds(0.1f);
            StartCoroutine(Attack(projectile, targetPosition, heroDamage));
        }
    }

    private IEnumerator Attack(GameObject projectile, Vector3 targetPosition, HeroDamage heroDamage)
    {
        yield return new WaitForSeconds(jumpDuration - 0.1f);
        //销毁炸弹实体
        GameManager.instance.DestroyPrefab(projectile);

        var txObj = GameManager.instance.AddPrefab(txPath, BattleManager.GetInstance().TeXiaoObj.transform, targetPosition, Quaternion.identity);

        int monsterLayerMask = 1 << LayerMask.NameToLayer("Monster");

        Collider[] hitColliders = Physics.OverlapSphere(targetPosition, finalState.range, monsterLayerMask, QueryTriggerInteraction.Collide);
        // 遍历所有检测到的碰撞体
        foreach (var collider in hitColliders)
        {
            CauseDamage(collider, heroDamage);
        }

        yield return new WaitForSeconds(1f);

        if (txObj != null)
        {
            GameManager.instance.DestroyPrefab(txObj);
        }
        //销毁特效
        yield return new WaitForSeconds(0.2f);
    }


    // 或者使用 OnDrawGizmos 使其始终可见（不仅是选中时）
    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawSphere(targetPosition1, BomNumber);
    //}
}
