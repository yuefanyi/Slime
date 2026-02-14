using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCiChang : WeaponContry
{

    private string txPath = "Weapon/CiChang/CiChangTx";       //磁场特效
    private GameObject txObj = null;
    private float AddRange = 1f;

    protected override void Start()
    {
        base.Start();
        txObj = GameManager.instance.AddPrefab(txPath, BattleManager.GetInstance().GetHeroObj().transform);
        ChangeTx();
    }

    // 攻击检测方法
    protected override bool PerformAttack()
    {
        if (txObj == null)
        {
            return false;
        }
        int qualityLevelNumber = BattleManager.GetInstance().GetWeaponPinZhi(GetWeaponStats().baseType, GetWeaponStats().qualityLevel);
        AddRange = 1 + (float)qualityLevelNumber * 0.3f;
        int monsterLayerMask = 1 << LayerMask.NameToLayer("Monster");

        //计算伤害
        HeroDamage heroDamage = base.GetDamage();

        Collider[] hitColliders = Physics.OverlapSphere(txObj.transform.position, finalState.range * AddRange, monsterLayerMask, QueryTriggerInteraction.Collide);
        // 遍历所有检测到的碰撞体
        foreach (var collider in hitColliders)
        {
            CauseDamage(collider, heroDamage);
        }
        return true;
    }
    protected override void ChangeTx()
    {
        if (txObj != null)
        {
            txObj.transform.localScale = Vector3.one * finalState.range * AddRange;
        }
    }
}
