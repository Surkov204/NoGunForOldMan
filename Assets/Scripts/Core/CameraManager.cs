using JS.Utils;
using UnityEngine;

public class CameraManager : ManualSingletonMono<CameraManager>
{
    [SerializeField] private Animator cameraAnimator;

    public void GrenadeCamera() {
        cameraAnimator.ResetTrigger("grenadeShaking");
        cameraAnimator.SetTrigger("grenadeShaking");
    }
    public void ResetGrenadeCamera(){
     
        cameraAnimator.Play("idle");
    }
}
