using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using Spine.Unity;
using UnityEngine.SceneManagement;
using DamageNumbersPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameManager :MonoSingleton<GameManager>
{
    #region 构造函数及其变量
    public GameManager()
    {
        configMag = new ConfigManager();
    }
    public static bool isDbugLog = true;
    public PlayerData playerData = null;                            //玩家数据（本地持久化）
    public ConfigManager configMag;
    private System.Random Random;                                   //随机种子
    private int TimeNumber = 0;
    private List<UnityAction> unityActionList = new List<UnityAction>();
    public bool isBattle = true;


    public static int TI_LI_MAX_NUMBER = 100;
    public static int TI_LI_CD_NUMBER = 600;
    public DamageNumber prefab;
    public DamageNumber prefabBaoJi;
    public DamageNumber prefabPlayer;

    //颜色数组
    [SerializeField] public List<Color> colors = new List<Color>();

    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Vector2 hotSpot = Vector2.zero;
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;
    //后处理相关
    private Volume globalVolume;
    private ColorCurves colorCurves;
    //测试按钮空格用
    public bool isSpace = false;

    #endregion

    private void Update()
    {
        foreach (var item in unityActionList)
        {
            item.Invoke();
        }
    }
    #region Awake()
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        //Application.targetFrameRate = 60;//设置帧率为60帧
        GetLocalPlayerData();
        Random = new System.Random(Guid.NewGuid().GetHashCode());
    }
    #endregion



    private void Start()
    {
        globalVolume = FindObjectOfType<Volume>();
        if (globalVolume != null && globalVolume.profile != null)
        {
            // 尝试获取Color Curves组件
            if (globalVolume.profile.TryGet<ColorCurves>(out colorCurves))
            {
                // 初始状态设置
                SetColorCurvesActive(false); // 或 false
            }
        }
        this.InvokeRepeating("CheckTime", 0, 0.1f);
        BeginGame();
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
        CreatTipsPanel();
    }

    void CheckTime()
    {
        TimeNumber++;

        if (TimeNumber % 10 == 0)
        {
       
        }
        if (TimeNumber % 20 == 0)
        {

        }
        BattleManager.GetInstance().CheckTime();
    }


    #region OnApplicationPause(bool pause)切屏感知
    public void OnApplicationPause(bool pause)
    {
        if (pause == true)
        {
            if (isDbugLog)
                Debug.Log("切屏感知");
            SaveGame();
        }
    }
    #endregion

    #region OnApplicationQuit() 退出游戏感知
    public void OnApplicationQuit()
    {
        if (isDbugLog)
            Debug.Log("退出感知");
        SaveGame();

    }
    #endregion

    #region 获取本地数据
    public void GetLocalPlayerData()
    {
        playerData = PlayerData.GetLocalData();//读取本地持久化玩家数据(包括本土化设置)
        configMag.InitGameCfg();//读取配置表
        playerData.InitData();//根据配置表和本地数据初始化游戏数据
    }
    #endregion

    #region SaveGame() 保存玩家数据
    public void SaveGame()
    {
        //if(SocketManager.instance.socket!=null)
        //SocketManager.instance.socket.Disconnect();
        playerData.Save();
    }
    #endregion

    #region OnDestroy()
    private void OnDestroy()
    {
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
        StopAllCoroutines();
    }
    #endregion

    /// <summary>
    /// 注册一个update在这里跑
    /// </summary>
    /// <param name="_action"></param>
    public void AddUpdateListener(UnityAction _action)
    {
        unityActionList.Add(_action);
    }

    /// <summary>
    /// 加载图片
    /// </summary>
    public void SpritPropImage(string id, Image image)
    {
        string path = "Icon/" + id;
        Sprite Tab3Img = ResourcesLoad.instance.Load<Sprite>(path);
        image.sprite = Tab3Img;
    }

    /// <summary>
    /// 加载图片--装备图标
    /// </summary>
    public void SpritPropEquipIcon(string id, Image image)
    {
        string path = "EquipIcon/" + id;
        Sprite Tab3Img = ResourcesLoad.instance.Load<Sprite>(path);
        image.sprite = Tab3Img;
    }


    /// <summary>
    /// 加载图片
    /// </summary>
    public void SpritPropImageByPath(string path, Image image)
    {
        Sprite Tab3Img = Resources.Load(path, typeof(Sprite)) as Sprite;
        image.sprite = Tab3Img;
    }

    /// <summary>
    /// 加载图片
    /// </summary>
    public void SpritPropImageByPath(string path, SpriteRenderer image)
    {
        Sprite Tab3Img = Resources.Load(path, typeof(Sprite)) as Sprite;
        image.sprite = Tab3Img;
    }

    /// <summary>
    /// 添加预制体
    /// </summary>
    /// <param name="name"></param>
    /// <param name="fatherTransform"></param>
    /// <returns></returns>
    public GameObject AddPrefab(string name, Transform fatherTransform)
    {
        string newpath = "Prefab/" + name;
        GameObject obj = ObjPool.instance.GetObj(newpath, fatherTransform);
        // 检查是否已有 DesObj，没有则添加
        DesObj desObj = obj.GetComponent<DesObj>();
        if (desObj == null)
        {
            desObj = obj.AddComponent<DesObj>();
        }
        desObj.InitDes(newpath);
        return obj;
    }
    public GameObject AddPrefab(GameObject _obj, Transform fatherTransform)
    {
        string newpath = "Prefab/" + name;
        GameObject obj = ObjPool.instance.GetObj(_obj, fatherTransform);
        // 检查是否已有 DesObj，没有则添加
        DesObj desObj = obj.GetComponent<DesObj>();
        if (desObj == null)
        {
            desObj = obj.AddComponent<DesObj>();
        }
        desObj.InitDes(newpath);
        return obj;
    }
    /// <summary>
    /// 添加预制体
    /// </summary>
    /// <param name="name"></param>
    /// <param name="fatherTransform"></param>
    /// <returns></returns>
    public GameObject AddPrefab(GameObject _obj, Transform fatherTransform, Vector3 position, Quaternion rotation)
    {
        string newpath = "Prefab/" + name;
        GameObject obj = ObjPool.instance.GetObj(_obj, fatherTransform, position, rotation);
        // 检查是否已有 DesObj，没有则添加
        DesObj desObj = obj.GetComponent<DesObj>();
        if (desObj == null)
        {
            desObj = obj.AddComponent<DesObj>();
        }
        desObj.InitDes(newpath);
        return obj;
    }
    public GameObject AddPrefab(string name, Transform fatherTransform, Vector3 position, Quaternion rotation)
    {
        string newpath = "Prefab/" + name;
        GameObject obj = ObjPool.instance.GetObj(newpath, fatherTransform, position, rotation);
        // 检查是否已有 DesObj，没有则添加
        DesObj desObj = obj.GetComponent<DesObj>();
        if (desObj == null)
        {
            desObj = obj.AddComponent<DesObj>();
        }
        desObj.InitDes(newpath);
        return obj;
    }

    /// <summary>
    /// 销毁预制体
    /// </summary>
    /// <returns></returns>
    public void DestroyPrefab(string name, GameObject gameObject)
    {
        string[] list = name.Split(new char[] { '(' });
        if (list.Length != 2)
        {
            string newpath = "Prefab/" + name;
            ObjPool.instance.Recycle(newpath, gameObject);
        }
        else
        {
            string newpath = "Prefab/" + list[0];
            ObjPool.instance.Recycle(newpath, gameObject);
        }
        return;
    }
    /// <summary>
    /// 销毁预制体
    /// </summary>
    /// <returns></returns>
    public void DestroyPrefab(GameObject prefabObj, GameObject gameObject, string _path = null)
    {
        ObjPool.instance.Recycle(prefabObj, gameObject, "Prefab/" + _path);
        return;
    }
    /// <summary>
    /// 销毁预制体
    /// </summary>
    /// <returns></returns>
    public void DestroyPrefab(GameObject gameObject)
    {
        string name = gameObject.GetComponent<DesObj>().name;
        ObjPool.instance.Recycle(name, gameObject);
        return;
    }
    /// <summary>
    /// 播放动画并重置动画到第0帧
    /// </summary>
    public void PlaySpine(SkeletonGraphic _skeletonGraphic, bool isLoop, string _spineName, bool isRest)
    {
        if (isRest)
        {
            _skeletonGraphic.AnimationState.ClearTracks();
            _skeletonGraphic.AnimationState.Update(0);
        }
        _skeletonGraphic.AnimationState.SetAnimation(0, _spineName, isLoop);

        return;
    }
    /// <summary>
    /// 播放动画并重置动画到第0帧
    /// </summary>
    public void PlaySpine(Animator _animator, string _spineName, bool isRest)
    {
        //_animator.Play(_spineName, 0 ,0f);
        if (isRest)
        {
            //_animator.Update(0);
            _animator.Play(_spineName, 0, 0f);
        }
        else
        {
            _animator.Play(_spineName);
        }
        return;
    }
    /// <summary>
    /// 获取对象池内对象数据
    /// </summary>
    /// <returns></returns>
    public ObjPool.PoolItem GetPoolItem(string name)
    {
        string newpath = "Prefab/" + name;
        return ObjPool.instance.GetPoolItem(newpath); ;
    }
    /// <summary>
    /// 网络拉取图片
    /// </summary>
    /// <param name="_url"></param>
    /// <param name="_image"></param>
    /// <returns></returns>
    public IEnumerator GetHead(string _url, Image _image)
    {
        if (_url == string.Empty || _url == "")
        {
            _url = "https://p11.douyinpic.com/aweme/100x100/aweme-avatar/mosaic-legacy_3797_2889309425.jpeg?from=3067671334";
        }

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(_url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(1f, 1f));
                _image.sprite = sprite;
                //Renderer renderer = plane.GetComponent<Renderer>();
                //renderer.material.mainTexture = texture;
            }
        }
    }

    /// <summary>
    /// 清空数据
    /// </summary>
    public void CleraPlayerData()
    {
        PlayerData.ClearLocalData();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Editor/Tools/Clear")]
    static void CleraPlayerData1()
    {
        PlayerData.ClearLocalData();
    }
#endif
    private GameObject[] GetDontDestroyOnLoadGameObjects()
    {
        var allGameObjects = new List<GameObject>();
        allGameObjects.AddRange(FindObjectsOfType<GameObject>());
        //移除所有场景包含的对象
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            var objs = scene.GetRootGameObjects();
            for (var j = 0; j < objs.Length; j++)
            {
                allGameObjects.Remove(objs[j]);
            }
        }
        //移除父级不为null的对象
        int k = allGameObjects.Count;
        while (--k >= 0)
        {
            if (allGameObjects[k].transform.parent != null)
            {
                allGameObjects.RemoveAt(k);
            }
        }
        return allGameObjects.ToArray();
    }

    //暂停游戏
    public void PauseGame()
    {
        Time.timeScale = 0f;
    }
    //恢复游戏
    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }
    public void BeginGame()
    {
        prefab.enablePooling = true;
        prefabBaoJi.enablePooling = true;
        prefabPlayer.enablePooling = true;
        UiManager.instance.CreatUi("Panel_Begin");
    }
    //开始战斗
    public void BeginBattle()
    {
        BattleManager.GetInstance().BeginGame();
    }
    //生成伤害瓢字
    public void SpawnAttackNumber(Vector3 _vector3, HeroDamage _heroDamage)
    {
        if (_heroDamage.type == 1)
        {
            prefab.Spawn(_vector3, _heroDamage.finalDamage);
        }
        else if (_heroDamage.type == 2)
        {
            prefabBaoJi.Spawn(_vector3, _heroDamage.criticalDamage);
        }
    }
    public void SpawnPlayerAttackNumber(Vector3 _vector3, int _damage)
    {
        Debug.Log(_vector3);
        prefabPlayer.Spawn(_vector3, _damage);
    }
    //英雄增加属性
    public void AddHero()
    {
        var state = BattleManager.GetInstance().GetHeroObj().GetComponent<PlayerContry>().GetHeroState();
        state.rangedDamage += 5;
        //BattleManager.GetInstance().GetWeaponObj().GetComponent<WeaponContry>().UpdateFinalStats();
    }
    //根据技能等级切换颜色
    public void ChangeColor(int _level, Material _materialInstance, float _emissionIntensity)
    {
    
        int index = _level - 1;
        if (index >= GameManager.instance.colors.Count)
        {
            index = GameManager.instance.colors.Count - 1;
        }
        Color hdrColor = GameManager.instance.colors[index] * _emissionIntensity;
        // 设置HDR emission颜色
        _materialInstance.SetColor("_EmissionColor", hdrColor);
    }
    //创建tipsPanel
    public void CreatTipsPanel()
    {
        UiManager.instance.CreatUi("Panel_Tips");
    }
    //创建tips
    public void SetTips(int _keyId)
    {
        UiManager.instance.CreatTipsUi(configMag.GetMsgInfoCfgByKey(_keyId).msg, 1f);
    }
    //后处理黑白
    public void SetColorCurvesActive(bool isActive)
    {
        if (colorCurves != null)
        {
            colorCurves.active = isActive;
        }
    }
    public void TestButton(int _index)
    {
        var cfg = BattleManager.GetInstance().configMag.GetMonsterInfoCfgCfgByKey(2);
        GameManager.instance.StartCoroutine(BattleManager.GetInstance().AddMonsterIenum(cfg, 10));
    }
    public void TestButton2()
    {
        BattleManager.GetInstance().AddWeaponPinZhiRand();
    }
    public void TestButton3()
    {
        BattleManager.GetInstance().BuySkillBox();
    }
}
