using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening; // 引入DOTween命名空间

public class Tips : MonoBehaviour
{
    private Coroutine destroyCoroutine;
    private CanvasGroup canvasGroup;
    private Text textComponent;
    private Tween fadeTween; // 存储DOTween动画引用

    public void InitTips(float _time, string _msg)
    {
        // 获取或添加CanvasGroup组件
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 初始设置为完全可见
        canvasGroup.alpha = 1f;

        // 获取文本组件
        textComponent = transform.Find("Text").GetComponent<Text>();
        textComponent.text = _msg;

        // 启动销毁协程
        destroyCoroutine = StartCoroutine(DestroyAfterTime(_time));
    }

    private IEnumerator DestroyAfterTime(float time)
    {
        // 等待大部分时间（保留0.5秒用于淡出效果）
        float displayTime = time - 0.5f;
        if (displayTime > 0)
        {
            yield return new WaitForSecondsRealtime(displayTime);
        }

        // 使用DOTween实现淡出效果
        if (fadeTween != null && fadeTween.IsActive())
        {
            fadeTween.Kill(); // 先停止可能存在的动画
        }

        // 创建淡出动画
        fadeTween = canvasGroup.DOFade(0f, 0.5f)
            .SetUpdate(true) // 设置为true表示不受Time.timeScale影响
            .SetEase(Ease.OutQuad) // 使用缓动函数使动画更自然
            .OnComplete(() =>
            {
                // 动画完成后销毁对象
                Destroy(gameObject);
            });
    }

    // 使用DOTween的简化版本
    public void InitTipsWithDOTween(float _time, string _msg)
    {
        // 获取或添加CanvasGroup组件
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 初始设置为完全可见
        canvasGroup.alpha = 1f;

        // 获取文本组件
        textComponent = transform.Find("Text").GetComponent<Text>();
        textComponent.text = _msg;

        // 使用DOTween的延迟调用和淡出动画
        DOVirtual.DelayedCall(_time - 0.5f, () =>
        {
            FadeOutAndDestroy(0.5f);
        }, false).SetUpdate(true);
    }

    // 使用DOTween实现淡出效果
    public void FadeOutAndDestroy(float duration = 0.5f)
    {
        // 停止所有可能正在运行的协程
        if (destroyCoroutine != null)
        {
            StopCoroutine(destroyCoroutine);
        }

        // 停止可能存在的动画
        if (fadeTween != null && fadeTween.IsActive())
        {
            fadeTween.Kill();
        }

        // 创建淡出动画
        fadeTween = canvasGroup.DOFade(0f, duration)
            .SetUpdate(true)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                Destroy(gameObject);
            });
    }

    // 带淡入效果的初始化
    public void InitTipsWithFadeIn(float _time, string _msg, float fadeInDuration = 0.2f)
    {
        // 获取或添加CanvasGroup组件
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 初始设置为完全透明
        canvasGroup.alpha = 0f;

        // 获取文本组件
        textComponent = transform.Find("Text").GetComponent<Text>();
        textComponent.text = _msg;

        // 淡入效果
        fadeTween = canvasGroup.DOFade(1f, fadeInDuration)
            .SetUpdate(true)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // 淡入完成后启动销毁计时
                destroyCoroutine = StartCoroutine(DestroyAfterTime(_time - fadeInDuration));
            });
    }

    public void DesThis()
    {
        // 停止所有可能正在运行的协程
        if (destroyCoroutine != null)
        {
            StopCoroutine(destroyCoroutine);
        }

        // 停止可能存在的动画
        if (fadeTween != null && fadeTween.IsActive())
        {
            fadeTween.Kill();
        }

        // 直接销毁对象
        Destroy(gameObject);
    }

    // 立即淡出并销毁
    public void DesThisWithFade(float fadeDuration = 0.5f)
    {
        FadeOutAndDestroy(fadeDuration);
    }

    // 当对象禁用时清理资源
    private void OnDisable()
    {
        if (destroyCoroutine != null)
        {
            StopCoroutine(destroyCoroutine);
        }

        if (fadeTween != null && fadeTween.IsActive())
        {
            fadeTween.Kill();
        }
    }

    // 当对象销毁时确保清理
    private void OnDestroy()
    {
        if (fadeTween != null && fadeTween.IsActive())
        {
            fadeTween.Kill();
        }
    }
}