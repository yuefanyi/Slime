using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiButton : MonoBehaviour
{
    public void BeginBattle()
    {
        GameManager.instance.BeginBattle();
    }
    public void EndPlayerChoiceCardUi()
    {
        BattleManager.GetInstance().EndPlayerChoiceCard();
    }

    public void ChangePlayerChoiceCardUi()
    {
        BattleManager.GetInstance().ChangePlayerChoiceCard();
    }

    public void BuySkillBox()
    {
        BattleManager.GetInstance().BuySkillBox();
    }

    public void SetLevelButton(string str)
    {
        // 尝试将字符串转换为整数
        if (!int.TryParse(str, out int level))
        {
            return; // 转换失败，直接返回
        }

        // 判断是否在 1 到 10 范围内
        if (level < 2 || level > 10)
        {
            return; // 超出范围，直接返回
        }
        level -= 1;
        // 如果需要在转换成功且范围内时执行其他逻辑，请在此处添加
        // 例如：加载对应关卡等
        BattleManager.GetInstance().SetLevel(level);
    }
}
