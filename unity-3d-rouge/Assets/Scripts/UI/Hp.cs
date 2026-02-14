using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hp : MonoBehaviour
{
    private Transform target;          // 需要跟随的目标
    public Vector3 offset = new Vector3(0, 2f, 0);  // 在头顶的偏移量
    public Camera mainCamera;
    private Image hpImage;
    private bool isShow = true;

    private void OnEnable()
    {
        hpImage = this.transform.Find("Image").GetComponent<Image>();
    }
    void Start()
    {
        // 如果未指定摄像机，使用主摄像机
        if (mainCamera == null)
            mainCamera = Camera.main;
        
    }

    void Update()
    {
        if (target != null && mainCamera != null)
        {
            // 更新血条位置
            transform.position = target.position;

            // 确保血条始终面向摄像机
            transform.rotation = Quaternion.identity;
        }
    }

    public void InitHpUi(Transform _transform)
    {
        target = _transform.Find("hpTag");
        // 更新血条位置
        transform.position = target.position;
        // 确保血条始终面向摄像机
        transform.rotation = Quaternion.identity;

        isShow = true;

        this.Invoke("ShowHp", 1f);
        //ShowHp();
    }

    public void ShowHp()
    {
        UpdateHpUi(1f);
    }

    public void HpDeath()
    {
        isShow = false;
    }

    public void UpdateHpUi(float _fill, bool _isShow = true)
    {
        if (this.gameObject.activeSelf == false && _isShow && isShow)
        {
            this.gameObject.SetActive(true);
        }
        if (hpImage != null)
        {
            hpImage.fillAmount = _fill;
        }
    }
}
