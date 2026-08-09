using UnityEngine;

public enum SpecialButtonType
{
    Upgrade,
    Unlock,
    Sell
}

public class SpecialButtonAudioTag : MonoBehaviour
{
    public SpecialButtonType buttonType;
}
