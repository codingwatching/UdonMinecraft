
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SyncedBlock : UdonSharpBehaviour
{
    [SerializeField] BlockSync sync; // Synced block manager
    [UdonSynced(UdonSyncMode.None)] string broadcastBlock = ""; // Global serialized block state

    // Local representation
    string localBlock = "";
    public byte x = 0, y = 0, z = 0, type = 0;

    public void setBlock(byte x, byte y, byte z, byte type)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.type = type;
        broadcastBlock = serialize(x, y, z, type);
    }

    // On block broadcast change local block state
    public override void OnPreSerialization()
    {
        if (broadcastBlock != localBlock)
        {
            localBlock = broadcastBlock;
            sync.applyBlock(this);
        }
    }

    // On receive block change local block state
    public override void OnDeserialization()
    {
        if (broadcastBlock != localBlock)
        {
            localBlock = broadcastBlock;
            deserialize(broadcastBlock);
            sync.applyBlock(this);
        }
    }

    // Pack bytes into string efficiently
    string serialize(byte x, byte y, byte z, byte type)
    {
        byte[] data = new byte[4] { x, y, z, type };
        return System.Convert.ToBase64String(data);
    }

    // Unpack bytes from string into local state
    void deserialize(string dataString)
    {
        byte[] data = System.Convert.FromBase64String(dataString);
        x = data[0];
        y = data[1];
        z = data[2];
        type = data[3];
    }
}
