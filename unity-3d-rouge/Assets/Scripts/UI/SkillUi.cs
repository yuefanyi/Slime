using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SkillUi : MonoBehaviour
{
    public int skillId = 0;
    public int level = 1;
    public Image cooldownImage; // 引用UI中的Image组件
    public Text counDown;
    public RectTransform uiElement;
    private float nextAttackTime;
    private float shakeCd = 0.1f;
    
    [SerializeField] private Image targetImage;

    //材质
    private Material materialInstance;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    void Awake()
    {
        uiElement = transform.rectTransform();
        targetImage = transform.Find("ImageDs").GetComponent<Image>();
        if (targetImage != null)
        {
            // 创建材质实例以避免修改共享材质
            materialInstance = new Material(targetImage.material);
            targetImage.material = materialInstance;
            GameManager.instance.ChangeColor(level, materialInstance,5);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetEmissionColor(Color color)
    {
        if (targetImage != null && targetImage.material != null)
        {
            targetImage.material.SetColor("_EmissionColor", color);
        }
    }

    void OnDestroy()
    {
        // 清理材质实例
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }

    // 开始冷却倒计时的方法
    public void StartCooldown(float remainingTime, float totalCooldownTime, bool isShake = true)
    {
        // 停止所有可能正在运行的协程，避免多个倒计时同时进行
        StopAllCoroutines();
        // 启动新的倒计时协程
        StartCoroutine(CooldownCoroutine(remainingTime, totalCooldownTime));
        float strength = 0.3f;

        if ( isShake &&  Time.time > nextAttackTime)
        {
            if (remainingTime > 0.3f)
            {
                remainingTime = 0.3f;
            }
            if (remainingTime < 0.1f)
            {
                remainingTime = 0.05f;
                strength = 0.1f;
            }
            nextAttackTime = Time.time + shakeCd;

            Shake(remainingTime, strength);
        }
   
    }

    public void Shake(float _time , float _strength)
    {
        //ShakePosition(_time);
        //ShakeRotation(_time);
        ShakeScale(_time, _strength);
    }

    public Material GetMaterialInstance()
    {
        return materialInstance;
    }

    // 倒计时协程
    private IEnumerator CooldownCoroutine(float remainingTime, float totalCooldownTime)
    {
        float timer = remainingTime; // 初始化计时器为剩余时间

        if (timer == 0)
        {
            cooldownImage.fillAmount = 0;
            counDown.text = timer.ToString("f2");
        }

        // 当计时器大于0时循环
        while (timer > 0)
        {
            // 根据剩余时间更新填充量
            // 填充量 = 剩余时间 / 总冷却时间
            cooldownImage.fillAmount = timer / totalCooldownTime;

            // 减少计时器（每帧减少Time.deltaTime）
            timer -= Time.deltaTime;
            counDown.text = timer.ToString("f2");
            // 等待下一帧
            yield return null;
        }

        // 确保冷却结束时图像完全为空
        cooldownImage.fillAmount = 0;
    }

    public void InitSkillId(int _skillId)
    {
        skillId = _skillId;
        cooldownImage = gameObject.transform.Find("ImageDs").GetComponent<Image>();
        counDown = gameObject.transform.Find("Text_CountDown").GetComponent<Text>();
        string path = "Tex/skill/" + skillId.ToString();
        GameManager.instance.SpritPropImageByPath(path, transform.Find("Icon").GetComponent<Image>());
    }

    public bool CheckSkillId(int _id)
    {
        return skillId == _id;
    }

    // 位置抖动
    private void ShakePosition(float remainingTime)
    {
        DOShakeUI(uiElement, remainingTime, 1f, 15, 90f, true);
    }

    // 旋转抖动
    private void ShakeRotation(float remainingTime)
    {
       DOShakeUIRotation(uiElement, remainingTime, 8f, 15, 180f);
    }

    // 缩放抖动
    private void ShakeScale(float remainingTime, float _strength)
    {
      DOShakeUIScale(uiElement, remainingTime, _strength, 15, 90f);
    }
    /// <summary>
    /// UI旋转抖动效果
    /// </summary>
    /// <param name="target">要抖动的UI元素</param>
    /// <param name="duration">抖动持续时间</param>
    /// <param name="strength">抖动强度</param>
    /// <param name="vibrato">振动次数</param>
    /// <param name="randomness">随机性(0-180)</param>
    /// <returns>DOTween序列</returns>
    public Sequence DOShakeUIRotation( RectTransform target,
    float duration = 0.5f,
    float strength = 5f,
    int vibrato = 10,
    float randomness = 90f)
    {
        // 保存原始旋转
        Vector3 originalRotation = target.localEulerAngles;

        // 创建序列
        Sequence sequence = DOTween.Sequence();

        Vector3 vector3 = new Vector3(0f, 0f, 20f);

        // 添加旋转抖动动画
        sequence.Append(target.DOShakeRotation(duration, vector3, vibrato, randomness));

        // 抖动结束后回到原始旋转
        sequence.AppendCallback(() => target.localEulerAngles = originalRotation);

        return sequence;
    }

    /// <summary>
    /// UI抖动效果
    /// </summary>
    /// <param name="target">要抖动的UI元素</param>
    /// <param name="duration">抖动持续时间</param>
    /// <param name="strength">抖动强度</param>
    /// <param name="vibrato">振动次数</param>
    /// <param name="randomness">随机性(0-180)</param>
    /// <param name="fadeOut">是否淡出抖动效果</param>
    /// <returns>DOTween序列</returns>
    public static Sequence DOShakeUI( RectTransform target,
        float duration = 0.5f,
        float strength = 10f,
        int vibrato = 10,
        float randomness = 90f,
        bool fadeOut = true)
    {
        // 保存原始位置
        Vector2 originalPos = target.anchoredPosition;

        // 创建序列
        Sequence sequence = DOTween.Sequence();

        // 添加抖动动画
        sequence.Append(target.DOShakeAnchorPos(duration, strength, vibrato, randomness, false, fadeOut));

        // 抖动结束后回到原始位置
        sequence.AppendCallback(() => target.anchoredPosition = originalPos);

        return sequence;
    }

    /// <summary>
    /// UI缩放抖动效果
    /// </summary>
    /// <param name="target">要抖动的UI元素</param>
    /// <param name="duration">抖动持续时间</param>
    /// <param name="strength">抖动强度</param>
    /// <param name="vibrato">振动次数</param>
    /// <param name="randomness">随机性(0-180)</param>
    /// <returns>DOTween序列</returns>
    public static Sequence DOShakeUIScale( RectTransform target,
        float duration = 0.5f,
        float strength = 0.2f,
        int vibrato = 10,
        float randomness = 90f)
    {
        // 保存原始缩放
        Vector3 originalScale = target.localScale;

        // 创建序列
        Sequence sequence = DOTween.Sequence();

        // 添加缩放抖动动画
        sequence.Append(target.DOShakeScale(duration, strength, vibrato, randomness));

        // 抖动结束后回到原始缩放
        sequence.AppendCallback(() => target.localScale = originalScale);

        return sequence;
    }
}
