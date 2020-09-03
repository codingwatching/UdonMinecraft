
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SpawnController : UdonSharpBehaviour
{
    [SerializeField] WorldGenerator generator;

    public override void Interact()
    {
        byte mapMiddle = (byte) Mathf.FloorToInt(generator.getTotalChunksWidth() / 2f);

        Networking.LocalPlayer.TeleportTo(new Vector3(mapMiddle, generator.getXZHeight(mapMiddle, mapMiddle), mapMiddle),
                                          Networking.LocalPlayer.GetRotation(),
                                          VRC_SceneDescriptor.SpawnOrientation.Default,
                                          false);
    }

    void Update()
    {
        if(gameObject.activeSelf && Networking.LocalPlayer != null && Vector3.Distance(Networking.LocalPlayer.GetPosition(), transform.position) <= 2.5f)
        {
            Interact();
        }
    }
}
