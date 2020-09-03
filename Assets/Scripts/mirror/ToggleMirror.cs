
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ToggleMirror : UdonSharpBehaviour
{
    [SerializeField] GameObject targetMirror;
    [SerializeField] GameObject[] allMirrors;

    public override void Interact()
    {
        foreach(GameObject mirror in allMirrors)
        {
            mirror.SetActive(false);
        }
        if(targetMirror != null)
        {
            targetMirror.SetActive(true);
        }
    }
}
