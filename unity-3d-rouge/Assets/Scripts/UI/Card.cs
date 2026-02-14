using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    CardInfoCfg cardInfoCfg;
    private float pulseScale = 1.05f;
    private float pulseDuration = 1f;

    private Vector3 originalScale;
    private RectTransform rectTransform;

    void Start()
    {
       
    }
    private void Awake()
    {
        
    }
    public void InitCard(CardInfoCfg _cardInfoCfg)
    {
        cardInfoCfg = _cardInfoCfg;
        UpdateUi();
        // 启动脉冲动画
        StartPulse();
    }

    public void UpdateUi()
    {
        transform.Find("Text_Name").GetComponent<Text>().text = cardInfoCfg.name;
        transform.Find("Text_Msg").GetComponent<Text>().text = cardInfoCfg.msg.ToString();
        transform.Find("Text_Level").GetComponent<Text>().text = cardInfoCfg.level.ToString();
        transform.Find("Text_Jb").GetComponent<Text>().text = ":"+cardInfoCfg.needJb.ToString();
        string path = "Tex/Level" + cardInfoCfg.level.ToString();
        GameManager.instance.SpritPropImageByPath(path, transform.Find("Level").GetComponent<Image>());
        path = "Tex/skill/" + cardInfoCfg.skillId.ToString();
        GameManager.instance.SpritPropImageByPath(path, transform.Find("Icon").GetComponent<Image>());
        string pathStart = "Tex/Star";
        string pathStart2 = "Tex/StarDark";
        for (int i = 0; i < 5; i++)
        {
            if (i < cardInfoCfg.level)
            {
                GameManager.instance.SpritPropImageByPath(pathStart, transform.Find("Starts/Image" + i).GetComponent<Image>());
            }
            else
            {
                GameManager.instance.SpritPropImageByPath(pathStart2, transform.Find("Starts/Image" + i).GetComponent<Image>());
            }
        }
    }
    //选择技能
    public void ChoiceCard()
    {
        BattleManager.GetInstance().PlayerChoiceCard(cardInfoCfg, gameObject);
    }



    public void StartPulse()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = Vector3.one;

        rectTransform.DOScale(originalScale * pulseScale, pulseDuration)
            .SetLoops(-1, LoopType.Yoyo) // Yoyo模式让动画来回播放
            .SetEase(Ease.InOutSine).SetUpdate(true); ;
    }
}
