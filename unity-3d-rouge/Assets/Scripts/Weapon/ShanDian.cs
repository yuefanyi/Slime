using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShanDian : MonoBehaviour
{
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private GameObject impactEffect;
    private string shoujiPath;
    private HeroDamage heroDamage;
    private Vector3 spawnPosition;      // 记录子弹生成时的位置
    private int maxTrigger = 1;
    private Collider myCollider;
    // Start is called before the first frame update
    void Start()
    {
        myCollider = GetComponent<Collider>();
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitShanDian(HeroDamage _heroDamage, string _shoujiPath, int _maxTrigger)
    {
        heroDamage = _heroDamage;
        shoujiPath = _shoujiPath;
        spawnPosition = this.transform.position;
        maxTrigger = _maxTrigger;
    }

    void OnTriggerEnter(Collider other)
    {
        // 排除不应该触发的层（如发射者、同队伍等）
        if (other.CompareTag("Player")) return;

        if (other.CompareTag("Monster"))
        {
            maxTrigger--;
            if (maxTrigger < 0)
            {
                return;
            }
            //GameManager.instance.AddPrefab(shoujiPath, BattleManager.GetInstance().TeXiaoObj.transform, transform.position, Quaternion.identity);
            other.GetComponent<MonsterChase>().TakeDamage(heroDamage);
            // 计算other物体到触发器最近的点（近似接触点）
            Vector3 contactPoint = other.transform.Find("shangHaiTag").transform.position;
            GameManager.instance.SpawnAttackNumber(contactPoint, heroDamage);

        }

    }
}
