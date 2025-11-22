using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTriggler : MonoBehaviour
{
    public DoorController door;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            door.Open();
            NarratorManager.Instance.Say("欢迎来到心理状况改善");
        }
    }
}
