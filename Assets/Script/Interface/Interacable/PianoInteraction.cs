using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoInteraction : MonoBehaviour, IInteractable
{

    private bool beingPlayed = false;

    // 对演奏界面UI的引用
    [SerializeField] private GameObject pianoUI;
    // 可选的玩家控制脚本，用于在演奏时禁用移动
    [SerializeField] private MonoBehaviour playerController;
    // 视角控制脚本， 用于在演奏时禁用视角旋转
    [SerializeField] private MonoBehaviour mouseLook;
    public string InteractionPrompt => beingPlayed ? "按E关闭" : "按E演奏";

    public Transform Transform => transform;

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        if (!beingPlayed)
        {
            EnterPianoMode();
        }
        else
        {
            ExitPianoMode();
        }
        beingPlayed = !beingPlayed;
    }

    private void EnterPianoMode()
    {
        if (pianoUI != null)
        {
            pianoUI.SetActive(true);
        }

        if (mouseLook != null)
        {
            mouseLook.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
    }

    private void ExitPianoMode()
    {
        if (pianoUI != null)
        {
            pianoUI.SetActive(false);
        }

        if (mouseLook != null)
        {
            mouseLook.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }
}
