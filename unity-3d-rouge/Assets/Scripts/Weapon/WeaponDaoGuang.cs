using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDaoGuang :WeaponContry
{
    private string txPath = "Weapon/DaoGuang/DaoGuangTx";
    [SerializeField] private Transform firePoint;         // 发射点
    private float projectileSpeed = 20f;  // 子弹速度
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override IEnumerator PerformAttackIenum()
    {
        HeroDamage heroDamage = base.GetDamage();
        var obj = BattleManager.GetInstance().GetRandomMonsterInRange(transform.position, heroDamage.range);
        Vector3 targetPosition = Vector3.zero;
        if (obj != null)
        {
            Vector3 looat = obj.transform.Find("lookAtTag").position;
            targetPosition = new Vector3(
                looat.x,
                transform.position.y,  // 保持自己的Y坐标
                looat.z
            );
            transform.LookAt(targetPosition);

        }
        // 品质等级
        int qualityLevelNumber = BattleManager.GetInstance().GetWeaponPinZhi(GetWeaponStats().baseType, GetWeaponStats().qualityLevel);
        for (int i = 0; i < qualityLevelNumber; i++)
        {
            //计算伤害
            heroDamage = base.GetDamage();
            // 实例化子弹
            var projectile = GameManager.instance.AddPrefab(txPath, BattleManager.GetInstance().TeXiaoObj.transform, firePoint.position, firePoint.rotation);
            projectile.GetComponent<ZiDan>().InitZiDan(heroDamage, string.Empty, 999);
            // 获取子弹的刚体组件
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            Vector3 finalDirection = firePoint.forward;
            if (rb != null)
            {
                // 施加力让子弹飞出去
                rb.velocity = finalDirection * projectileSpeed;
            }
            yield return new WaitForSeconds(0.2f);
        }

    }

}
