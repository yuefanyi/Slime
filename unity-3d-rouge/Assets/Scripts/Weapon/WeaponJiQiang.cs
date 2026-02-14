using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponJiQiang : WeaponContry
{
    [Header("射击设置")]
    [SerializeField] private GameObject projectilePrefab;  // 子弹预制体
    [SerializeField] private Transform firePoint;         // 发射点
    [SerializeField] private ParticleSystem muzzleFlash;  // 枪口火焰特效
    [SerializeField] private AudioClip shootSound;        // 射击音效
    private float projectileSpeed = 20f;  // 子弹速度
    private float maxHorizontalSpread = 5f; // 左右各5度
    public float minPlayInterval = 0.1f; // 最小播放间隔（秒）
    private float lastPlayTime = -1f; // 上一次播放的时间

    protected override void Start()
    {
        base.Start();

    }

    protected override bool PerformAttack()
    {
        //base.PerformAttack(); // 调用父类的动画播放
        for (int i = 0; i < 1; i++)
        {
            HeroDamage heroDamage = base.GetDamage();
            var obj = BattleManager.GetInstance().GetNearestMonsterInRange(transform.position, heroDamage.range);
            if (obj == null)
            {
                return false;
            }
            transform.LookAt(obj.transform.Find("lookAtTag"));
            // 播放枪口火焰
            PlayMuzzleFlash();

            // 播放射击音效
            PlayShootSound();

            // 发射子弹
            FireProjectile();
        }
       

        return true;
    }

    private void PlayMuzzleFlash()
    {
        // 如果 muzzleFlash 未设置，直接返回
        if (muzzleFlash == null)
        {
            Debug.LogWarning("MuzzleFlash is not assigned!");
            return;
        }

        // 检查是否满足冷却时间
        float currentTime = Time.time;
        if (currentTime - lastPlayTime < minPlayInterval)
        {
            return; // 时间间隔太短，不播放
        }

        // 记录本次播放时间并触发效果
        lastPlayTime = currentTime;
        muzzleFlash.Play();
    }

    private void PlayShootSound()
    {
        
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("Projectile prefab or fire point not set!");
            return;
        }

        // 计算随机散布角度
        float randomSpread = Random.Range(-maxHorizontalSpread, maxHorizontalSpread);
        Quaternion spreadRotation = Quaternion.Euler(0, randomSpread, 0);

        // 应用随机散布到发射方向
        Vector3 finalDirection = spreadRotation * firePoint.forward;

        //计算伤害
        HeroDamage heroDamage = base.GetDamage();
        // 品质等级
        int qualityLevelNumber = BattleManager.GetInstance().GetWeaponPinZhi(GetWeaponStats().baseType, GetWeaponStats().qualityLevel);
        // 实例化子弹
        var projectile = GameManager.instance.AddPrefab("Weapon/JiQiang/ZiDan", BattleManager.GetInstance().TeXiaoObj.transform, firePoint.position, firePoint.rotation);
        projectile.GetComponent<ZiDan>().InitZiDan(heroDamage,"Weapon/JiQiang/ShouJi", qualityLevelNumber);

        // 获取子弹的刚体组件
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 施加力让子弹飞出去
            rb.velocity = finalDirection * projectileSpeed;
        }
        else
        {
            Debug.LogWarning("Projectile has no Rigidbody component!");
        }

        //设置子弹伤害

    }
}
