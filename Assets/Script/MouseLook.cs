using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f; // 鼠标灵敏度
    private float xRotation = 0f; // 垂直旋转角度

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // 锁定鼠标光标
    }

    void Update()
    {
        // 获取鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 计算垂直旋转角度并限制范围
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // 旋转摄像机持有者（CameraHolder）
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 旋转玩家对象（假设玩家对象的标签为 “Player”）
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.transform.Rotate(Vector3.up * mouseX);
    }
}
