
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class BlockController : UdonSharpBehaviour
{
    [SerializeField] WorldGenerator generator;
    [SerializeField] GameObject hitbox;
    [SerializeField] GameObject pickaxe;
    [SerializeField] GameObject blockSelector;
    [SerializeField] BlockSync blockSync;
    [SerializeField] public GameObject previewBlock;

    Vector3 initScale;
    Quaternion initRotation;

    bool pickedUp = false;
    public byte type = 2; // Default held block is grass

    void Start()
    {
        initScale = transform.localScale;
        initRotation = transform.localRotation;
        OnDrop();

        updateHeldBlock(gameObject, type);
        updateHeldBlock(previewBlock, type);
    }

    void Update()
    {
        if(Networking.LocalPlayer != null)
        {
            if (!pickedUp)
            {
                // Inventory location management
                if(Networking.LocalPlayer.IsUserInVR())
                {
                    transform.position = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position + new Vector3(0, 0.25f, 0);
                    transform.rotation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * initRotation;
                } else
                {
                    transform.position = Networking.LocalPlayer.GetPosition() + new Vector3(0.3f, 1f, 0.1f);
                    transform.rotation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * initRotation;
                }
            } else
            {
                // Block preview positioning
                int[] blockPos = generator.positionToBlockPosition(hitbox.transform.position);

                if(generator.isInBounds(blockPos[0], blockPos[1], blockPos[2]))
                {
                    previewBlock.transform.position = new Vector3(blockPos[0] + 0.5f, blockPos[1], blockPos[2] + 0.5f);
                } else
                {
                    previewBlock.transform.position = new Vector3(0, 0, 0);
                }
            }
        }
    }

    // Place block
    public override void OnPickupUseUp()
    {
        int[] block = generator.positionToBlockPosition(hitbox.transform.position);
        if (generator.isInBounds(block[0], block[1], block[2]) && generator.getBlockType(block[0], block[1], block[2]) != type)
        {
            blockSync.broadcastBlock((byte) block[0], (byte) block[1], (byte) block[2], type);
        }
    }

    // Equip block for placing
    override public void OnPickup()
    {
        pickedUp = true;
        transform.localScale = new Vector3(initScale.x, initScale.y, initScale.z);
        pickaxe.SetActive(false);
        blockSelector.SetActive(false);
        gameObject.transform.position += Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * Vector3.forward;
    }

    // Reset inventory
    override public void OnDrop()
    {
        pickedUp = false;
        transform.localScale = new Vector3(initScale.x, initScale.y, initScale.z) / 3f;
        pickaxe.SetActive(true);
        blockSelector.SetActive(true);
        previewBlock.transform.position = new Vector3(0, 0, 0);
    }

    // Builds meshes and UVs for inventory items, reused a lot of the code from WorldGenerator but it's fine for now
    public void updateHeldBlock(GameObject o, byte type)
    {
        generator.fillBlockTable(); // Ensure block table is loaded

        byte[] texture = generator.getTextureOffsets(type);
        byte transparency = 0, textureX = 0, textureY = 0, sideX = 0, sideY = 0, bottomX = 0, bottomY = 0;
        if (texture.Length == 3)
        {
            transparency = texture[0];
            textureX = sideX = bottomX = texture[1];
            textureY = sideY = bottomY = texture[2];
        }
        else
        {
            transparency = texture[0];
            textureX = texture[1];
            textureY = texture[2];
            sideX = texture[3];
            sideY = texture[4];
            bottomX = texture[5];
            bottomY = texture[6];
        }

        Vector3[] vertices = new Vector3[6 * 4];
        Vector2[] uv = new Vector2[6 * 4];
        int[] tris = new int[6 * 6];

        int meshIndex = 0;
        float x = -0.5f, y = 0, z = -0.5f; // Offset to center block
        // Percent width and height of texture file
        float percentWidth = 0.02083333f;
        float percentHeight = 0.0212766f;

        vertices[meshIndex * 4 + 0] = new Vector3(x, y, z);
        vertices[meshIndex * 4 + 1] = new Vector3(x + 1, y, z);
        vertices[meshIndex * 4 + 2] = new Vector3(x, y, z + 1);
        vertices[meshIndex * 4 + 3] = new Vector3(x + 1, y, z + 1);

        uv[meshIndex * 4 + 0] = new Vector2(percentWidth * textureX, percentHeight * textureY);
        uv[meshIndex * 4 + 1] = new Vector2(percentWidth * textureX + percentWidth, percentHeight * textureY);
        uv[meshIndex * 4 + 2] = new Vector2(percentWidth * textureX, percentHeight * textureY + percentHeight);
        uv[meshIndex * 4 + 3] = new Vector2(percentWidth * textureX + percentWidth, percentHeight * textureY + percentHeight);

        tris[meshIndex * 6 + 0] = meshIndex * 4 + 0;
        tris[meshIndex * 6 + 1] = meshIndex * 4 + 2;
        tris[meshIndex * 6 + 2] = meshIndex * 4 + 1;
        tris[meshIndex * 6 + 3] = meshIndex * 4 + 2;
        tris[meshIndex * 6 + 4] = meshIndex * 4 + 3;
        tris[meshIndex * 6 + 5] = meshIndex * 4 + 1;

        meshIndex++;

        vertices[meshIndex * 4 + 0] = new Vector3(x, y - 1, z);
        vertices[meshIndex * 4 + 1] = new Vector3(x + 1, y - 1, z);
        vertices[meshIndex * 4 + 2] = new Vector3(x, y - 1, z + 1);
        vertices[meshIndex * 4 + 3] = new Vector3(x + 1, y - 1, z + 1);

        uv[meshIndex * 4 + 0] = new Vector2(percentWidth * bottomX, percentHeight * bottomY);
        uv[meshIndex * 4 + 1] = new Vector2(percentWidth * bottomX + percentWidth, percentHeight * bottomY);
        uv[meshIndex * 4 + 2] = new Vector2(percentWidth * bottomX, percentHeight * bottomY + percentHeight);
        uv[meshIndex * 4 + 3] = new Vector2(percentWidth * bottomX + percentWidth, percentHeight * bottomY + percentHeight);

        tris[meshIndex * 6 + 0] = meshIndex * 4 + 1;
        tris[meshIndex * 6 + 1] = meshIndex * 4 + 2;
        tris[meshIndex * 6 + 2] = meshIndex * 4 + 0;
        tris[meshIndex * 6 + 3] = meshIndex * 4 + 1;
        tris[meshIndex * 6 + 4] = meshIndex * 4 + 3;
        tris[meshIndex * 6 + 5] = meshIndex * 4 + 2;

        meshIndex++;

        vertices[meshIndex * 4 + 0] = new Vector3(x + 1, y - 1, z);
        vertices[meshIndex * 4 + 1] = new Vector3(x + 1, y, z);
        vertices[meshIndex * 4 + 2] = new Vector3(x + 1, y - 1, z + 1);
        vertices[meshIndex * 4 + 3] = new Vector3(x + 1, y, z + 1);

        uv[meshIndex * 4 + 0] = new Vector2(percentWidth * sideX, percentHeight * sideY);
        uv[meshIndex * 4 + 2] = new Vector2(percentWidth * sideX + percentWidth, percentHeight * sideY);
        uv[meshIndex * 4 + 1] = new Vector2(percentWidth * sideX, percentHeight * sideY + percentHeight);
        uv[meshIndex * 4 + 3] = new Vector2(percentWidth * sideX + percentWidth, percentHeight * sideY + percentHeight);

        tris[meshIndex * 6 + 0] = meshIndex * 4 + 1;
        tris[meshIndex * 6 + 1] = meshIndex * 4 + 2;
        tris[meshIndex * 6 + 2] = meshIndex * 4 + 0;
        tris[meshIndex * 6 + 3] = meshIndex * 4 + 1;
        tris[meshIndex * 6 + 4] = meshIndex * 4 + 3;
        tris[meshIndex * 6 + 5] = meshIndex * 4 + 2;

        meshIndex++;
    
        vertices[meshIndex * 4 + 0] = new Vector3(x, y - 1, z);
        vertices[meshIndex * 4 + 1] = new Vector3(x, y, z);
        vertices[meshIndex * 4 + 2] = new Vector3(x, y - 1, z + 1);
        vertices[meshIndex * 4 + 3] = new Vector3(x, y, z + 1);

        uv[meshIndex * 4 + 0] = new Vector2(percentWidth * sideX, percentHeight * sideY);
        uv[meshIndex * 4 + 2] = new Vector2(percentWidth * sideX + percentWidth, percentHeight * sideY);
        uv[meshIndex * 4 + 1] = new Vector2(percentWidth * sideX, percentHeight * sideY + percentHeight);
        uv[meshIndex * 4 + 3] = new Vector2(percentWidth * sideX + percentWidth, percentHeight * sideY + percentHeight);

        tris[meshIndex * 6 + 0] = meshIndex * 4 + 0;
        tris[meshIndex * 6 + 1] = meshIndex * 4 + 2;
        tris[meshIndex * 6 + 2] = meshIndex * 4 + 1;
        tris[meshIndex * 6 + 3] = meshIndex * 4 + 2;
        tris[meshIndex * 6 + 4] = meshIndex * 4 + 3;
        tris[meshIndex * 6 + 5] = meshIndex * 4 + 1;

        meshIndex++;
    
        vertices[meshIndex * 4 + 0] = new Vector3(x, y - 1, z + 1);
        vertices[meshIndex * 4 + 1] = new Vector3(x + 1, y - 1, z + 1);
        vertices[meshIndex * 4 + 2] = new Vector3(x, y, z + 1);
        vertices[meshIndex * 4 + 3] = new Vector3(x + 1, y, z + 1);

        uv[meshIndex * 4 + 0] = new Vector2(percentWidth * sideX, percentHeight * sideY);
        uv[meshIndex * 4 + 1] = new Vector2(percentWidth * sideX + percentWidth, percentHeight * sideY);
        uv[meshIndex * 4 + 2] = new Vector2(percentWidth * sideX, percentHeight * sideY + percentHeight);
        uv[meshIndex * 4 + 3] = new Vector2(percentWidth * sideX + percentWidth, percentHeight * sideY + percentHeight);

        tris[meshIndex * 6 + 0] = meshIndex* 4 + 1;
        tris[meshIndex * 6 + 1] = meshIndex* 4 + 2;
        tris[meshIndex * 6 + 2] = meshIndex* 4 + 0;
        tris[meshIndex * 6 + 3] = meshIndex* 4 + 1;
        tris[meshIndex * 6 + 4] = meshIndex* 4 + 3;
        tris[meshIndex * 6 + 5] = meshIndex* 4 + 2;

        meshIndex++;
    
        vertices[meshIndex * 4 + 0] = new Vector3(x, y - 1, z);
        vertices[meshIndex * 4 + 1] = new Vector3(x + 1, y - 1, z);
        vertices[meshIndex * 4 + 2] = new Vector3(x, y, z);
        vertices[meshIndex * 4 + 3] = new Vector3(x + 1, y, z);

        uv[meshIndex * 4 + 0] = new Vector2(percentWidth * sideX, percentHeight * sideY);
        uv[meshIndex * 4 + 1] = new Vector2(percentWidth * sideX + percentWidth, percentHeight * sideY);
        uv[meshIndex * 4 + 2] = new Vector2(percentWidth * sideX, percentHeight * sideY + percentHeight);
        uv[meshIndex * 4 + 3] = new Vector2(percentWidth * sideX + percentWidth, percentHeight * sideY + percentHeight);

        tris[meshIndex * 6 + 0] = meshIndex * 4 + 0;
        tris[meshIndex * 6 + 1] = meshIndex * 4 + 2;
        tris[meshIndex * 6 + 2] = meshIndex * 4 + 1;
        tris[meshIndex * 6 + 3] = meshIndex * 4 + 2;
        tris[meshIndex * 6 + 4] = meshIndex * 4 + 3;
        tris[meshIndex * 6 + 5] = meshIndex * 4 + 1;

        // Apply mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        o.GetComponent<MeshFilter>().mesh = mesh;
    }
}
