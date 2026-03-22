using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats
{
    [Header("核心属性")]
    public int baseType = 1;          // 基础类型100英雄200武器300怪物
    public int level = 1;             // 角色当前等级
    public int qualityLevel = 1;          // 品质等级
    public int maxHealth = 10;        // 最大生命值
    public int health = 10;           //当前生命
    public float healthRegen = 1f;    // 生命恢复速率（每秒恢复量）


    [Header("战斗属性")]
    public int baseDamage = 1;    //基础伤害加成
    public int meleeDamage = 1;   // 近战伤害值
    public int rangedDamage = 1;  // 远程伤害值
    public int elementalDamage = 0; // 元素伤害值（可为负值）
    public int critRate = 0;    // 暴击率（0-1表示0%-100%）
    public int critMultiplier = 150;   //爆伤  
    public float attackSpeed = 1;    // 攻击速度（每秒攻击次数 基础值为1）
    public int armor = 0;          // 护甲值（减少受到的伤害）
    public int dodgeChance = 0;  // 闪避概率（0-1表示0%-100%）

    [Header("特殊属性")]
    public int lifeSteal = 0;      // 生命窃取百分比（0-1表示0%-100%）
    public int moveSpeed = 1;      // 移动速度（单位：米/秒）
    public int luck = 0;           // 幸运值（影响掉落率等）
    public int harvest = 0;        // 收获值（影响资源收集效率）
    public int engineering = 0;    // 工程学值（影响特殊能力）
    public int range = 0;          // 攻击范围（基础值为1）
}

public enum DamageType 
{ 
    Melee,  //近战
    Ranged  //远程
}
public enum BaseStats
{
    baseDamage = 1,
    attackSpeed = 2,
    moveSpeed = 3,
    range = 4,
    luck = 5,
    critRate = 6,
    critMultiplier = 7,
    harvest = 8,
}
public class HeroDamage
{
    public int finalDamage;     //普攻伤害值
    public int criticalDamage;  //暴击伤害值
    public int type;       //普攻1暴击2
    public float range;     //射程
    public float stop;       //怪物受击移动停顿时间
    public int skillId;         //技能ID 伤害统计用
}
public enum DropId
{
    skill = 1,          //技能
    citie = 2,          //磁铁
    hp = 3,             //血瓶
}
public enum PrayId
{
    pinZhiTiSheng = 300,            //随机提升品质
    jiNengCao = 301,                //增加技能槽
}
public class WeightedDrop
{
    public DropId id;      //id
    public int weight;      // 权重值，数字越大概率越高
    public string itemName; // 用于调试
}

public class LevelManagement
{
    public int CurrentLevel = 1;                 //当前等级
    public int CurrentEXP = 0;                   //当前经验值
    public int CurrentJb = 0;                    //当前金币
    public int CurrentGkLevel = 0;              //当前关卡
    public int LevelTime = 0;                   //关卡持续时间
    public int MaxLevelTime = 20;               //当前波次持续时间 
    public int ChangeSkillButtonNumber = 0;     //刷新技能次数
    public int SkillBoxNumber = 3;              //技能槽数量

    //获取升级需要的经验值
    public int GetLevelUpExp()
    {
        return 10 + 10* (CurrentLevel-1);
    }
    //增加经验
    public bool AddExp(int _number)
    {
        CurrentEXP += _number;
        AddJb(_number);
        int needExp = GetLevelUpExp();
        if (CurrentEXP >= needExp)
        {
            LevelUp(needExp);
            return true;
        }

        return false;
    }
    //增加金币
    public void AddJb(int _number)
    {
        CurrentJb += _number;
        BattleManager.GetInstance().UpdateJbUi();
        return;
    }
    //升级
    public void LevelUp(int _needNumber)
    {
        CurrentLevel++;
        CurrentEXP -= _needNumber;
    }
    //设置持续时间
    public void SetMaxLevelTime(int _number)
    {
        MaxLevelTime = _number;
    }
    public void Reset()
    {
        CurrentLevel = 1;
        CurrentEXP = 0;
        CurrentGkLevel = 0;
        LevelTime = 0;
        ChangeSkillButtonNumber = 0;
        CurrentJb = 0;
    }
    //开始下一波
    public void BeginNextLevel()
    {
        LevelTime = 0;
        CurrentGkLevel++;
        MaxLevelTime = 20;
        ChangeSkillButtonNumber = 0;
    }
    //获取刷新技能次数需要的金币
    public int GetNeedCurrentJbByNumber()
    {
        //return 10 + ChangeSkillButtonNumber * 10;
        return 0;
    }
    //刷新技能次数
    public void SetChangeSkillButtonNumber()
    {
        ChangeSkillButtonNumber++;
    }
    //刷新技能
    public bool CheckChangeSkill()
    {
        int needNumber = GetNeedCurrentJbByNumber();
        if (CurrentJb >= needNumber)
        {
            CurrentJb -= needNumber;
            return true;
        }
        return false;
    }
    //检查金币
    public bool CheckJb(int _number)
    {
        if (CurrentJb >= _number)
        {
            return true;
        }
        return false;
    }
    //增加技能槽数量
    public void AddSkillBoxNumber(int _number)
    {
        SkillBoxNumber += _number;
    }
    //获取技能数量
    public int GetSkillBoxNumber()
    {
        return SkillBoxNumber;
    }
    //获取升级技能槽需要的金币
    public int GetAddSkillBoxNeedJb()
    {
        int iRet = 100;
        if (SkillBoxNumber == 5)
        {
            iRet = 100;
        }
        else if (SkillBoxNumber == 6)
        {
            iRet = 200;
        }
        else
        {
            iRet = 300;
        }
        return iRet;
    }
}
