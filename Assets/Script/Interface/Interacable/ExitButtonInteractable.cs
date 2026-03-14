using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitButtonInteractable : MonoBehaviour, IInteractable
{

    [Header("游戏控制器")]
    [SerializeField] private TherapyGameManager gameManager;
    public string InteractionPrompt => "按E退回到主菜单";

    public Transform Transform => transform;

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        Debug.Log("the button is pushed");
        gameManager.ReturnToMainMenu();
    }

}
