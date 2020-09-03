
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WorldController : UdonSharpBehaviour
{
    [SerializeField] WorldGenerator generator;

    public override void Interact()
    {
        gameObject.SetActive(false);
        generator.startSetup();
    }

    void Update()
    {
        if (gameObject.activeSelf && Networking.LocalPlayer != null && Vector3.Distance(Networking.LocalPlayer.GetPosition(), transform.position) <= 2.5f)
        {
            Interact();
        }
    }
}
