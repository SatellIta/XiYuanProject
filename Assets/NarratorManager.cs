using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NarratorManager : MonoBehaviour
{
    public static NarratorManager Instance;
    public TextMeshProUGUI ui;
    void Awake() => Instance = this;
    public void Say(string line, float autoClear = 5f)
    {
        ui.text = line;
        CancelInvoke();
        Invoke(nameof(Clear), autoClear);
    }
    void Clear() => ui.text = "";
}
