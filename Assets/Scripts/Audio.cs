
using System;
using UnityEngine;

[Serializable]
public class SystemSound
{
    public Audio type;
    public AudioClip clip;
}

public enum Audio
{
    Failure,
    Success,
    OnDish,
    Stew,
    Grill,
    Cut,
    Fry,
    Result
}