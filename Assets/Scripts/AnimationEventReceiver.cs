using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    public void PlayeSe()
    {
        GameController.Instance?.PlaySE();
    }
}