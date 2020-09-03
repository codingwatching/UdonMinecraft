
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class BlockSelector : UdonSharpBehaviour
{
    [SerializeField] WorldGenerator generator;
    [SerializeField] BlockController blockController;
    [SerializeField] Canvas canvas;
    [SerializeField] GameObject pickaxe;
    [SerializeField] GameObject block;

    bool pickBlock = false;
    Vector3 initScale;

    void Start()
    {
        initScale = transform.localScale;
        OnPick();

        blockController.updateHeldBlock(gameObject, 35);
    }

    void Update()
    {
        if (Networking.LocalPlayer != null)
        {
            if (!pickBlock)
            {
                if (Networking.LocalPlayer.IsUserInVR())
                {
                    transform.position = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position + new Vector3(0, 0.35f, 0);
                    transform.rotation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
                }
                else
                {
                    transform.position = Networking.LocalPlayer.GetPosition() + new Vector3(0.3f, 1f, 0.2f);
                    transform.rotation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
                }
            } else
            {
                if (Networking.LocalPlayer.IsUserInVR())
                {
                    canvas.gameObject.transform.position = Networking.LocalPlayer.GetPosition() + Networking.LocalPlayer.GetRotation() * new Vector3(0, 1, 1);
                    canvas.gameObject.transform.rotation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
                }
                else
                {
                    canvas.gameObject.transform.position = Networking.LocalPlayer.GetPosition() + new Vector3(0.5f, 1f, 0.1f);
                    canvas.gameObject.transform.rotation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
                }
            }
        }
    }

    public override void OnPickup()
    {
        pickBlock = true;
        pickaxe.SetActive(false);
        block.SetActive(false);
        canvas.gameObject.SetActive(true);
        gameObject.transform.localScale = new Vector3(0, 0, 0);
    }

    void OnPick()
    {
        pickBlock = false;
        transform.localScale = new Vector3(initScale.x, initScale.y, initScale.z) / 3f;
        gameObject.SetActive(true);
        pickaxe.SetActive(true);
        block.SetActive(true);
        canvas.gameObject.SetActive(false);
        blockController.updateHeldBlock(blockController.gameObject, blockController.type);
        blockController.updateHeldBlock(blockController.previewBlock, blockController.type);
    }

    public void s2() { blockController.type = 2; OnPick(); }
    public void s3() { blockController.type = 3; OnPick(); }
    public void s4() { blockController.type = 4; OnPick(); }
    public void s5() { blockController.type = 5; OnPick(); }
    public void s6() { blockController.type = 6; OnPick(); }
    public void s7() { blockController.type = 7; OnPick(); }
    public void s8() { blockController.type = 8; OnPick(); }
    public void s9() { blockController.type = 9; OnPick(); }
    public void s10() { blockController.type = 10; OnPick(); }
    public void s11() { blockController.type = 11; OnPick(); }
    public void s12() { blockController.type = 12; OnPick(); }
    public void s13() { blockController.type = 13; OnPick(); }
    public void s14() { blockController.type = 14; OnPick(); }
    public void s15() { blockController.type = 15; OnPick(); }
    public void s16() { blockController.type = 16; OnPick(); }
    public void s17() { blockController.type = 17; OnPick(); }
    public void s18() { blockController.type = 18; OnPick(); }
    public void s19() { blockController.type = 19; OnPick(); }
    public void s20() { blockController.type = 20; OnPick(); }
    public void s21() { blockController.type = 21; OnPick(); }
    public void s22() { blockController.type = 22; OnPick(); }
    public void s23() { blockController.type = 23; OnPick(); }
    public void s24() { blockController.type = 24; OnPick(); }
    public void s25() { blockController.type = 25; OnPick(); }
    public void s26() { blockController.type = 26; OnPick(); }
    public void s27() { blockController.type = 27; OnPick(); }
    public void s28() { blockController.type = 28; OnPick(); }
    public void s29() { blockController.type = 29; OnPick(); }
    public void s30() { blockController.type = 30; OnPick(); }
    public void s31() { blockController.type = 31; OnPick(); }
    public void s32() { blockController.type = 32; OnPick(); }
    public void s33() { blockController.type = 33; OnPick(); }
    public void s34() { blockController.type = 34; OnPick(); }
    
}
