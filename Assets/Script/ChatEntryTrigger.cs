using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChatEntryTrigger : MonoBehaviour
{
    [SerializeField] UnityEvent onEnter;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            onEnter.Invoke();
    }
}
