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
}
