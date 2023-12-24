using UnityEngine;

public class GameSettings : MonoBehaviour
{
    private static GameSettings s_settings;

    public const float DestroyHeight = -5.0f;
    public static int DefaultLayer = -1;
    public static int SledgeLayer = -1;

    private void Awake()
    {
        if (s_settings != this && s_settings != null)
        {
            Destroy(s_settings.gameObject);
        }

        s_settings = this;
        DontDestroyOnLoad(this);

        //soundManager = Instantiate(soundManagerPrefab, transform);
        DefaultLayer = LayerMask.NameToLayer("Default");
        SledgeLayer = LayerMask.NameToLayer("Sledge");
    }
}
