using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gm : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 按键O刷怪
        if (Input.GetKeyDown(KeyCode.I))
        {
            GameManager.instance.TestButton(0);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            GameManager.instance.TestButton2();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            GameManager.instance.TestButton3();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            BattleManager.GetInstance().AddPlayerExp(100);
        }

        // 按键空格放技能
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameManager.instance.isSpace = true;
        }
    }
}
