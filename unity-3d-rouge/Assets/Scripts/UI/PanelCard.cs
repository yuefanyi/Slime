using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelCard : MonoBehaviour
{
    private List<GameObject> skillIcon = new List<GameObject>();
    private List<GameObject> infoObj = new List<GameObject>();
    public void InitPanel(bool isPray = false)
    {
        UpdateDrawCardPanel(isPray);
        UpdateCardPanelSkillIcon();
    }
    //刷新抽卡模块
    public void UpdateDrawCardPanel(bool isPray = false)
    {
        List<int> list;
        if (BattleManager.GetInstance().levelManagement.CurrentLevel % 2 == 0)
        {
            list = BattleManager.GetInstance().DrawCards(3, BattleManager.GetInstance().HeroStats.luck, 2);
        }
        else
        {
            list = BattleManager.GetInstance().DrawCards(3, BattleManager.GetInstance().HeroStats.luck, 1);
        }
        if (isPray)
        {
            list = BattleManager.GetInstance().DrawCards(3, BattleManager.GetInstance().HeroStats.luck, 3);
        }
 
        for (int i = 0; i < list.Count; i++)
        {
            var cfg = BattleManager.GetInstance().configMag.GetCardInfoCfgCfgByKey(list[i]);
            gameObject.transform.Find("Cards/Card" + i).GetComponent<Card>().InitCard(cfg);
            gameObject.transform.Find("Cards/Card" + i).gameObject.SetActive(true);
        }
        transform.Find("Cards/ButtonChange/Text").GetComponent<Text>().text = ":" + BattleManager.GetInstance().levelManagement.GetNeedCurrentJbByNumber();
        BattleManager.GetInstance().UpdateJbUi();

    
    }
    //刷新技能栏
    public void UpdateCardPanelSkillIcon()
    {
        foreach (var item in skillIcon)
        {
            Destroy(item);
        }
        skillIcon.Clear();
        //实例化技能
        int number = BattleManager.GetInstance().GetNowSkillNumber();
        for (int i = 0; i < number; i++)
        {
            var obj = GameManager.instance.AddPrefab("UI/CardIcon", transform.Find("SkillIcons"));
            obj.GetComponent<CardIcon>().InitIndex(i);
            obj.transform.Find("Kuang").gameObject.SetActive(false);
            obj.transform.Find("Image").gameObject.SetActive(false);
            obj.transform.Find("Button").gameObject.SetActive(false);
            obj.transform.Find("ButtonSm").gameObject.SetActive(false);
            obj.transform.Find("Image (1)").gameObject.SetActive(false);
            obj.transform.Find("TextLevel").gameObject.SetActive(false);
            var materialInstance = new Material(obj.transform.Find("Kuang").GetComponent<Image>().material);
            obj.transform.Find("Kuang").GetComponent<Image>().material = materialInstance;
            skillIcon.Add(obj);
        }
        var list = BattleManager.GetInstance().GetWeaponList();
        if (number < list.Count)
        {
            Debug.LogError("数据异常");
            return;
        }
        //设置技能图标和外框
        for (int i = 0; i < list.Count; i++)
        {
            var stats = list[i].GetComponent<WeaponContry>().GetWeaponStats();
            skillIcon[i].transform.Find("Kuang").gameObject.SetActive(true);
            skillIcon[i].transform.Find("Image").gameObject.SetActive(true);
            skillIcon[i].transform.Find("Button").gameObject.SetActive(true);
            skillIcon[i].transform.Find("ButtonSm").gameObject.SetActive(true);
            skillIcon[i].transform.Find("Image (1)").gameObject.SetActive(true);
            skillIcon[i].transform.Find("TextLevel").gameObject.SetActive(true);
            string path = "Tex/skill/" + stats.baseType.ToString();
            GameManager.instance.SpritPropImageByPath(path, skillIcon[i].transform.Find("Image").GetComponent<Image>());
            GameManager.instance.ChangeColor(stats.qualityLevel, skillIcon[i].transform.Find("Kuang").GetComponent<Image>().material, 3);
            skillIcon[i].transform.Find("TextLevel").GetComponent<Text>().text = stats.level.ToString();
        }
        transform.Find("ButtonSkill/Text").GetComponent<Text>().text = ":" + BattleManager.GetInstance().levelManagement.GetAddSkillBoxNeedJb();
        UpdateHeroInfo();
        
    }
    //刷新数据模块
    public void UpdateHeroInfo()
    {
        foreach (var item in infoObj)
        {
            Destroy(item);
        }
        infoObj.Clear();
        var trs = transform.Find("Info");
        CharacterStats heroState = BattleManager.GetInstance().HeroStats;
        for (int i = 0; i < 8; i++)
        {
            var obj = GameManager.instance.AddPrefab("UI/CardInfo", trs);
            int index = i + 1;
            //设置图标
            GameManager.instance.SpritPropImageByPath("Tex/skill/"+index.ToString(), obj.transform.Find("Image").GetComponent<Image>());
            //设置name
            obj.transform.Find("Text_Name").GetComponent<Text>().text = GameManager.instance.configMag.GetMsgInfoCfgByKey(index).msg;
            //设置数值
            obj.transform.Find("Text_Numaber").GetComponent<Text>().text = GetHeroNumber(index, heroState);
            infoObj.Add(obj);
        }
    
    }
    //获取英雄数值
    public string GetHeroNumber(int _index, CharacterStats _heroState)
    {
        string str = string.Empty;
        if (_index == 1)
        {
            str = _heroState.baseDamage.ToString();
        }
        else if (_index == 2)
        {
            str = _heroState.attackSpeed.ToString();
        }
        else if (_index == 3)
        {
            str = _heroState.moveSpeed.ToString();
        }
        else if (_index == 4)
        {
            str = _heroState.range.ToString();
        }
        else if (_index == 5)
        {
            str = _heroState.luck.ToString();
        }
        else if (_index == 6)
        {
            str = _heroState.critRate.ToString();
        }
        else if (_index == 7)
        {
            str = _heroState.critMultiplier.ToString();
        }
        else if (_index == 8)
        {
            str = _heroState.harvest.ToString();
        }
        return str;
    }

    public void Test1()
    {
        BattleManager.GetInstance().WeaponUpLevel(0, 1);
    }
}
