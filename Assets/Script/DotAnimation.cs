using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

// 这个脚本的用途是在UI文本上显示动态的点动画（. .. ...）
// 用于AI思考等待时的视觉反馈
public class DotAnimation : MonoBehaviour
{
    public TMP_Text targetText;
    
    void OnEnable()
    {
        if(targetText == null) targetText = GetComponentInChildren<TMP_Text>();
        StartCoroutine(AnimateDots());
    }

    IEnumerator AnimateDots()
    {
        while (true)
        {
            targetText.text = ".";
            yield return new WaitForSeconds(0.3f);
            targetText.text = "..";
            yield return new WaitForSeconds(0.3f);
            targetText.text = "...";
            yield return new WaitForSeconds(0.3f);
        }
    }
}