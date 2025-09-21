using UnityEngine;

public interface ISaveable
{
    object CaptureState();             
    void RestoreState(object state);   
    string GetUniqueId();
    void ResetState();
}
