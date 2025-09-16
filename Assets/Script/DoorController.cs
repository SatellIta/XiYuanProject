using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public float openDistance = 2.5f;   // 向上开多高
    public float openSpeed = 2f;
    Vector3 closedPos;
    void Start() => closedPos = transform.position;
    public void Open()
    {
        StopAllCoroutines();                // 避免重复点击
        StartCoroutine(OpenRoutine());
    }
    System.Collections.IEnumerator OpenRoutine()
    {
        Vector3 openPos = closedPos + Vector3.up * openDistance;
        while (transform.position != openPos)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, openPos, openSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
