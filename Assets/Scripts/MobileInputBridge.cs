using UnityEngine;

public class MobileInputBridge : MonoBehaviour
{
    public PlayerController playerController;

    private Vector2 currentInput = Vector2.zero;

    public void PressLeft()
    {
        currentInput.x = -1;
        playerController.ReceiveMobileMove(currentInput);
    }

    public void ReleaseLeft()
    {
        if (currentInput.x == -1) currentInput.x = 0;
        playerController.ReceiveMobileMove(currentInput);
    }

    public void PressRight()
    {
        currentInput.x = 1;
        playerController.ReceiveMobileMove(currentInput);
    }

    public void ReleaseRight()
    {
        if (currentInput.x == 1) currentInput.x = 0;
        playerController.ReceiveMobileMove(currentInput);
    }

    public void Jump()
    {
        playerController.ReceiveMobileJump();
    }

    public void Attack()
    {
        playerController.ReceiveMobileAttack();
    }
}
