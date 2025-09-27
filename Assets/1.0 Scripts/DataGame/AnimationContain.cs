using UnityEngine;

[CreateAssetMenu(fileName = "AnimationContain", menuName = "Scriptable Objects/AnimationContain")]
public class AnimationContain : ScriptableObject
{
    [SerializeField] private Animator shakingCameraBomba;
    public Animator ShakingCameraBomba => shakingCameraBomba;
}
