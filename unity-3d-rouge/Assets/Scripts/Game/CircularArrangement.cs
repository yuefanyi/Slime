using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class CircularArrangement : MonoBehaviour
{
    public List<GameObject> gameObjects = new List<GameObject>(); // 需要排列的GameObject列表
    public Transform centerPoint;        // 旋转中心点
    public float radius = 5f;            // 分布半径
    public float rotationSpeed = 30f;    // 旋转速度（度/秒）
    public bool autoArrangeOnChange = true; // 列表变化时自动重新排列
    public float arrangementDelay = 0.1f; // 重新排列的延迟时间（防止同一帧多次更新）

    private int lastObjectCount = 0;     // 用于检测对象数量变化
    private bool isArranging = false;    // 防止重复排列

    void Start()
    {
        ArrangeObjectsInCircle();
        lastObjectCount = gameObjects.Count;
    }

    void Update()
    {
        // 检测对象数量变化
        if (gameObjects.Count != lastObjectCount)
        {
            lastObjectCount = gameObjects.Count;
            if (autoArrangeOnChange && !isArranging)
            {
                StartCoroutine(DelayedArrange());
            }
        }
        this.transform.position = centerPoint.position;
        RotateObjectsAroundCenter();

    }

    // 动态添加物体
    public void AddObject(GameObject newObject)
    {
        if (!gameObjects.Contains(newObject))
        {
            gameObjects.Add(newObject);
            if (autoArrangeOnChange && !isArranging)
            {
                StartCoroutine(DelayedArrange());
            }
        }
    }

    // 动态移除物体
    public void RemoveObject(GameObject objectToRemove)
    {
        if (gameObjects.Contains(objectToRemove))
        {
            gameObjects.Remove(objectToRemove);
            if (autoArrangeOnChange && !isArranging)
            {
                StartCoroutine(DelayedArrange());
            }
        }
    }

    // 清空所有物体
    public void ClearAllObjects()
    {
        gameObjects.Clear();
        lastObjectCount = 0;
    }

    // 延迟排列（防止同一帧多次更新）
    IEnumerator DelayedArrange()
    {
        isArranging = true;
        yield return new WaitForSeconds(arrangementDelay);
        ArrangeObjectsInCircle();
        isArranging = false;
    }

    // 将物体均匀分布在圆周上
    public void ArrangeObjectsInCircle()
    {
        if (gameObjects == null || gameObjects.Count == 0)
        {
            Debug.Log("No objects to arrange");
            return;
        }

        // 清理空引用
        gameObjects.RemoveAll(obj => obj == null);

        float angleStep = 360f / gameObjects.Count;
        float currentAngle = 0f;

        foreach (GameObject obj in gameObjects)
        {
            if (obj == null) continue;

            // 计算位置 (X = cos(角度), Z = sin(角度))
            Vector3 position = centerPoint.position +
                new Vector3(
                    Mathf.Cos(currentAngle * Mathf.Deg2Rad) * radius,
                    0,
                    Mathf.Sin(currentAngle * Mathf.Deg2Rad) * radius
                );

            obj.transform.position = position;
            //obj.transform.LookAt(centerPoint); // 使物体朝向中心点

            currentAngle += angleStep;
        }
    }

    // 围绕中心点旋转所有物体
    void RotateObjectsAroundCenter()
    {
        foreach (GameObject obj in gameObjects)
        {
            if (obj != null)
            {
                // 围绕中心点旋转
                obj.transform.RotateAround(
                    centerPoint.position,
                    Vector3.up,
                    rotationSpeed * Time.deltaTime
                );

                // 保持物体始终朝向中心点
                //obj.transform.LookAt(centerPoint);
            }
        }
    }

    // 在Inspector中显示重新排列按钮（编辑器专用）
    [ContextMenu("Re-arrange Objects")]
    void ArrangeInEditor()
    {
        ArrangeObjectsInCircle();
    }
}