using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZiDan : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private GameObject impactEffect;
    private string shoujiPath = string.Empty;
    private HeroDamage heroDamage;
    private Vector3 spawnPosition;      // 记录子弹生成时的位置
    private int maxTrigger = 1;
    private Collider myCollider;
    void FixedUpdate()
    {
        
    }

    void Start()
    {
        myCollider = GetComponent<Collider>();
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // 检查是否超出最大攻击距离
        if (heroDamage != null)
        {
            if (Vector3.Distance(spawnPosition, transform.position) >= heroDamage.range)
            {
                GameManager.instance.DestroyPrefab(gameObject);
            }
        }
      
    }

    public void InitZiDan(HeroDamage _heroDamage, string _shoujiPath, int _maxTrigger)
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
            if (shoujiPath != string.Empty)
            {
                GameManager.instance.AddPrefab(shoujiPath, BattleManager.GetInstance().TeXiaoObj.transform, transform.position, Quaternion.identity);
            }
            other.GetComponent<MonsterChase>().TakeDamage(heroDamage);
            // 计算other物体到触发器最近的点（近似接触点）
            Vector3 contactPoint = other.transform.Find("shangHaiTag").transform.position;
            GameManager.instance.SpawnAttackNumber(contactPoint, heroDamage);
            if (maxTrigger <= 0)
            {
                GameManager.instance.DestroyPrefab(gameObject);
            }
        }
        else if (other.CompareTag("Map"))
        {
            if (shoujiPath != string.Empty)
            {
                GameManager.instance.AddPrefab(shoujiPath, BattleManager.GetInstance().TeXiaoObj.transform, transform.position, Quaternion.identity);
            }
        }
  
    }
  

}
