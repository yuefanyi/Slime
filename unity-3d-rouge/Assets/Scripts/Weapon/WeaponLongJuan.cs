using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponLongJuan : WeaponContry
{
    private string txPath = "Weapon/LongJuan/LongJuanTx";       //龙卷特效
    private List<GameObject> txObjList = new List<GameObject>();

    // 移动相关变量
    private List<Vector3> targetPositionList = new List<Vector3>();
    private float moveInterval = 3f; // 移动间隔时间（A段时间）
    private float moveTimer = 0f;
    private float moveSpeed = 1f; // 移动速度

    // 地图边界（根据实际需要调整）
    private float minX = -10f;
    private float maxX = 10f;
    private float minZ = -10f;
    private float maxZ = 10f;

    protected override void Start()
    {
        base.Start();

        ChangeNumber();
        // 初始化目标位置
        GenerateRandomTargetPosition();
        ChangeTx();
    }

    protected override void Update()
    {
        base.Update();

        // 移动逻辑
        moveTimer += Time.deltaTime;
        if (moveTimer >= moveInterval)
        {
            moveTimer = 0f;
            GenerateRandomTargetPosition();
        }

        // 缓慢向目标位置移动
        for (int i = 0; i < targetPositionList.Count; i++)
        {
          txObjList[i].transform.position = Vector3.MoveTowards(txObjList[i].transform.position,
          targetPositionList[i], moveSpeed * Time.deltaTime);
        }
      
    }

    // 生成随机目标位置
    private void GenerateRandomTargetPosition()
    {
        int needNumber = txObjList.Count - targetPositionList.Count;
        for (int i = 0; i < needNumber; i++)
        {
            targetPositionList.Add(new Vector3());
        }
        for (int i = 0; i < targetPositionList.Count; i++)
        {
            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);
            targetPositionList[i] = new Vector3(randomX, 0.33f, randomZ);
        }
       
    }

    // 攻击检测方法
    protected override bool PerformAttack()
    {
        for (int i = 0; i < txObjList.Count; i++)
        {
            int monsterLayerMask = 1 << LayerMask.NameToLayer("Monster");

            //计算伤害
            HeroDamage heroDamage = base.GetDamage();

            Collider[] hitColliders = Physics.OverlapSphere(txObjList[i].transform.position, finalState.range, monsterLayerMask, QueryTriggerInteraction.Collide);
            // 遍历所有检测到的碰撞体
            foreach (var collider in hitColliders)
            {
                CauseDamage(collider, heroDamage);
            }
        }
        //if (txObj == null)
        //{
        //    return false;
        //}
        return true;
    }

    protected override void ChangeTx()
    {
        ChangeNumber();
        for (int i = 0; i < txObjList.Count; i++)
        {
            if (txObjList[i] != null)
            {
                txObjList[i].transform.localScale = Vector3.one * finalState.range;
            }
        }
    }

    // 可选：当对象销毁时清理资源
    protected virtual void OnDestroy()
    {
        for (int i = 0; i < txObjList.Count; i++)
        {
            if (txObjList[i] != null)
            {
                Destroy(txObjList[i]);
            }
        }
        txObjList.Clear();
        targetPositionList.Clear();
    }

    public void ChangeNumber()
    {
        int qualityLevelNumber = BattleManager.GetInstance().GetWeaponPinZhi(GetWeaponStats().baseType, GetWeaponStats().qualityLevel);
        int number = txObjList.Count;
        int needNumber = qualityLevelNumber - number;
        for (int i = 0; i < needNumber; i++)
        {
            Vector3 vector3 = new Vector3(transform.position.x, 0.33f, transform.position.z);
            var txObj = GameManager.instance.AddPrefab(txPath, BattleManager.GetInstance().TeXiaoObj.transform, vector3, transform.rotation);
            txObjList.Add(txObj);
        }
        GenerateRandomTargetPosition();
    }
}
