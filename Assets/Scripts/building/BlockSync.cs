
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class BlockSync : UdonSharpBehaviour
{
    [SerializeField] WorldGenerator generator;
    [SerializeField] GameObject[] syncBlocks;
    [SerializeField] GameObject[] inventoryItems;
    [SerializeField] GameObject spectatorWarning;
    bool spectator = false;

    public void broadcastBlock(byte x, byte y, byte z, byte type)
    {
        int blockIndex = getOwnedOrLowerReleasedIndex();

        if(blockIndex <= syncBlocks.Length - 1) // Check if player has a synced gameobject to talk to
        {
            GameObject block = syncBlocks[blockIndex];
            SyncedBlock syncedBlock = block.GetComponent<SyncedBlock>();

            Networking.SetOwner(Networking.LocalPlayer, block);
            syncedBlock.setBlock(x, y, z, type);

            if (VRCPlayerApi.GetPlayerCount() == 1) // Force block place in solo world
            {
                syncedBlock.OnPreSerialization();
            }
        }
    }

    public void applyBlock(SyncedBlock block)
    {
        generator.addBlock(block.x, block.y, block.z, block.type);

        // Prevent players from clipping the chunk mesh
        if (block.type != 0)
        {
            Vector3 blockPos = new Vector3(block.x + 0.5f, block.y - 0.5f, block.z + 0.5f);
            if (Vector3.Distance(blockPos, Networking.LocalPlayer.GetPosition()) < 0.75f)
            {
                Networking.LocalPlayer.TeleportTo(Networking.LocalPlayer.GetPosition() + new Vector3(0, 1f, 0), Networking.LocalPlayer.GetRotation()); // Teleport up to prevent falling through map
            }

            if (Vector3.Distance(blockPos, Networking.LocalPlayer.GetPosition() + new Vector3(0, 1.75f, 0)) < 0.75f)
            {
                Networking.LocalPlayer.TeleportTo(Networking.LocalPlayer.GetPosition() + new Vector3(0, 2f, 0), Networking.LocalPlayer.GetRotation()); // Teleport up to prevent placing block in head
            }
        }
    }

    // Magical function to get each player their own index in an array (the actual index may change but the player will always have a unique index)
    public int getOwnedOrLowerReleasedIndex()
    {
        int occupiedIndexes = 0;
        for (int index = 0; index < Networking.LocalPlayer.playerId; index++)
        {
            if (VRCPlayerApi.GetPlayerById(index) != null)
            {
                occupiedIndexes++;
            }
        }
        return occupiedIndexes;
    }

    // Assign spectator mode to any players that overflow the array of synced blocks
    void Start()
    {
        if(Networking.LocalPlayer == null) return;

        int blockIndex = getOwnedOrLowerReleasedIndex();
        if (blockIndex > syncBlocks.Length - 1)
        {
            spectator = true;
            spectatorWarning.SetActive(true);
            foreach(GameObject item in inventoryItems)
            {
                item.SetActive(false);
            }
        }
    }

    // Take a player out of spectator mode if a player leaves
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if(spectator)
        {
            int blockIndex = getOwnedOrLowerReleasedIndex();

            if (blockIndex <= syncBlocks.Length - 1)
            {
                spectator = false;
                spectatorWarning.SetActive(false);
                foreach (GameObject item in inventoryItems)
                {
                    item.SetActive(true);
                }
            }
        }
    }
}
