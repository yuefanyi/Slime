using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponShanDian : WeaponContry
{
    private string txPath = "Weapon/ShanDian/ShanDianTx";
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override IEnumerator PerformAttackIenum()
    {
        // 品质等级
        int qualityLevelNumber = BattleManager.GetInstance().GetWeaponPinZhi(GetWeaponStats().baseType, GetWeaponStats().qualityLevel);
        for (int i = 0; i < qualityLevelNumber; i++)
        {
            //计算伤害
            HeroDamage heroDamage = base.GetDamage();
            var obj = BattleManager.GetInstance().GetRandomMonsterInRange(transform.position, heroDamage.range);
            if (obj == null)
            {
                continue;
            }
            // 实例化子弹
            var projectile = GameManager.instance.AddPrefab(txPath, BattleManager.GetInstance().TeXiaoObj.transform, obj.transform.position, Quaternion.identity);
            projectile.GetComponent<ShanDian>().InitShanDian(heroDamage, null, 1);
            // 获取子弹的刚体组件
            //Rigidbody rb = projectile.GetComponent<Rigidbody>();
            //Vector3 finalDirection = firePoint.forward;
            //if (rb != null)
            //{
            //    // 施加力让子弹飞出去
            //    rb.velocity = finalDirection * projectileSpeed;
            //}
            yield return new WaitForSeconds(0.1f);
        }

    }
}
