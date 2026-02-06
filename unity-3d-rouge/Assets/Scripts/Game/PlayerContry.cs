using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerContry : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f; // 新增：旋转平滑度

    [Header("组件引用")]
    [SerializeField] private Camera playerCamera; // 可拖拽指定摄像机

    private CharacterController controller;
    private Vector3 lastMoveDirection; // 记录最后移动方向

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

        // 初始化最后方向为当前朝向
        lastMoveDirection = transform.forward;
    }

    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // 获取输入
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 创建基于摄像机的移动方向
        Vector3 moveDirection = CalculateCameraRelativeDirection(horizontal, vertical);

        // 移动角色
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        // 仅在真正移动时更新朝向
        if (moveDirection != Vector3.zero)
        {
            RotateTowardsMovement(moveDirection);
            lastMoveDirection = moveDirection; // 更新最后方向
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
}
