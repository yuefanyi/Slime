using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 游戏内语言文字
/// </summary>
public class LanguageCfg
{
    public string key="";                             //索引值
    public string content = "";                       //文字内容
}

/// <summary>
/// 任务配置表
/// </summary>
public class TaskInfoCfg
{
    public int ID;						    //ID
    public string system;					//所属系统
    public string describe;				    //描述
    public long targetNumber;			    //目标数
    public int prepositionTaskID;		    //前置任务ID
    public string award;					//奖励
    public int type;					    //类型（1日常2周长3成长4线索）
    public int open;					    //开关（0关闭1开启）
    public int jump;					    //跳转目的（0没有跳转）
    public int iocn;					    //图标
    public int gameID;					    //对应的游戏id
    public int rank;					    //排序
    public int frame;					    //外框
    public int numberType;				    //任务累计类型（1限制类2累积类型）
    public int taskType;				    //任务类型客户端分类用
    public string awardVip;					//战令奖励
    public int needUnlock;                  //是否需要解锁（0不需要1需要）
    public int vitality;                    //活跃度展示
}
/// <summary>
/// 怪物配置表
/// </summary>
public class MonsterInfoCfg
{
    public int ID;                          // 怪物唯一ID
    public string name;                     // 怪物名称（可选）
    public int level;                       // 等级
    public int hp;                          // 血量
    public int attack;                      // 攻击力
    public int attackType;                  // 攻击类型（1近战/2远程/3魔法等）
    public string modelName;                // 模型名称（Prefab路径）
    public float moveSpeed;                 // 移动速度（建议补充）
    public int expReward;                   // 击杀经验奖励
    public int goldReward;                  // 击杀金币奖励
    public string dropItems;                // 掉落物品ID列表（用分号分隔，如 "1001;1002"）
    public int spawnArea;                   // 刷新区域ID
    public string skills;                   // 技能ID列表（如 "101;202"）
    public string description;              // 怪物描述（可选）
    public int jbNumber;                    // 掉落金币数量
    public int type;                        // 怪物类型1小怪2精英3Boss
}
/// <summary>
/// 关卡配置表
/// </summary>
public class LevelInfoCfg
{
    public int ID;                          //自增id
    public int LevelId;                     //关卡id
    public int AddMonsterTime;              //刷新怪物时间点
    public int LevelTime;                   //关卡时间
    public string MonsterInfo;              //怪物数据（id:数量;id:数量）
    public bool isAdd = false;              //该波次是否已经刷出
}
/// <summary>
/// 技能配置表
/// </summary>
public class SkillInfoCfg
{
    public int ID;                          //自增id
    public int skillId;                     //技能id
    public int type;                        //类型（1武器2技能）
    public int level;                       //等级
    public int baseDamage;                //远程伤害值
    public float attackSpeed;               //攻击速度
    public int range;                       //攻击范围
    public int attackType;                  //攻击类型（1定时触发2定时触发协程）
    public string prefbPath;                //预制体路径
}
/// <summary>
/// 抽卡配置表
/// </summary>
public class CardInfoCfg
{
    public int ID;                          //自增id
    public int skillId;                     //技能id
    public int skillAddId;                  //对应技能表的唯一id
    public int type;                        //类型（1基础属性2技能）
    public int level;                       //等级
    public float number;                    //增加值
    public string name;                     //名称
    public string msg;                      //描述
    public int needJb;                      //购买需要的金币数量
}
/// <summary>
/// 信息配置表
/// </summary>
public class MsgInfoCfg
{
    public int ID;                          //自增id
    public string msg;                      //msg
}

