using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using System.Linq;
using System.IO;

public class ConfigManager
{

    public List<MonsterInfoCfg> MonsterInfoCfg = new List<MonsterInfoCfg>();                       //怪物配置
    public List<LevelInfoCfg> LevelInfoCfg = new List<LevelInfoCfg>();                             //关卡配置
    public List<SkillInfoCfg> SkillInfoCfg = new List<SkillInfoCfg>();                             //技能配置
    public List<CardInfoCfg> CardInfoCfg = new List<CardInfoCfg>();                                //抽卡配置
    public List<MsgInfoCfg> MsgInfoCfg = new List<MsgInfoCfg>();                                   //信息配置
    //读取游戏配置表
    public void InitGameCfg()  //初始化自定义的游戏配置表
    {
        //故意未Try 这部分加载 如果配置表异常会直接关闭程序
        string txt = FileTool.Read_Txt("MonsterInfoCfg");
        MonsterInfoCfg = JsonMapper.ToObject<List<MonsterInfoCfg>>(txt);
        txt = FileTool.Read_Txt("LevelInfoCfg");
        LevelInfoCfg = JsonMapper.ToObject<List<LevelInfoCfg>>(txt);
        txt = FileTool.Read_Txt("SkillInfoCfg");
        SkillInfoCfg = JsonMapper.ToObject<List<SkillInfoCfg>>(txt);
        txt = FileTool.Read_Txt("CardInfoCfg");
        CardInfoCfg = JsonMapper.ToObject<List<CardInfoCfg>>(txt);
        txt = FileTool.Read_Txt("MsgInfoCfg");
        MsgInfoCfg = JsonMapper.ToObject<List<MsgInfoCfg>>(txt);
        return;
    }

    public MonsterInfoCfg GetMonsterInfoCfgCfgByKey(int key)
    {
        return MonsterInfoCfg.Find((item) => item.ID == key);
    }
    public LevelInfoCfg GetLevelInfoCfgCfgByKey(int key)
    {
        return LevelInfoCfg.Find((item) => item.ID == key);
    }
    public MsgInfoCfg GetMsgInfoCfgByKey(int key)
    {
        return MsgInfoCfg.Find((item) => item.ID == key);
    }
    public SkillInfoCfg GetSkillInfoCfgCfgByKey(int key)
    {
        return SkillInfoCfg.Find((item) => item.ID == key);
    }
    public CardInfoCfg GetCardInfoCfgCfgByKey(int key)
    {
        return CardInfoCfg.Find((item) => item.ID == key);
    }
    //获取该关卡所有怪物数据配置
    public void GetLevelInfoCfgCfgByLevelId(int levelId, out List<LevelInfoCfg> _levelInfoCfgs)
    {
        _levelInfoCfgs = new List<LevelInfoCfg>();
        foreach (var item in LevelInfoCfg)
        {
            item.isAdd = false;
            if (item.LevelId == levelId)
            {
                _levelInfoCfgs.Add(item);
            }
        }
    }
    //获取关卡倒计时
    public int GetLevelInfoCfgTimeByLevelId(int levelId)
    {
        foreach (var item in LevelInfoCfg)
        {
            if (item.LevelId == levelId)
            {
                return item.LevelTime;
            }
        }
        return 0;
    }
}