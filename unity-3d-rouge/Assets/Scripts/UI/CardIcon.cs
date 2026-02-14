using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardIcon : MonoBehaviour
{
    // Start is called before the first frame update
    public int idIndex = 0;
    public void InitIndex(int _index)
    {
        idIndex = _index;
    }
    public void UpLevelSkill()
    {
        BattleManager.GetInstance().WeaponUpLevelButton(idIndex);
    }
    public void WeaponSell()
    {
        BattleManager.GetInstance().WeaponSell(idIndex);
    }
}
