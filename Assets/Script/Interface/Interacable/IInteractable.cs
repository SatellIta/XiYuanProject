using UnityEngine;

// 所有可交互设施的统一接口
public interface IInteractable
{
    // 交互提示文本（如“按E打开门”“按E拾取道具”）
    string InteractionPrompt { get; }
    // 核心交互逻辑（原ToggleDoor这类行为都归到这里）
    void Interact();
    // 检查是否可交互（如门是否锁定）
    bool CanInteract();

    Transform Transform { get; }
}