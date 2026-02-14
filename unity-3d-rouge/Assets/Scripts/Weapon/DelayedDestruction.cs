using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedDestruction : MonoBehaviour
{
    [SerializeField] private float lifetime = 1f;

    private void OnEnable()
    {
        // 每次激活时，启动延时销毁
        Invoke(nameof(DestroyAfterDelay), lifetime);
    }

    private void OnDisable()
    {
        // 如果对象被提前禁用（如手动 SetActive(false)），取消未执行的 Invoke
        CancelInvoke(nameof(DestroyAfterDelay));
    }

    private void DestroyAfterDelay()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.DestroyPrefab(gameObject);
        }
        else
        {
            Debug.LogWarning("GameManager instance is missing!");
            Destroy(gameObject); // 备用方案，直接销毁
        }
    }
}
