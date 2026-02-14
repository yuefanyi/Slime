using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerContry : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 10f; // 新增：旋转平滑度

    [Header("组件引用")]
    [SerializeField] private Camera playerCamera; // 可拖拽指定摄像机
    [SerializeField] private Animator playerAnimator; // 新增：Animator组件引用

    private CharacterController controller;
    private Vector3 lastMoveDirection; // 记录最后移动方向
    private float currentSpeed; // 新增：当前速度值
    private CharacterStats heroState;

    //闪白效果
    private bool isFlashing = false;
    private Renderer[] renderers;
    public Color flashColor = Color.red; // 闪白颜色
    private float flashDuration = 0.1f; // 颜色闪白时长
    private Color[] originalColors;
    private Material[] originalMaterials;
    private bool isDead = false;

    void Start()
    {
        // 获取组件
        controller = GetComponent<CharacterController>();

        // 确保有摄像机引用
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogError("未找到主摄像机！请手动指定玩家摄像机。");
            }
        }

        // 确保有Animator引用
        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<Animator>();
            if (playerAnimator == null)
            {
                Debug.LogError("未找到Animator组件！请手动指定或添加Animator组件。");
            }
        }

        // 初始化最后方向为当前朝向
        lastMoveDirection = transform.forward;

        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;
            if (originalMaterials[i].HasProperty("_BaseColor"))
            {
                originalColors[i] = originalMaterials[i].GetColor("_BaseColor");
            }
        }
    }

    void Update()
    {
        HandleMovement();
        UpdateAnimation(); // 新增：每帧更新动画状态
        //UpdateRota();
    }

    private void HandleMovement()
    {
        if (isDead)
        {
            return;
        }
        // 检查 ESC 键
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 这里可以添加暂停游戏、打开菜单等逻辑
            HandleEscapePressed();
        }
        // 获取输入
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 创建基于摄像机的移动方向
        Vector3 moveDirection = CalculateCameraRelativeDirection(horizontal, vertical);
        // 归一化向量，确保所有方向速度一致
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }
        // 计算当前速度（用于动画控制）
        currentSpeed = moveDirection.magnitude;
        // 移动角色
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        // 仅在真正移动时更新朝向
        if (moveDirection != Vector3.zero)
        {
            RotateTowardsMovement(moveDirection);
            lastMoveDirection = moveDirection; // 更新最后方向
        }
    }

    private void UpdateRota()
    {
        // 1. 获取鼠标屏幕位置
        Vector3 mouseScreenPos = Input.mousePosition;

        // 2. 创建从摄像机到鼠标的射线
        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPos);

        // 3. 定义水平面（法线向上）并计算交点
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // 通过原点且平行于地面的平面
        float enter;

        if (groundPlane.Raycast(ray, out enter))
        {
            // 4. 获取鼠标在水平面的世界坐标
            Vector3 mouseWorldPos = ray.GetPoint(enter);

            // 5. 计算水平方向向量（忽略Y轴）
            Vector3 direction = mouseWorldPos - transform.position;
            direction.y = 0; // 关键：强制Y分量为0

            // 6. 应用旋转（仅绕Y轴旋转）
            if (direction != Vector3.zero)
            {
                // 直接设置旋转
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                //transform.rotation = targetRotation;

                //或使用插值平滑旋转（可选）
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }
    // 新增：更新动画状态
    private void UpdateAnimation()
    {
        if (playerAnimator != null)
        {
            // 根据当前速度设置动画参数
            playerAnimator.SetFloat("Speed", currentSpeed);

            // 或者使用布尔值控制（二选一）
            // playerAnimator.SetBool("IsMoving", currentSpeed > 0.1f);
        }
    }

    private Vector3 CalculateCameraRelativeDirection(float horizontal, float vertical)
    {
        // 获取摄像机方向向量
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;

        // 忽略Y轴分量
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // 组合方向向量
        return (cameraForward * vertical) + (cameraRight * horizontal);
    }

    private void RotateTowardsMovement(Vector3 movementDirection)
    {
        // 确保方向有效
        if (movementDirection.sqrMagnitude > 0.001f)
        {
            // 创建目标旋转
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);

            // 平滑旋转
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    public void SetHeroState(CharacterStats _heroState)
    {
        heroState = _heroState;
        isDead = false;
        UpdateMoveSpeed();
    }
    public CharacterStats GetHeroState()
    {
        return heroState;
    }
    //更新移速
    public void UpdateMoveSpeed()
    {
        moveSpeed = heroState.moveSpeed/100f;
    }

    public void PlayerApplyHitEffects(int _damage)
    {
        //闪白
        StartCoroutine(FlashColor());
        //瓢字
        GameManager.instance.SpawnPlayerAttackNumber(transform.position, _damage);
        //扣血
        heroState.health -= _damage;
        //刷新血量
        BattleManager.GetInstance().UpdateBattleUi();
        //检查死亡
        if (heroState.health <= 0 && isDead == false)
        {
            Debug.Log("死亡");
            //播放死亡效果
            Sequence deathSequence = DOTween.Sequence();
            // 3.3 缩放效果
            deathSequence.Append(transform.DOScale(
                Vector3.zero,
                0.25f
            ).SetEase(Ease.InBack));
            GameManager.instance.StartCoroutine(BattleManager.GetInstance().EndGame(transform.position));
            isDead = true;
        }

    }

    // 颜色闪白效果
    private IEnumerator FlashColor()
    {
        if (isFlashing) yield break;

        isFlashing = true;

        // 应用闪白材质
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].material.SetColor("_BaseColor", flashColor);
            }
        }

        yield return new WaitForSeconds(flashDuration);

        // 恢复原始颜色
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].material.SetColor("_BaseColor", originalColors[i]);
            }
        }

        isFlashing = false;
    }
    //ESC案件
    public void HandleEscapePressed()
    {
        BattleManager.GetInstance().PlayerLevelUp();
    }
}