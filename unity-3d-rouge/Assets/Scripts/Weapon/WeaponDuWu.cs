using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDuWu : WeaponContry
{

    private string txPath = "Weapon/DuWu/DuWuTx";       //炸弹特效
    private int attackNumber = 10;                           //毒雾攻击次数
    private List<GameObject> txObjList = new List<GameObject>();

    protected override void Start()
    {
        base.Start();
        ChangeTx();
    }
    protected override IEnumerator PerformAttackIenum()
    {
        int qualityLevelNumber = BattleManager.GetInstance().GetWeaponPinZhi(GetWeaponStats().baseType, GetWeaponStats().qualityLevel);
        // 设置随机半径范围
        float randomRadius = 1.0f; // 你可以调整这个半径值

        for (int i = 0; i < qualityLevelNumber; i++)
        {
            // 获取目标位置
            // 在半径范围内生成随机偏移
            Vector2 randomOffset = Random.insideUnitCircle * randomRadius;

            // 应用随机偏移到目标位置
            Vector3 targetPosition = new Vector3(
                transform.position.x + randomOffset.x,
                0.35f,
                transform.position.z + randomOffset.y
            );

            // 实例化特效
            var txObj = GameManager.instance.AddPrefab(txPath, BattleManager.GetInstance().TeXiaoObj.transform, targetPosition, transform.rotation);
            txObjList.Add(txObj);
            ChangeTx();
            StartCoroutine(Attack(txObj, targetPosition));
            yield return new WaitForSeconds(0.1f);
        }
     
    }

    private IEnumerator Attack(GameObject txObj, Vector3 targetPosition) 
    {
        yield return new WaitForSeconds(1f);

        int monsterLayerMask = 1 << LayerMask.NameToLayer("Monster");

        for (int j = 0; j < attackNumber; j++)
        {
            //计算伤害
            HeroDamage heroDamage = base.GetDamage();

            Collider[] hitColliders = Physics.OverlapSphere(targetPosition, finalState.range, monsterLayerMask, QueryTriggerInteraction.Collide);
            // 遍历所有检测到的碰撞体
            foreach (var collider in hitColliders)
            {
                CauseDamage(collider, heroDamage);
            }

            yield return new WaitForSeconds(0.4f);
        }
        yield return new WaitForSeconds(3f);

        if (txObj != null)
        {
            txObjList.Remove(txObj);
            GameManager.instance.DestroyPrefab(txObj);
        }
    }

    protected override void ChangeTx()
    {
        foreach (var item in txObjList)
        {
            item.transform.localScale = Vector3.one * finalState.range;
        }
    }

}
