using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // 添加DOTween命名空间

public class DaoJu : MonoBehaviour
{
    [Header("自转设置")]
    [SerializeField] private float rotationSpeed = 90f; // 每秒旋转角度
    [SerializeField] private Vector3 rotationAxis = Vector3.up; // 旋转轴，默认绕Y轴旋转
    private float oldrotationSpeed = 0f;

    [Header("拾取动画设置")]
    [SerializeField] private float jumpHeight = 0.3f; // 弹跳高度
    [SerializeField] private float jumpDuration = 0.5f; // 弹跳持续时间
    [SerializeField] private float scaleMultiplier = 1.3f; // 缩放倍数
    [SerializeField] private float scaleDuration = 0.3f; // 缩放持续时间

    private bool isCollected = false; // 防止重复触发
    private bool isMove = false;        //是否可以传送

    void Update()
    {
        // 自转功能
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // 排除不应该触发的层（如发射者、同队伍等）
        if (other.CompareTag("Player") == false) return;

        // 防止重复触发
        if (isCollected) return;

        if (this.gameObject.tag == "XiTie")
        {
            isCollected = true;
            StartCoroutine(XiTie());
        }
        if (this.gameObject.tag == "ChuanSongMenStore" && isMove)
        {
            isMove = false;
            BattleManager.GetInstance().BeginBattle();
            Debug.Log("传送门");
        }
        if (this.gameObject.tag == "ChuanSongMenBattle" && isMove)
        {
            isMove = false;
            //BattleManager.GetInstance().GotoStore();
            BattleManager.GetInstance().BeginBattle();
        }
        if (this.gameObject.tag == "SkillBox")
        {
            if (cardInfoCfg == null)
            {
                Debug.Log("技能盒子异常");
                return;
            }
            StartCoroutine(SkillBox());
        }
        if (this.gameObject.tag == "DaoCaoRen")
        {
            BattleManager.GetInstance().PlayerLevelUp(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (this.gameObject.tag == "ChuanSongMenStore")
        {
            BattleManager.GetInstance().ChuanSongMenContry(1,true);
        }
    }

    private IEnumerator XiTie()
    {
        // 播放拾取动画
        yield return StartCoroutine(PlayCollectionAnimation());

        // 完成动画后执行原有逻辑
        BattleManager.GetInstance().SetSqrCollectRadius();

        Destroy(this.gameObject);
    }

    private IEnumerator SkillBox()
    {
        // 播放拾取动画
        yield return StartCoroutine(PlayCollectionAnimation());

        // 完成动画后执行原有逻辑
        BattleManager.GetInstance().PlayerChoiceCard(cardInfoCfg, null);
        Destroy(this.gameObject);

    }

    private IEnumerator PlayCollectionAnimation()
    {
        // 创建动画序列
        Sequence collectionSequence = DOTween.Sequence();

        // 同时执行向上弹跳和缩放动画
        collectionSequence.Join(transform.DOJump(
            transform.position + Vector3.up * jumpHeight,
            jumpHeight,
            1,
            jumpDuration).SetEase(Ease.OutQuad));

        collectionSequence.Join(transform.DOScale(
            transform.localScale * scaleMultiplier,
            scaleDuration).SetEase(Ease.OutBack));

        // 等待动画完成
        yield return collectionSequence.WaitForCompletion();
    }

    //关闭传送门
    public void CloseChuanSong()
    {
        isMove = false;
        oldrotationSpeed = rotationSpeed;
        rotationSpeed = 0;
        this.transform.Find("ChuanSongMen").gameObject.SetActive(false);
        this.transform.Find("ChuanSongMen (2)").gameObject.SetActive(false);
        this.transform.Find("ChuanSongMen (1)").gameObject.SetActive(false);
    }
    //开启传送门
    public void OpenChuanSong()
    {
        isMove = true;
        rotationSpeed = oldrotationSpeed;
        this.transform.Find("ChuanSongMen").gameObject.SetActive(true);
        this.transform.Find("ChuanSongMen (2)").gameObject.SetActive(true);
        this.transform.Find("ChuanSongMen (1)").gameObject.SetActive(true);
    }

    private string texturePath = "Tex/Skill/";
    private string materialPath = "daoju/BoxTu"; // 基础材质

    public void ChangeMaterialWithTexture()
    {
        // 加载贴图
        if (cardInfoCfg == null)
        {
            Debug.Log("技能盒失败数据异常");
            return;
        }
        string name = texturePath + cardInfoCfg.skillId;
        Debug.Log(name);
        Texture2D texture = Resources.Load<Texture2D>(name);
        // 加载基础材质
        Material baseMaterial = Resources.Load<Material>(materialPath);

        if (texture != null && baseMaterial != null)
        {
            Renderer renderer = this.transform.Find("Box/Cube").GetComponent<Renderer>();
            if (renderer != null)
            {
                // 创建材质实例
                Material materialInstance = new Material(baseMaterial);
                materialInstance.mainTexture = texture;
                renderer.material = materialInstance;

                Debug.Log("材质实例创建并应用成功！");
            }
        }
        else
        {
            Debug.LogError("贴图或材质加载失败！");
        }
    }

    public CardInfoCfg cardInfoCfg = null;
    public void InitCardInfoCfg(CardInfoCfg _cardInfoCfg)
    {
        cardInfoCfg = _cardInfoCfg;
    }
}