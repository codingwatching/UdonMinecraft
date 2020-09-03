
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PickaxeController : UdonSharpBehaviour
{
    [SerializeField] WorldGenerator generator;
    [SerializeField] GameObject hitbox;
    [SerializeField] GameObject block;
    [SerializeField] GameObject blockSelector;
    [SerializeField] BlockSync blockSync;

    bool pickedUp = false;
    Vector3 initScale;
    Quaternion initRotation;

    void Start()
    {
        initScale = transform.localScale;
        initRotation = transform.localRotation;
        OnDrop();
    }

    void Update()
    {
        if (Networking.LocalPlayer != null)
        {
            if (!pickedUp)
            {
                // Inventory location management
                if (Networking.LocalPlayer.IsUserInVR())
                {
                    transform.position = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position + new Vector3(0, 0.1f, 0);
                    transform.rotation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * initRotation;
                }
                else
                {
                    transform.position = Networking.LocalPlayer.GetPosition() + new Vector3(0.3f, 1f, 0);
                    transform.rotation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * initRotation;
                }
            }
            else
            {
                // Check if pickaxe hitbox is in solid block and if so remove it
                int[] block = generator.positionToBlockPosition(hitbox.transform.position);
                if (generator.isInBounds(block[0], block[1], block[2]) && generator.getBlockType(block[0], block[1], block[2]) != 0)
                {
                    blockSync.broadcastBlock((byte) block[0], (byte) block[1], (byte) block[2], 0);
                }
            }
        }
    }

    // Equip pickaxe
    override public void OnPickup()
    {
        pickedUp = true;
        transform.localScale = new Vector3(initScale.x, initScale.y, initScale.z);
        block.SetActive(false);
        blockSelector.SetActive(false);
    }

    // Reset inventory
    override public void OnDrop()
    {
        pickedUp = false;
        transform.localScale = new Vector3(initScale.x, initScale.y, initScale.z) / 10f;
        block.SetActive(true);
        blockSelector.SetActive(true);
    }
}
