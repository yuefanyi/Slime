using Cinemachine;
using DamageNumbersPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


public class BattleManager : BaseManager<BattleManager>
{
    private const int MAX_SKILL_LEVEL = 20;
    private const int MAX_SKILL_PINZHI_LEVEL = 5;

    public ConfigManager configMag;


    private int TimeNumber = 0;
    private List<LevelInfoCfg> levelInfoCfgs = new List<LevelInfoCfg>();
    private GameObject MonsterObj;      //怪物根节点
    private GameObject HeroObj;         //英雄obj
    public CharacterStats HeroStats;   //英雄基础属性
    private List<GameObject> WeaponObjList = new List<GameObject>();       //武器obj
    private List<GameObject> SkillUiObjList = new List<GameObject>();      //技能ui
    public GameObject JingYanObj;      //经验跟obj
    public GameObject TeXiaoObj;       //特效根节点
    public GameObject DaoJuObj;        //道具根节点
    public GameObject CanvasHpObj;     //血条canvas节点
    private float JingYanTagY;    //经验生成得Y轴坐标
    private GameObject battleUiObj;     //战斗界面UI根节点
    private Text jbText;                //金币Text
    private CinemachineVirtualCamera virtualCamera;
    private List<GameObject> monsterList = new List<GameObject>();      //怪物obj数组
    private List<GameObject> jingYanList = new List<GameObject>();      //经验obj数组
    private Dictionary<int, int> damageDoneDic = new Dictionary<int, int>();
    private List<WeightedDrop> weightedDrops = new List<WeightedDrop>();//精英怪掉落列表

    public LevelManagement levelManagement = new LevelManagement();
    private float XiTieTime = 0f;

    private bool isBattle = false;


    public BattleManager()
    {
        InitBattleManager();
    }

    //0.1S进入一次
    public void CheckTime()
    {
        TimeNumber++;

        GameManager.instance.StartCoroutine(DetectExperienceStones());

        if (TimeNumber % 10 == 0)
        {
            if (isBattle)
            {
                levelManagement.LevelTime++;
            }
            XiTieTime--;
            CheckAddMonster();
            UpdateCountDown();
            CheckXiTieTime();
            CheckLevelCountDown();
        }
        if (TimeNumber % 50 == 0)
        {
            LogDamageDone();
        }
    }
    //初始化数据
    public void InitBattleManager()
    {
        configMag = GameManager.instance.configMag;
        MonsterObj = GameObject.Find("Map/Monster").gameObject;
        virtualCamera = GameObject.Find("Map/Virtual Camera").GetComponent<CinemachineVirtualCamera>();
        TeXiaoObj = GameObject.Find("Map/TeXiao").gameObject;
        JingYanObj = GameObject.Find("Map/JingYan").gameObject;
        JingYanTagY = GameObject.Find("Map/Map/MapTag").transform.position.y;
        DaoJuObj = GameObject.Find("Map/DaoJu").gameObject;
        CanvasHpObj = GameObject.Find("CanvasHp").gameObject;
        InitAwardJingYing();
    }
    //初始化精英怪掉落列表
    public void InitAwardJingYing()
    {
        weightedDrops.Clear();

        WeightedDrop weightedDrop1 = new WeightedDrop();
        weightedDrop1.id = DropId.skill;
        weightedDrop1.weight = 10000;
        weightedDrops.Add(weightedDrop1);

        WeightedDrop weightedDrop2 = new WeightedDrop();
        weightedDrop2.id = DropId.citie;
        weightedDrop2.weight = 10000;
        weightedDrops.Add(weightedDrop2);
    }
    //开始游戏
    public void BeginGame()
    {
        GameManager.instance.SetColorCurvesActive(false);
        //关闭开始界面
        UiManager.instance.CloseUiByName("Panel_Begin");
        //加载英雄
        AddHero();
        //开启战斗UI界面
        battleUiObj = UiManager.instance.CreatUi("Panel_Battle");
        jbText = battleUiObj.transform.Find("jb/Text_Number").GetComponent<Text>();
        //GotoStore();

        //加载武器
        AddWeapon(1);

        BeginBattle();
    }
    //开始战役
    public void BeginBattle()
    {
        isBattle = true;

        levelManagement.BeginNextLevel();
        //读取关卡配置
        levelInfoCfgs.Clear();
        configMag.GetLevelInfoCfgCfgByLevelId(levelManagement.CurrentGkLevel, out levelInfoCfgs);
        levelManagement.SetMaxLevelTime(configMag.GetLevelInfoCfgTimeByLevelId(levelManagement.CurrentGkLevel));
        
        //刷新战斗UI
        UpdateBattleUi();
        UpdateCountDown();
        //移动英雄
        MovePlayer(GetBattlePlayerVec());
        //关闭商城传送门
        ChuanSongMenContry(1,false);
    }
    //开始下一波
    public void BeginNewLevel()
    {
        levelManagement.BeginNextLevel();
        //读取关卡配置
        levelInfoCfgs.Clear();
        configMag.GetLevelInfoCfgCfgByLevelId(levelManagement.CurrentGkLevel, out levelInfoCfgs);
        levelManagement.SetMaxLevelTime(configMag.GetLevelInfoCfgTimeByLevelId(levelManagement.CurrentGkLevel));
        UpdateCountDown();
    }
    //结束游戏
    public IEnumerator EndGame(Vector3 vector3)
    {
        //画面黑白
        GameManager.instance.SetColorCurvesActive(true);
        //创建死亡特效
        var obj = GameManager.instance.AddPrefab("Weapon/TongYong/PlayerDeadTx", BattleManager.GetInstance().TeXiaoObj.transform, vector3, Quaternion.identity);
        //销毁技能
        foreach (var item in WeaponObjList)
        {
            UnityEngine.Object.Destroy(item);
        }
        WeaponObjList.Clear();

        yield return new WaitForSeconds(5f);
        // 1. 停止所有协程
        GameManager.instance.StopAllCoroutines();
        // 4. 销毁英雄和武器
        if (HeroObj != null)
        {
            HeroObj = null;
        }
        // 2. 清空并销毁所有怪物
        monsterList.Clear();
        // 3. 清空并销毁所有经验石
        jingYanList.Clear();
        //清空所有技能
        WeaponObjList.Clear();
        damageDoneDic.Clear();
        SkillUiObjList.Clear();
        //4. 清空对象池
        ObjPool.instance.ClearPoolCompletely();
        //清空UI
        UiManager.instance.ClearAll();
        // 5. 重置关卡数据
        TimeNumber = 0;
        currentIndex = 0;
        levelInfoCfgs.Clear();

        // 6. 关闭战斗UI
        if (battleUiObj != null)
        {
            //UiManager.instance.CloseUi(battleUiObj);
            battleUiObj = null;
        }

        // 7. 显示结束游戏UI
        UiManager.instance.CreatUi("Panel_Begin");

        // 8. 重置摄像机
        if (virtualCamera != null)
        {
            virtualCamera.Follow = null;
            virtualCamera.LookAt = null;
        }

        // 9. 重置经验管理
        levelManagement.Reset();

        // 10. 恢复游戏时间（如果之前暂停了）
        GameManager.instance.ResumeGame();
    }
    //定时检测加载怪物
    public void CheckAddMonster()
    {
        foreach (var item in levelInfoCfgs)
        {
            if (item.AddMonsterTime <= levelManagement.LevelTime && item.isAdd == false)
            {
                item.isAdd = true;
                string[] list = item.MonsterInfo.Split(new char[] { ';' });
                for (int i = 0; i < list.Length; i++)
                {
                    string[] listGrade = list[i].Split(new char[] { ':' });
                    if (listGrade.Length != 2)
                    {
                        Debug.LogError("怪物配置异常" + list[i]);
                        return;
                    }
                    int id = int.Parse(listGrade[0]);
                    var cfg = configMag.GetMonsterInfoCfgCfgByKey(id);
                    GameManager.instance.StartCoroutine(AddMonsterIenum(cfg, int.Parse(listGrade[1])));
                }
            }
        }
    }
    //加载怪物
    public void AddMonster(MonsterInfoCfg _monsterInfoCfg)
    {
        GameManager.instance.AddPrefab("Monster/" + _monsterInfoCfg.modelName, MonsterObj.transform);
    }
    public IEnumerator AddMonsterIenum(MonsterInfoCfg _monsterInfoCfg, int _number)
    {
        //分帧创建怪物
        for (int i = 0; i < _number; i++)
        {
            GameManager.instance.StartCoroutine(AddMonsterTxIenum(_monsterInfoCfg));
            yield return null;
        }
    }
    public IEnumerator AddMonsterTxIenum(MonsterInfoCfg _monsterInfoCfg)
    {
        float time = 1.5f;
        //加载怪物出场特效
        var Vector3 = GetMonsterRandPostion();
        var objTx = GameManager.instance.AddPrefab("Weapon/TongYong/MonsterStartTx", TeXiaoObj.transform);
        objTx.transform.position = Vector3;
        GameObject jyTx = null;
        if (_monsterInfoCfg.type == 2)
        {
            jyTx = GameManager.instance.AddPrefab("Weapon/TongYong/MonsterJYStartTx", TeXiaoObj.transform);
            jyTx.transform.position = Vector3;
            time = 3f;
        }
        yield return new WaitForSeconds(time);
        //加载怪物
        GameManager.instance.DestroyPrefab(objTx);
        var obj = GameManager.instance.AddPrefab("Monster/" + _monsterInfoCfg.modelName, MonsterObj.transform);
        obj.GetComponent<MonsterChase>().InitMonsterStats(_monsterInfoCfg);
        obj.transform.position = Vector3;
        monsterList.Add(obj);
        //删除特殊怪物特效
        if (jyTx != null)
        {
            yield return new WaitForSeconds(2f);
            GameManager.instance.DestroyPrefab(jyTx);
        }
    }
    //刪除怪物
    public void RemoveMonsterList(GameObject _obj)
    {
        monsterList.Remove(_obj);
    }
    //获取怪物随机坐标
    public int maxSpawnAttempts = 30;
    public float spawnRadius = 20f;
    public Vector3 GetMonsterRandPostion()
    {
        int maxAttempts = 30;
        float radius = 10f;
        Vector3 center = Vector3.zero;
        float sampleDistance = 2f;
        // 尝试在NavMesh上找到随机点
        for (int i = 0; i < maxAttempts; i++)
        {
            // 生成随机方向
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 randomPoint = center + new Vector3(randomCircle.x, 0, randomCircle.y);

            // 检查点是否在NavMesh上
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, sampleDistance, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        // 如果随机失败，尝试使用中心点作为备选
        if (NavMesh.SamplePosition(center, out NavMeshHit centerHit, sampleDistance * 2, NavMesh.AllAreas))
        {
            return centerHit.position;
        }

        // 最后备选方案：返回中心点并记录警告
        Debug.LogWarning($"无法在NavMesh上找到安全位置，返回中心点: {center}");
        return center;
    }   
    //加载英雄
    public void AddHero()
    {
        if (HeroObj != null) { return; }

        HeroObj = GameManager.instance.AddPrefab("Hero/Hero01", GameObject.Find("Map/Player").transform);

        if (virtualCamera != null)
        {
            virtualCamera.Follow = HeroObj.transform;
            virtualCamera.LookAt = HeroObj.transform; 
        }
        //初始化英雄数据
        InitHeroState();
    }
    //获得商城传送门坐标
    public Vector3 GetStorePlayerVec()
    {
        return GameObject.Find("Map/MapStore/JiTai/PlayerTag").transform.position;
    }
    //获得战斗地图传送门坐标
    public Vector3 GetBattlePlayerVec()
    {
        return GameObject.Find("Map/Map/JiTai/PlayerTag").transform.position;
    }
    //移动英雄坐标
    public void MovePlayer(Vector3 newPosition)
    {
        CharacterController characterController = HeroObj.transform.GetComponent<CharacterController>();
        characterController.enabled = false;
        HeroObj.transform.position = newPosition;
        characterController.enabled = true;
    }
    //初始化英雄数据
    public void InitHeroState()
    {
        CharacterStats heroStats = new CharacterStats()
        {
            baseType = 1,           // 英雄
            level = 5,
            maxHealth = 100,
            health = 100,
            healthRegen = 2.5f,

            baseDamage = 100,
            meleeDamage = 0,
            rangedDamage = 0,
            elementalDamage = 1,
            critRate = 5,         // 15%
            attackSpeed = 100,
            armor = 1,
            dodgeChance = 1,       // 5%
            lifeSteal = 1,         // 5%
            moveSpeed = 200,
            luck = 1,
            harvest = 0,
            engineering = 1,
            range = 100
        };
        HeroStats = heroStats;
        HeroObj.GetComponent<PlayerContry>().SetHeroState(HeroStats);
    }
    //加载武器
    public void AddWeapon(int _id)
    {
        SkillInfoCfg cfg = configMag.GetSkillInfoCfgCfgByKey(_id);

        if (cfg != null)
        {
            //先检查有没得该武器
            GameObject obj = CheckWeaponSkillId(cfg.skillId);
            if (obj != null)
            {
                InitWeaponState(cfg, obj);
                return;
            }

            Transform transform;
            if (cfg.type == 1)
            {
                transform = HeroObj.transform.Find("WeapanTag");
            }
            else
            {
                transform = HeroObj.transform.Find("SkillTag");
            }
            //加载UI
            var uiObj = AddSkillUi(cfg.skillId);
            var WeaponObj = GameManager.instance.AddPrefab(cfg.prefbPath, transform);
            WeaponObjList.Add(WeaponObj);
            InitWeaponState(cfg, WeaponObj, uiObj);
         
        }
        else
        {
            Debug.LogError("加载武器异常" + _id);
        }
    }
    //技能升级（2合成1）
    public void WeaponUpLevel(int _index1, int _index2)
    {
        //先检查下标是否合法
        if (_index1 >= WeaponObjList.Count || _index2 >= WeaponObjList.Count)
        {
            Debug.LogError("合成技能异常" + _index1 + _index2);
            return;
        }
        if (_index1 == _index2)
        {
            Debug.LogError("下标相同" + _index1 + _index2);
            return;
        }

        CharacterStats stats1 = WeaponObjList[_index1].GetComponent<WeaponContry>().GetWeaponStats();
        CharacterStats stats2 = WeaponObjList[_index2].GetComponent<WeaponContry>().GetWeaponStats();
        //检查技能ID是否一致
        if (stats1.baseType != stats2.baseType)
        {
            Debug.LogError("合成id不一致");
            return;
        }
        //检查技能等级
        if (stats1.level != stats2.level || stats1.level >= MAX_SKILL_LEVEL)
        {
            Debug.LogError("技能等级异常");
            return;
        }
        //升级技能
        int id = GetCardWeaponId(stats1.level, stats1.baseType);
        SkillInfoCfg cfg = configMag.GetSkillInfoCfgCfgByKey(id);
        InitWeaponState(cfg, WeaponObjList[_index1], WeaponObjList[_index1].GetComponent<WeaponContry>().skillUiObj);
        //删除第二个技能
        UnityEngine.Object.Destroy(WeaponObjList[_index2]);
        WeaponObjList.Remove(WeaponObjList[_index2]);
        //刷新ui
        var objPanel = UiManager.instance.GetUiByName("Panel_Card");
        if (objPanel != null)
        {
            objPanel.GetComponent<PanelCard>().UpdateCardPanelSkillIcon();
        }
    }
    //合成技能
    public void WeaponUpLevelButton(int _index1)
    {
        if (_index1 >= WeaponObjList.Count)
        {
            Debug.Log("合成技能异常");
            return;
        }
        //先遍历是否有同等级的技能
        int index2 = -1;
        CharacterStats stats1 = WeaponObjList[_index1].GetComponent<WeaponContry>().GetWeaponStats();

        for (int i = 0; i < WeaponObjList.Count; i++)
        {
            if (i == _index1)
            {
                continue;
            }
            CharacterStats stats2 = WeaponObjList[i].GetComponent<WeaponContry>().GetWeaponStats();
            if (stats1.baseType == stats2.baseType && stats1.level == stats2.level)
            {
                index2 = i;
                break;
            }
        }
        if (index2 == -1)
        {
            Debug.Log("没有同等级技能");
            return;
        }
        WeaponUpLevel(_index1, index2);
    }
    //出售技能
    public void WeaponSell(int _index)
    {
        if (_index >= WeaponObjList.Count)
        {
            Debug.Log("售卖技能异常");
            return;
        }
        //获取货币
        int level = WeaponObjList[_index].GetComponent<WeaponContry>().GetWeaponStats().level;
        levelManagement.AddJb(GetSkillSellJb(level));

        UnityEngine.Object.Destroy(WeaponObjList[_index]);
        WeaponObjList.Remove(WeaponObjList[_index]);
        var objPanel = UiManager.instance.GetUiByName("Panel_Card");
        if (objPanel != null)
        {
            objPanel.GetComponent<PanelCard>().UpdateCardPanelSkillIcon();
        }
    }
    //根据等级获取售卖的金币
    public int GetSkillSellJb(int _level)
    {
        return 5 * _level;
    }
    public GameObject AddSkillUi(int _id)
    {
        Transform transform = battleUiObj.transform.Find("JiNeng");
        var obj = GameManager.instance.AddPrefab("UI/JiNengIcon", transform);
        obj.GetComponent<SkillUi>().InitSkillId(_id);
        SkillUiObjList.Add(obj);
        return obj;
    }
    public GameObject CheckWeaponSkillId(int _skillId, int _level)
    {
        foreach (var item in WeaponObjList)
        {
            var stats = item.GetComponent<WeaponContry>().GetWeaponStats();
            if (stats.baseType == _skillId && stats.level == _level)
            {
                return item;
            }
        }
        return null;
    }
    public GameObject CheckWeaponSkillId(int _skillId)
    {
        foreach (var item in WeaponObjList)
        {
            var stats = item.GetComponent<WeaponContry>().GetWeaponStats();
            if (stats.baseType == _skillId)
            {
                return item;
            }
        }
        return null;
    }
    //初始化武器数据
    public void InitWeaponState(SkillInfoCfg _skillInfoCfg, GameObject WeaponObj, GameObject SkillUiObj = null)
    {
        CharacterStats weaponStats = new CharacterStats()
        {
            baseType = 1,           // 英雄
            level = 5,
            maxHealth = 150,
            healthRegen = 2.5f,

            meleeDamage = 1,
            rangedDamage = 1,
            elementalDamage = 1,
            critRate = 1,         // 15%
            attackSpeed = 19f,
            armor = 1,
            dodgeChance = 1,       // 5%

            lifeSteal = 1,         // 5%
            moveSpeed = 2,
            luck = 1,
            harvest = 1,
            engineering = 1,
            range = 800
        };
        weaponStats.baseType = _skillInfoCfg.skillId;
        weaponStats.level = _skillInfoCfg.level;
        weaponStats.baseDamage = _skillInfoCfg.baseDamage;
        weaponStats.attackSpeed = _skillInfoCfg.attackSpeed;
        weaponStats.range = _skillInfoCfg.range;
        WeaponObj.GetComponent<WeaponContry>().SetWeaponState(weaponStats, HeroObj.GetComponent<PlayerContry>().GetHeroState(), _skillInfoCfg.attackType, SkillUiObj);
    }
    //获取英雄obj
    public GameObject GetHeroObj()
    {
        return HeroObj;
    }
    //获取武器obj
    public List<GameObject> GetWeaponObj()
    {
        return WeaponObjList;
    }
    //计算英雄伤害
    public HeroDamage CalculateDamage(CharacterStats heroStats,CharacterStats weaponStats,DamageType damageType)
    {
        HeroDamage heroDamage = new HeroDamage();
        int weaponBaseDamage = weaponStats.baseDamage;
        // 1. 选择伤害类型的基础值
        int heroDamageBonus = damageType == DamageType.Melee
            ? heroStats.meleeDamage
            : heroStats.rangedDamage;

        // 2. 计算武器伤害（包含元素伤害）
        float baseDamageMultiplier = heroStats.baseDamage / 100f;
        int weaponDamage = Mathf.FloorToInt((weaponBaseDamage + heroDamageBonus) * baseDamageMultiplier);

        // 3. 暴击判定
        bool isCrit = Random.Range(0f, 1f) <= (heroStats.critRate / 100f);
        float critMultiplier = heroStats.critMultiplier / 100f;

        // 4. 最终伤害计算
        int finalDamage = Mathf.FloorToInt(weaponDamage * critMultiplier);
        heroDamage.finalDamage = weaponDamage;
        heroDamage.criticalDamage = finalDamage;
        heroDamage.type = isCrit ? 2 : 1;

        //5.计算射程
        heroDamage.range = weaponStats.range /100f * heroStats.range / 100f;
        heroDamage.stop = GetMoveStop(weaponStats.baseType);

        heroDamage.skillId = weaponStats.baseType;
        return heroDamage;
    }
    public float GetMoveStop(int _skillId)
    {
        //if (_skillId == 206)
        //{
        //    return 0.01f;
        //}
        return 0;
    }
    /// <summary>
    /// 获取攻击范围内最近的怪物
    /// </summary>
    /// <param name="myPosition">自身位置</param>
    /// <param name="attackRange">攻击范围</param>
    /// <returns>最近的怪物对象，若无满足条件的怪物则返回null</returns>
    public GameObject GetNearestMonsterInRange(Vector3 myPosition, float attackRange)
    {
        // 验证参数有效性
        if (attackRange <= 0)
        {
            Debug.LogWarning("攻击范围必须大于0！");
            return null;
        }

        GameObject nearestMonster = null;
        float minSqrDistance = Mathf.Infinity;
        float sqrAttackRange = attackRange * attackRange;  // 使用平方比较避免开方运算

        foreach (GameObject monster in monsterList)
        {
            if (monster == null || !monster.activeInHierarchy)
                continue;

            Vector3 toMonster = monster.transform.position - myPosition;
            float sqrDistance = toMonster.sqrMagnitude;

            // 检查是否在攻击范围内且距离更近
            if (sqrDistance <= sqrAttackRange && sqrDistance < minSqrDistance)
            {
                minSqrDistance = sqrDistance;
                nearestMonster = monster;
            }
        }

        return nearestMonster;
    }
    // <summary>
    /// 获取攻击范围内的随机怪物
    /// </summary>
    /// <param name="myPosition">自身位置</param>
    /// <param name="attackRange">攻击范围</param>
    /// <returns>随机怪物对象，若无满足条件的怪物则返回null</returns>
    public GameObject GetRandomMonsterInRange(Vector3 myPosition, float attackRange)
    {
        // 验证参数有效性
        if (attackRange <= 0)
        {
            Debug.LogWarning("攻击范围必须大于0！");
            return null;
        }

        List<GameObject> validMonsters = new List<GameObject>();
        float sqrAttackRange = attackRange * attackRange;  // 平方距离比较优化性能

        foreach (GameObject monster in monsterList)
        {
            // 跳过无效对象
            if (monster == null || !monster.activeInHierarchy)
                continue;

            // 计算平方距离
            float sqrDistance = (monster.transform.position - myPosition).sqrMagnitude;

            // 在攻击范围内则加入候选列表
            if (sqrDistance <= sqrAttackRange)
            {
                validMonsters.Add(monster);
            }
        }

        // 随机选择符合条件的怪物
        if (validMonsters.Count > 0)
        {
            int randomIndex = Random.Range(0, validMonsters.Count);
            return validMonsters[randomIndex];
        }

        return null;  // 无符合条件的怪物
    }
    //生成经验
    public void AddJingYan(Vector3 _vec3, int _addNumber)
    {
        Vector3 vector3 = new Vector3(_vec3.x, JingYanTagY, _vec3.z);
        float randomYRotation = Random.Range(0f, 360f); // Y轴随机旋转
        Quaternion randomRotation = Quaternion.Euler(0f, randomYRotation, 0f); // 可以调整X/Y/Z
        GameObject obj = GameManager.instance.AddPrefab("DaoJu/jingyan1", BattleManager.GetInstance().JingYanObj.transform, vector3, randomRotation);
        obj.GetComponent<JingYanContry>().InitJingYan(_addNumber);
        jingYanList.Add(obj);
        //计算额外掉落经验
        int randNumber = Random.Range(1, 101);
        if (randNumber <= HeroStats.harvest)
        {
            float randFloatx = Util.randomFloat(-0.1f, 0.1f);
            float randFloaty = Util.randomFloat(-0.1f, 0.1f);
            vector3 = new Vector3(_vec3.x+ randFloatx, JingYanTagY, _vec3.z+ randFloaty);
            randomYRotation = Random.Range(0f, 360f); // Y轴随机旋转
            randomRotation = Quaternion.Euler(0f, randomYRotation, 0f); // 可以调整X/Y/Z
            obj = GameManager.instance.AddPrefab("DaoJu/jingyan2", BattleManager.GetInstance().JingYanObj.transform, vector3, randomRotation);
            obj.GetComponent<JingYanContry>().InitJingYan(_addNumber);
            jingYanList.Add(obj);
        }
    }
    //精英怪掉落
    public void AddJingYingAward(Vector3 vector3)
    {
        //DropId id = GetRandomItemByWeight();
        ////生成奖励
        //AddAward(id, vector3);
    }
    //随机掉落奖励
    private DropId GetRandomItemByWeight()
    {
        if (weightedDrops.Count == 0) return DropId.skill;

        // 计算总权重
        int totalWeight = 0;
        foreach (var drop in weightedDrops)
        {
            totalWeight += drop.weight;
        }

        // 生成随机数
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        // 根据权重选择物品
        foreach (var drop in weightedDrops)
        {
            currentWeight += drop.weight;
            if (randomValue < currentWeight)
            {
                return drop.id;
            }
        }

        return weightedDrops[0].id; // 保底返回第一个
    }
    //生成奖励
    private void AddAward(DropId id, Vector3 vector3)
    {
        if (id == DropId.skill)
        {
            var list = BattleManager.GetInstance().DrawCards(1, BattleManager.GetInstance().HeroStats.luck, 2);
            var obj = GameManager.instance.AddPrefab("DaoJu/Box", DaoJuObj.transform, vector3, Quaternion.identity);
            CardInfoCfg cfg = BattleManager.GetInstance().configMag.GetCardInfoCfgCfgByKey(list[0]);
            obj.GetComponent<DaoJu>().InitCardInfoCfg(cfg);
            obj.GetComponent<DaoJu>().ChangeMaterialWithTexture();
        }
        else if (id == DropId.citie)
        {
            var obj = GameManager.instance.AddPrefab("DaoJu/XiTie", DaoJuObj.transform, vector3, Quaternion.identity);
        }
    }
    //刷新战斗UI
    public void UpdateBattleUi()
    {
        if (battleUiObj == null || HeroStats == null)
        {
            return;
        }

        battleUiObj.transform.Find("Hp/Image").GetComponent<Image>().fillAmount = (float)HeroStats.health / (float)HeroStats.maxHealth;
        battleUiObj.transform.Find("Hp/Text").GetComponent<Text>().text = HeroStats.health.ToString();
        battleUiObj.transform.Find("JingYan/Image").GetComponent<Image>().fillAmount = (float)levelManagement.CurrentEXP / (float)levelManagement.GetLevelUpExp();
        battleUiObj.transform.Find("Text_Level").GetComponent<Text>().text = "Level:"+levelManagement.CurrentGkLevel;
        UpdateJbUi();
    }
    //刷新金币
    public void UpdateJbUi()
    {
        if (jbText != null)
        {
            jbText.text = ":"+levelManagement.CurrentJb.ToString();
        }
        GameObject obj = UiManager.instance.GetUiByName("Panel_Card");
        if (obj != null)
        {
            obj.transform.Find("Cards/TextNumber").GetComponent<Text>().text = ":" + levelManagement.CurrentJb.ToString();
        }
    }
    //刷新倒计时
    public void UpdateCountDown()
    {
        int number = levelManagement.MaxLevelTime - levelManagement.LevelTime;
        if (number <= 0)
        {
            number = 0;
        }
        string time = DateTimeUtil.secondToMMSS(number);
        if (battleUiObj != null)
        {
            battleUiObj.transform.Find("Text_CountDown").GetComponent<Text>().text = time+"s";
        }
    }
    //检查波次时间结束
    public void CheckLevelCountDown()
    {
        if (levelManagement.LevelTime < levelManagement.MaxLevelTime)
        {
            return;
        }
        if (isBattle == false)
        {
            return;
        }
        //波次结束
        //打开购买界面
        //PlayerChoiceCardPanel();
        LevelEnd();
    }
    //波次结束
    public void LevelEnd()
    {
        isBattle = false;

        DesMonsterAll();
        //打开战斗界面传送门
        ChuanSongMenContry(2,true);
        //吸铁石效果
        SetSqrCollectRadius();
    }
    //去商店
    public void GotoStore()
    {
        //关闭所有传送门
        ChuanSongMenContry(1, false);
        ChuanSongMenContry(2, false);
        //触发商店奖励
        StoreAward();
        MovePlayer(GetStorePlayerVec());
    }
    // 分帧检测拾取经验协程
    public int maxChecksPerFrame = 200;          // 每帧检测数量
    private int currentIndex;                   // 当前检测索引
    private Transform playerTransform;          // 玩家位置
    private float sqrCollectRadius = 4;         //收集半径的平方值（优化计算）
    private float oldCollectRadius = 4;
    public float flySpeed = 15f;                // 经验飞向玩家的速度
    IEnumerator DetectExperienceStones()
    {
        if (jingYanList.Count == 0) yield break;

        playerTransform = HeroObj.transform;

        int processed = 0;
        int validCount = 0;

        // 分帧处理经验石
        while (processed < maxChecksPerFrame && validCount < jingYanList.Count)
        {
            // 获取当前检测的经验石
            GameObject stone = jingYanList[currentIndex];
            currentIndex = (currentIndex + 1) % jingYanList.Count;
            validCount++;

            // 跳过无效对象
            if (stone == null || !stone.activeSelf) continue;

            processed++;

            // 检查距离
            if (IsWithinCollectRange(stone.transform.position))
            {
                // 开始收集
                GameManager.instance.StartCoroutine(CollectStone(stone));
            }

            // 每处理几个石头就等待一帧
            if (processed > 0 && processed % 5 == 0)
            {
                yield return null;
            }
        }
    }
    // 检查是否在收集范围内（使用平方距离优化）
    bool IsWithinCollectRange(Vector3 stonePosition)
    {
        Vector3 offset = stonePosition - playerTransform.position;
        return offset.sqrMagnitude <= sqrCollectRadius;
    }
    // 收集经验石协程
    IEnumerator CollectStone(GameObject stone)
    {
        RemoveExperienceStone(stone);
        // 短暂延迟使收集更自然
        yield return new WaitForSeconds(0.1f);

        // 平滑移动到玩家位置
        while (stone != null && stone.activeSelf &&
               Vector3.Distance(stone.transform.position, playerTransform.position) > 0.1f)
        {
            stone.transform.position = Vector3.MoveTowards(
                stone.transform.position,
                playerTransform.position,
                flySpeed * Time.deltaTime
            );
            yield return null;
        }

        // 收集完成
        if (stone != null)
        {
            // 这里添加经验值获取逻辑
            // 例如：PlayerStats.AddExperience(expValue);
            AddPlayerExp(stone.GetComponent<JingYanContry>().addNumber);
            // 回收或销毁经验石
            ReturnToPool(stone);
        }
    }
    // 从列表中移除经验石
    public void RemoveExperienceStone(GameObject stone)
    {
        if (jingYanList.Contains(stone))
        {
            jingYanList.Remove(stone);

            // 调整索引防止越界
            if (currentIndex >= jingYanList.Count)
            {
                currentIndex = 0;
            }
        }
    }
    // 回收经验石（使用对象池）
    void ReturnToPool(GameObject stone)
    {
        GameManager.instance.DestroyPrefab(stone);
    }
    //增加经验值
    public void AddPlayerExp(int _number)
    {
        if (levelManagement.AddExp(_number))
        {
            //升级
            PlayerLevelUp();
        }
        //刷新UI界面
        UpdateBattleUi();
    }
    //吸铁石功能
    public void SetSqrCollectRadius()
    {
        XiTieTime = 5f;
        oldCollectRadius = sqrCollectRadius;
        sqrCollectRadius = 1024;
    }
    public void CheckXiTieTime()
    {
        if (XiTieTime <= 0)
        {
            sqrCollectRadius = oldCollectRadius;
        }
    }
    //抽卡算法
    /// <summary>
    /// 抽卡函数（返回List<int>，且skillId唯一）
    /// </summary>
    /// <param name="count">抽取数量</param>
    /// <param name="luck">幸运值（0-100）</param>
    /// <param name="type">卡牌类型（1武器/2技能）</param>
    /// <returns>抽中的卡牌ID列表（skillId唯一）</returns>
    public List<int> DrawCards(int count, float luck, int type)
    {
        // 1. 过滤符合类型的卡牌
        List<CardInfoCfg> eligibleCards;
        if (type == 0)
        {
            eligibleCards = configMag.CardInfoCfg.ToList();
        }
        else
        {
            eligibleCards = configMag.CardInfoCfg.Where(c => c.type == type).ToList();
        }
        
        if (eligibleCards.Count == 0)
            return new List<int>();

        List<int> drawnCardIds = new List<int>();
        HashSet<int> drawnSkillIds = new HashSet<int>(); // 记录已抽取的skillId

        // 创建当前可抽取卡池的副本（避免修改原始数据）
        var availableCards = new List<CardInfoCfg>(eligibleCards);

        for (int i = 0; i < count; i++)
        {
            // 如果可抽取卡池为空，提前结束
            if (availableCards.Count == 0)
                break;

            // 2. 计算每张卡的权重（基础权重 + 幸运加成）
            float totalWeight = 0;
            var cardWeights = new Dictionary<CardInfoCfg, float>();

            foreach (var card in availableCards)
            {
                // 基础权重：等级越高权重越低（便于后续幸运值反转）
                float baseWeight = 1f / card.level;

                // 幸运加成：幸运值越高，高等级卡权重加成越大
                float luckBonus = luck * card.level * 0.01f;

                // 高级卡概率调整：通过因子降低高等级卡的权重
                float levelAdjustment = 1.0f;
                if (card.level == 1) // 假设1是最低级，大于1都是高级卡
                {
                    // highLevelProbabilityFactor 越小，高级卡概率越低
                    levelAdjustment = 1.0f;
                }
                else if (card.level == 2)
                {
                    levelAdjustment = 0.5f;
                }
                else if (card.level == 3)
                {
                    levelAdjustment = 0.1f;
                }
                else if (card.level == 4)
                {
                    levelAdjustment = 0.05f;
                }
                else if (card.level == 5)
                {
                    levelAdjustment = 0.01f;
                }

                // 最终权重 = (基础权重 + 幸运加成) × 等级调整因子
                float finalWeight = (baseWeight + luckBonus) * levelAdjustment;

                cardWeights.Add(card, finalWeight);
                totalWeight += finalWeight;
            }

            // 3. 加权随机选择
            float randomPoint = UnityEngine.Random.Range(0, totalWeight);
            float currentWeight = 0;
            CardInfoCfg selectedCard = null;

            foreach (var pair in cardWeights)
            {
                currentWeight += pair.Value;
                if (currentWeight >= randomPoint)
                {
                    selectedCard = pair.Key;
                    break;
                }
            }

            if (selectedCard != null)
            {
                // 4. 添加选中的卡牌
                drawnCardIds.Add(selectedCard.ID);
                drawnSkillIds.Add(selectedCard.skillId);

                // 5. 从可抽取池中移除所有相同skillId的卡牌
                availableCards.RemoveAll(card => card.skillId == selectedCard.skillId);
            }
        }

        return drawnCardIds;
    }
    //抽卡界面
    public void PlayerChoiceCardPanel()
    {
        DesMonsterAll();
        //暂停游戏
        GameManager.instance.PauseGame();
        //创建抽卡界面
        var obj = UiManager.instance.CreatUi("Panel_Card");
        obj.GetComponent<PanelCard>().InitPanel();

    }
    //玩家升级
    public void PlayerLevelUp(bool isPray = false)
    {
        //暂停游戏
        GameManager.instance.PauseGame();
        //创建抽卡界面
        var obj = UiManager.instance.CreatUi("Panel_Card");
        obj.GetComponent<PanelCard>().InitPanel(isPray);
    }

    //选择卡牌
    public void PlayerChoiceCard(CardInfoCfg _cardInfoCfg, GameObject _gameObject)
    {
        //先检查货币是否足够
        //if (levelManagement.CheckJb(_cardInfoCfg.needJb) == false)
        //{
        //    //提示弹窗
        //    GameManager.instance.SetTips(1001);
        //    return;
        //}
        
        if (_cardInfoCfg.type == 1)
        {
            UpdateHeroStats(_cardInfoCfg);
        }
        //else if (_cardInfoCfg.type == 2)
        //{
        //    //检查还有无技能位置
        //    if (WeaponObjList.Count < GetNowSkillNumber())
        //    {
        //        AddWeapon(_cardInfoCfg.skillAddId);
        //    }
        //    else
        //    {
        //        GameObject obj = CheckWeaponSkillId(_cardInfoCfg.skillId, _cardInfoCfg.level);
        //        if (obj != null && _cardInfoCfg.level < MAX_SKILL_LEVEL)
        //        {
        //            //升级技能
        //            int id = GetCardWeaponId(_cardInfoCfg.level, _cardInfoCfg.skillId);
        //            SkillInfoCfg cfg = configMag.GetSkillInfoCfgCfgByKey(id);
        //            InitWeaponState(cfg, obj, obj.GetComponent<WeaponContry>().skillUiObj);
        //        }
        //        else
        //        {
        //            //提示弹窗 
        //            GameManager.instance.SetTips(1002);
        //            return;
        //        }
        //    }
        //}
        else if (_cardInfoCfg.type == 2)
        {
            //检查还有无技能位置
            if (WeaponObjList.Count < GetNowSkillNumber())
            {
                GameObject obj = CheckWeaponSkillId(_cardInfoCfg.skillId);

                AddWeapon(GetCardWeaponId(obj, _cardInfoCfg));
            }
            else
            {
                GameObject obj = CheckWeaponSkillId(_cardInfoCfg.skillId);
                if (obj != null)
                {
                    AddWeapon(GetCardWeaponId(obj, _cardInfoCfg));
                }
                else
                {
                    //提示弹窗 
                    GameManager.instance.SetTips(1002);
                    return;
                }
         
            }
        }
        else if (_cardInfoCfg.type == 3)
        {
            PlayerPray(_cardInfoCfg);
        }

        //levelManagement.AddJb(-_cardInfoCfg.needJb);
        if (_gameObject != null)
        {
            _gameObject.SetActive(false);
        }
        var objPanel = UiManager.instance.GetUiByName("Panel_Card");
        if (objPanel != null)
        {
            objPanel.GetComponent<PanelCard>().UpdateCardPanelSkillIcon();
        }
        EndPlayerChoiceCard();
    }
    //祈愿
    public void PlayerPray(CardInfoCfg _cardInfoCfg)
    {
        if (_cardInfoCfg.skillId == (int)PrayId.pinZhiTiSheng)
        {
            //随机提升技能品质
            AddWeaponPinZhiRand();
        }
        else if (_cardInfoCfg.skillId == (int)PrayId.jiNengCao)
        {
            //购买技能槽
            BuySkillBox();
        }
    }
    //刷新抽卡界面
    public void ChangePlayerChoiceCard()
    {
        GameObject obj = UiManager.instance.GetUiByName("Panel_Card");
        if (obj == null)
        {
            Debug.Log("刷新抽卡界面异常");
            return;
        }
        //检查货币是否足够
        if (levelManagement.CheckChangeSkill())
        {
            levelManagement.SetChangeSkillButtonNumber();
            obj.GetComponent<PanelCard>().UpdateDrawCardPanel();
        }
        else
        {
            GameManager.instance.SetTips(1001);
        }
        UpdateJbUi();
    }
    //结束抽卡界面
    public void EndPlayerChoiceCard()
    {
        UiManager.instance.CloseUiByName("Panel_Card");
        GameManager.instance.ResumeGame();
        //BeginNewLevel();
    }
    //销毁未死的怪物
    public void DesMonsterAll()
    {
        foreach (var item in monsterList)
        {
            item.GetComponent<MonsterChase>().DesHpUiObj();
            GameManager.instance.DestroyPrefab(item);
        }
        monsterList.Clear();
        //foreach (var item in jingYanList)
        //{
        //    GameManager.instance.DestroyPrefab(item);
        //}
        //jingYanList.Clear();
        //currentIndex = 0;
    }

    //获取正确的技能ID有技能获取下一级技能没有技能获取一级技能
    public int GetCardWeaponId(GameObject _obj, CardInfoCfg _cardInfoCfg)
    {
        int level = _cardInfoCfg.level;
        int id = 0;
        if (_obj != null)
        {
            level = _obj.GetComponent<WeaponContry>().GetWeaponStats().level;
            level += _cardInfoCfg.level;
            if (level >= MAX_SKILL_LEVEL)
            {
                level = MAX_SKILL_LEVEL;
            }
        }
        foreach (var item in configMag.SkillInfoCfg)
        {
            if (item.skillId == _cardInfoCfg.skillId && level == item.level)
            {
                id = item.ID;
                return id;
            }
        }
        return id;
    }
    public int GetCardWeaponId(int _level, int _skillId)
    {
        int id = 0;
        int level = _level;
        level++;
        if (level >= MAX_SKILL_LEVEL)
        {
            level = MAX_SKILL_LEVEL;
        }
        foreach (var item in configMag.SkillInfoCfg)
        {
            if (item.skillId == _skillId && level == item.level)
            {
                id = item.ID;
                return id;
            }
        }
        return id;
    }
    //更新英雄数据
    public void UpdateHeroStats(CardInfoCfg _cardInfoCfg)
    {
        BaseStats BaseStats = (BaseStats)_cardInfoCfg.skillId;
        switch (BaseStats)
        {
            case BaseStats.baseDamage:
                HeroStats.baseDamage += (int)_cardInfoCfg.number;
                break;
            case BaseStats.attackSpeed:
                HeroStats.attackSpeed += (int)_cardInfoCfg.number;
                break;
            case BaseStats.moveSpeed:
                HeroStats.moveSpeed += (int)_cardInfoCfg.number;
                break;
            case BaseStats.range:
                HeroStats.range += (int)_cardInfoCfg.number;
                break;
            case BaseStats.luck:
                HeroStats.luck += (int)_cardInfoCfg.number;
                break;
            case BaseStats.critRate:
                HeroStats.critRate += (int)_cardInfoCfg.number;
                break;
            case BaseStats.critMultiplier:
                HeroStats.critMultiplier += (int)_cardInfoCfg.number;
                break;
            case BaseStats.harvest:
                HeroStats.harvest += (int)_cardInfoCfg.number;
                break;

            default:
                Debug.LogError("更新英雄数据异常_cardInfoCfg.skillId = " + _cardInfoCfg.skillId);
                break;
        }
        foreach (var item in WeaponObjList)
        {
            item.GetComponent<WeaponContry>().UpdateFinalStats();
        }
        HeroObj.GetComponent<PlayerContry>().UpdateMoveSpeed();
    }
    //摄像机抖动
    public void CameraShake()
    {
        
    }
    //伤害统计
    public void DamageDone(HeroDamage  _heroDamage)
    {
        int damage;
        if (_heroDamage.type == 1)
        {
            damage = _heroDamage.finalDamage;
        }
        else
        {
            damage = _heroDamage.criticalDamage;
        }
        if (damageDoneDic.TryGetValue(_heroDamage.skillId, out int currentDamage))
        {
            damageDoneDic[_heroDamage.skillId] = currentDamage + damage;
        }
        else
        {
            damageDoneDic.Add(_heroDamage.skillId, damage);
        }
    }
    //打印伤害
    public void LogDamageDone()
    {
        
        foreach (var item in damageDoneDic)
        {
            Debug.Log(item.Key.ToString()+":"+ item.Value.ToString());
        }
        Debug.Log("----------------------------------------------------");
    }
    //获取当前可装载技能数量
    public int GetNowSkillNumber()
    {
        return levelManagement.GetSkillBoxNumber();
    }
    //获取武器obj list
    public List<GameObject> GetWeaponList()
    {
        return WeaponObjList;
    }
    //购买技能位置
    public void BuySkillBox()
    {
        //检查金币是否足够
        //int needNumber = levelManagement.GetAddSkillBoxNeedJb();
        //if (levelManagement.CheckJb(needNumber) == false)
        //{
        //    //提示弹窗
        //    GameManager.instance.SetTips(1001);
        //    return;
        //}
        ////扣除金币
        //levelManagement.AddJb(-needNumber);
        //增加槽位
        levelManagement.AddSkillBoxNumber(1);
        //刷新界面
        var objPanel = UiManager.instance.GetUiByName("Panel_Card");
        if (objPanel != null)
        {
            objPanel.GetComponent<PanelCard>().UpdateCardPanelSkillIcon();
        }
    }
    //获取武器品质
    public int GetWeaponPinZhi(int _skillId, int _qualityLevel)
    {
        int iRet = _qualityLevel;
        if (_skillId == 203)
        {
            iRet *= 2;
        }
        return iRet;
    }
    //开关传送门
    public void ChuanSongMenContry(int index, bool isBoos)
    {
        DaoJu obj = null;
        if (index == 1)
        {
            obj = GameObject.Find("Map/MapStore/JiTai").gameObject.GetComponent<DaoJu>();
        }
        else
        {
            obj = GameObject.Find("Map/Map/JiTai").gameObject.GetComponent<DaoJu>();
        }
        if (isBoos)
        {
            obj.OpenChuanSong();
        }
        else
        {
            obj.CloseChuanSong();
        }
    }
    //商店奖励
    public void StoreAward()
    {
        //List<int> list;
        //list = BattleManager.GetInstance().DrawCards(2, BattleManager.GetInstance().HeroStats.luck, 2);

        //int number = 2;
        //for (int i = 0; i < number; i++)
        //{
        //    Vector3 vec = GameObject.Find("Map/MapStore/Tags/Tag" + i.ToString()).transform.position;
        //    var obj = GameManager.instance.AddPrefab("DaoJu/Box", DaoJuObj.transform, vec, Quaternion.identity);
        //    CardInfoCfg cfg = BattleManager.GetInstance().configMag.GetCardInfoCfgCfgByKey(list[i]);
        //    obj.GetComponent<DaoJu>().InitCardInfoCfg(cfg);
        //    obj.GetComponent<DaoJu>().ChangeMaterialWithTexture();
        //}
    }
    
    //提升武器品质
    public void AddWeaponPinZhi(int _index)
    {
        if (_index >= WeaponObjList.Count || _index < 0)
        {
            Debug.Log("提升品质异常" + _index);
            return;
        }
        var stats = WeaponObjList[_index].GetComponent<WeaponContry>().GetWeaponStats();
        stats.qualityLevel++;
        if (stats.qualityLevel > MAX_SKILL_PINZHI_LEVEL)
        {
            stats.qualityLevel = MAX_SKILL_PINZHI_LEVEL;
        }
        //刷新界面
        WeaponObjList[_index].GetComponent<WeaponContry>().UpdateFinalStats();
        //刷新卡牌界面
        var objPanel = UiManager.instance.GetUiByName("Panel_Card");
        if (objPanel != null)
        {
            objPanel.GetComponent<PanelCard>().UpdateCardPanelSkillIcon();
        }
    }
    //随机提升武器品质
    public void AddWeaponPinZhiRand()
    {
        // 获取未满级的武器列表
        List<int> availableIndices = new List<int>();

        for (int i = 0; i < WeaponObjList.Count; i++)
        {
            // 假设武器对象有一个Weapon组件，包含IsMaxLevel属性来判断是否满级
            var weapon = WeaponObjList[i].GetComponent<WeaponContry>().GetWeaponStats();
            if (weapon != null && weapon.qualityLevel < MAX_SKILL_PINZHI_LEVEL)
            {
                availableIndices.Add(i);
            }
        }

        // 如果有可用的武器
        if (availableIndices.Count > 0)
        {
            int randNumber = Util.randomInt(0, availableIndices.Count - 1);
            AddWeaponPinZhi(availableIndices[randNumber]);
        }
        else
        {
            // 所有武器都已满级的处理逻辑
            Debug.Log("所有武器都已满级！");
            // 可以选择不添加武器，或者添加其他逻辑
        }
    }
    //添加血条UI
    public void AddHpUi(GameObject _obj)
    {
        if (_obj != null)
        {
            var hpObj = GameManager.instance.AddPrefab("UI/Hp", CanvasHpObj.transform, new Vector3(100, 100, 100), Quaternion.identity);
            _obj.GetComponent<MonsterChase>().InitHpUiObj(hpObj);
            hpObj.SetActive(false);
            hpObj.GetComponent<Hp>().InitHpUi(_obj.transform);
        }
    }
    //设置关卡
    public void SetLevel(int _level)
    {
        if (_level >= 1 && _level <=10)
        {
            levelManagement.CurrentGkLevel = _level;
        }
    }
}
