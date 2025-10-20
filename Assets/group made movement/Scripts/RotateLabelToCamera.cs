using Unity.VisualScripting;
using UnityEngine;

public class RotateLabelToCamera: MonoBehaviour
{
    /*Runs if related function is ran (UpdateStepLabel)*/
    void LateUpdate()
    {
        if(Camera.main != null)
        {
            transform.LookAt(Camera.main.transform.position);
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }
    }
}
