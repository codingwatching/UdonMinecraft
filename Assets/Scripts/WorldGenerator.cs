using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using UnityEngine.UI;

public class WorldGenerator : UdonSharpBehaviour
{
    [SerializeField] GameObject chunkPrefab;
    [SerializeField] Material blockTextures;
    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject floorPrefab;
    [SerializeField] GameObject spawnButton;
    [SerializeField] Text progressText;

    byte CHUNK_COUNT = 24; // Amount of chunks per axis
    byte CHUNK_SIZE = 4;
    byte CHUNK_HEIGHT = 32;

    GameObject[][] chunks;
    bool[][] chunkLoaded;
    bool[][] meshLoaded;
    MeshFilter[][] meshFilter;
    MeshCollider[][] meshCollider;
    byte[][][] blocks;
    Vector3[] vertices = new Vector3[2048 * 4 * 4];
    Vector2[] uv = new Vector2[2048 * 4 * 4];
    int[] tris = new int[2048 * 4 * 6];

    bool chunkLoadingStarted = false;
    bool chunksReady = false;

    public void startSetup()
    {
        if(getTotalChunksWidth() > byte.MaxValue)
        {
            Debug.LogError("Map size is too big for byte-based terrain system");
            return;
        }

        // Setup all arrays
        fillBlockTable();
        initializeBlockArray();
        fillBlockArray();

        chunks = new GameObject[CHUNK_COUNT][];
        chunkLoaded = new bool[CHUNK_COUNT][];
        meshLoaded = new bool[CHUNK_COUNT][];
        meshFilter = new MeshFilter[CHUNK_COUNT][];
        meshCollider = new MeshCollider[CHUNK_COUNT][];

        for (byte x = 0; x < CHUNK_COUNT; x++)
        {
            chunks[x] = new GameObject[CHUNK_COUNT];
            chunkLoaded[x] = new bool[CHUNK_COUNT];
            meshLoaded[x] = new bool[CHUNK_COUNT];
            meshFilter[x] = new MeshFilter[CHUNK_COUNT];
            meshCollider[x] = new MeshCollider[CHUNK_COUNT];

            for (byte y = 0; y < CHUNK_COUNT; y++)
            {
                GameObject chunk = VRCInstantiate(chunkPrefab);

                chunks[x][y] = chunk;
                chunkLoaded[x][y] = false;
                meshLoaded[x][y] = false;
                meshFilter[x][y] = chunk.GetComponent<MeshFilter>();
                meshCollider[x][y] = chunk.GetComponent<MeshCollider>();
            }
        }

        // Setup world walls
        GameObject floor = VRCInstantiate(floorPrefab);
        floor.transform.localScale = new Vector3(CHUNK_SIZE * CHUNK_COUNT, 1, CHUNK_SIZE * CHUNK_COUNT);
        GameObject wallFront = VRCInstantiate(wallPrefab);
        GameObject wallBack = VRCInstantiate(wallPrefab);
        GameObject wallLeft = VRCInstantiate(wallPrefab);
        GameObject wallRight = VRCInstantiate(wallPrefab);
        wallFront.transform.localScale = wallBack.transform.localScale = wallLeft.transform.localScale = wallRight.transform.localScale = new Vector3(CHUNK_SIZE * CHUNK_COUNT, CHUNK_HEIGHT + 10, 1);
        wallFront.transform.position = new Vector3(CHUNK_SIZE * CHUNK_COUNT, 0, 0);
        wallBack.transform.position = new Vector3(0, 0, CHUNK_SIZE * CHUNK_COUNT);
        wallRight.transform.position = new Vector3(CHUNK_SIZE * CHUNK_COUNT, 0, CHUNK_SIZE * CHUNK_COUNT);
        wallFront.transform.Rotate(new Vector3(0, -180, 0));
        wallLeft.transform.Rotate(new Vector3(0, -90, 0));
        wallRight.transform.Rotate(new Vector3(0, 90, 0));

        chunkLoadingStarted = true;
    }

    // Chunk loading
    void Update()
    {
        if (chunkLoadingStarted)
        {   
            for (byte x = 0; x < CHUNK_COUNT; x++)
            {
                for (byte y = 0; y < CHUNK_COUNT; y++)
                {
                    if (!chunkLoaded[x][y])
                    {
                        progressText.text = "Loading chunk data.. (" + (x * CHUNK_COUNT + y) + "/" + (CHUNK_COUNT * CHUNK_COUNT) + ")";
                        loadChunkXY(x, y);
                        chunkLoaded[x][y] = true;
                        return;
                    }
                }
            }

            for (byte x = 0; x < CHUNK_COUNT; x++)
            {
                for (byte y = 0; y < CHUNK_COUNT; y++)
                {
                    if (!meshLoaded[x][y])
                    {
                        progressText.text = "Generating terrain.. (" + (x * CHUNK_COUNT + y) + "/" + (CHUNK_COUNT * CHUNK_COUNT) + ")";
                        setBlockTextureMaterial(chunks[x][y]);
                        buildMesh(x, y);
                        meshLoaded[x][y] = true;
                        return;
                    }
                }
            }

            // TODO instance catchup

            chunkLoadingStarted = false;
            chunksReady = true;

            progressText.text = "";
            spawnButton.SetActive(true);
        }
    }

    void initializeBlockArray()
    {
        blocks = new byte[CHUNK_SIZE * CHUNK_COUNT][][];
    }

    void fillBlockArray()
    {
        for (byte x = 0; x < CHUNK_SIZE * CHUNK_COUNT; x++)
        {
            blocks[x] = new byte[CHUNK_HEIGHT][];

            for (byte y = 0; y < CHUNK_HEIGHT; y++)
            {
                blocks[x][y] = new byte[CHUNK_SIZE * CHUNK_COUNT];
            }
        }
    }

    void setBlockTextureMaterial(GameObject chunk)
    {
        chunk.GetComponent<Renderer>().material = blockTextures;
    }

    // Chunks
   void loadChunkXY(byte x, byte y)
    {
        chunks[x][y].transform.position = new Vector3(CHUNK_SIZE * x, 0, CHUNK_SIZE * y);
        loadChunk(x, y);
    }

    void loadChunk(byte chunkX, byte chunkY)
    {
        // Chunk population
        for (byte x = (byte) (CHUNK_SIZE * chunkX); x < CHUNK_SIZE * chunkX + CHUNK_SIZE; x++)
        {
            for (byte y = 0; y < CHUNK_HEIGHT; y++)
            {
                for (byte z = (byte) (CHUNK_SIZE * chunkY); z < CHUNK_SIZE * chunkY + CHUNK_SIZE; z++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.05f, z * 0.05f) * 5f;
                    float treeNoise = Mathf.PerlinNoise(x * 0.5f, z * 0.5f);
                    byte height = (byte) (5 + (int) noise);
                    if (height == y)
                    {
                        blocks[x][y][z] = 2;

                        if (treeNoise > 0.9f)
                        {
                            makeTree(x, y, z);
                        }
                    }
                    else if (y == 0)
                    {
                        blocks[x][y][z] = 1;
                    }
                    else if(y < height && y > height - 2)
                    {
                        blocks[x][y][z] = 5;
                    }
                    else if(y < height)
                    {
                        blocks[x][y][z] = 3;
                    }
                    /*if (y < height)
                    {
                        blocks[x][y][z] = Random.Range(0, blockTable.Length);
                    }*/
                }
            }
        }
    }

    void buildMesh(byte chunkX, byte chunkY)
    {
        int meshIndex = 0;

        for (byte x = 0; x < CHUNK_SIZE; x++)
        {
            for (byte y = 0; y < CHUNK_HEIGHT - 1; y++)
            {
                for (byte z = 0; z < CHUNK_SIZE; z++)
                {
                    byte blockX = (byte) (CHUNK_SIZE * chunkX + x), blockY = y, blockZ = (byte) (CHUNK_SIZE * chunkY + z);
                    byte blockType = blocks[blockX][blockY][blockZ];

                    if (blockType > 0)
                    {
                        // Percent width and height of texture file
                        float percentWidth = 0.02083333f;
                        float percentHeight = 0.0212766f;

                        byte[] texture = getTextureOffsets(blockType);
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

                        // UP
                        if (blockY + 1 >= CHUNK_HEIGHT - 1 || transparency == 2 || getTextureOffsets(blocks[blockX][blockY + 1][blockZ])[0] != transparency) // If adjecent block is outside chunk or type is other transparency layer


                        {
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
                        }

                        // DOWN
                        if (blockY - 1 >= 0 && (transparency == 2 || getTextureOffsets(blocks[blockX][blockY - 1][blockZ])[0] != transparency)) // Adjecent block is inside chunk AND type is different transparency layer
                        {
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
                        }

                        // RIGHT
                        if (blockX + 1 <= CHUNK_SIZE * CHUNK_COUNT - 1 && (transparency == 2 || getTextureOffsets(blocks[blockX + 1][blockY][blockZ])[0] != transparency))
                        {
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
                        }

                        // LEFT
                        if (blockX - 1 >= 0 && (transparency == 2 || getTextureOffsets(blocks[blockX - 1][blockY][blockZ])[0] != transparency))
                        {
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
                        }

                        // BACK
                        if (blockZ + 1 <= CHUNK_SIZE * CHUNK_COUNT - 1 && (transparency == 2 || getTextureOffsets(blocks[blockX][blockY][blockZ + 1])[0] != transparency))
                        {
                            vertices[meshIndex * 4 + 0] = new Vector3(x, y - 1, z + 1);
                            vertices[meshIndex * 4 + 1] = new Vector3(x + 1, y - 1, z + 1);
                            vertices[meshIndex * 4 + 2] = new Vector3(x, y, z + 1);
                            vertices[meshIndex * 4 + 3] = new Vector3(x + 1, y, z + 1);

                            uv[meshIndex * 4 + 0] = new Vector2(percentWidth * sideX, percentHeight * sideY);
                            uv[meshIndex * 4 + 1] = new Vector2(percentWidth * sideX + percentWidth, percentHeight * sideY);
                            uv[meshIndex * 4 + 2] = new Vector2(percentWidth * sideX, percentHeight * sideY + percentHeight);
                            uv[meshIndex * 4 + 3] = new Vector2(percentWidth * sideX + percentWidth, percentHeight * sideY + percentHeight);

                            tris[meshIndex * 6 + 0] = meshIndex * 4 + 1;
                            tris[meshIndex * 6 + 1] = meshIndex * 4 + 2;
                            tris[meshIndex * 6 + 2] = meshIndex * 4 + 0;
                            tris[meshIndex * 6 + 3] = meshIndex * 4 + 1;
                            tris[meshIndex * 6 + 4] = meshIndex * 4 + 3;
                            tris[meshIndex * 6 + 5] = meshIndex * 4 + 2;

                            meshIndex++;
                        }

                        // FRONT
                        if (blockZ - 1 >= 0 && (transparency == 2 || getTextureOffsets(blocks[blockX][blockY][blockZ - 1])[0] != transparency))
                        {
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

                            meshIndex++;
                        }
                    }
                }
            }
        }

        // Crunch arrays
        Vector3[] finalVertices = new Vector3[meshIndex * 4];
        for (int i = 0; i < finalVertices.Length; i++)
        {
            finalVertices[i] = vertices[i];
        }

        Vector2[] finalUv = new Vector2[meshIndex * 4];
        for (int i = 0; i < finalUv.Length; i++)
        {
            finalUv[i] = uv[i];
        }
        
        int[] finalTris = new int[meshIndex * 6];
        for (int i = 0; i < finalTris.Length; i++)
        {
            finalTris[i] = tris[i];
        }

        // Apply mesh
        Mesh mesh = new Mesh();
        mesh.vertices = finalVertices;
        mesh.uv = finalUv;
        mesh.triangles = finalTris;
        mesh.RecalculateNormals();

        meshFilter[chunkX][chunkY].mesh = mesh;
        meshCollider[chunkX][chunkY].sharedMesh = mesh;

        Debug.Log("Chunk " + chunkX + " " + chunkY + " rendered with " + finalVertices.Length + " verts and " + finalTris.Length + " tris");
    }
    
    void rerenderAdjacentChunks(byte blockX, byte blockZ)
    {
        byte[] chunk = getBlockChunk(blockX, blockZ);
        byte[] chunkRight = getBlockChunk(blockX + 1, blockZ);
        byte[] chunkLeft = getBlockChunk(blockX - 1, blockZ);
        byte[] chunkBack = getBlockChunk(blockX, blockZ + 1);
        byte[] chunkFront = getBlockChunk(blockX, blockZ - 1);

        if (chunkRight != null && !sameChunk(chunk, chunkRight))
        {
            buildMesh(chunkRight[0], chunkRight[1]);
        }
        if (chunkLeft != null && !sameChunk(chunk, chunkLeft))
        {
            buildMesh(chunkLeft[0], chunkLeft[1]);
        }
        if (chunkBack != null && !sameChunk(chunk, chunkBack))
        {
            buildMesh(chunkBack[0], chunkBack[1]);
        }
        if (chunkFront != null && !sameChunk(chunk, chunkFront))
        {
            buildMesh(chunkFront[0], chunkFront[1]);
        }
    }

    bool sameChunk(byte[] chunk1, byte[] chunk2)
    {
        return chunk1[0] == chunk2[0] && chunk1[1] == chunk2[1];
    }

    byte[] getBlockChunk(int blockX, int blockZ)
    {
        if(blockX < 0 || blockZ < 0 || blockX > CHUNK_SIZE * CHUNK_COUNT - 1 || blockZ > CHUNK_SIZE * CHUNK_COUNT - 1)
        {
            return null;
        }
        return new byte[2] { (byte) ((blockX - (blockX % CHUNK_SIZE)) / CHUNK_SIZE), (byte) ((blockZ - (blockZ % CHUNK_SIZE)) / CHUNK_SIZE) };
    }

    // Public functions
    public bool isInBounds(int blockX, int blockY, int blockZ)
    {
        byte[] chunk = getBlockChunk(blockX, blockZ);
        if (chunk == null || chunk[0] < 0 || chunk[0] > CHUNK_COUNT - 1 || chunk[1] < 0 || chunk[1] > CHUNK_COUNT - 1 || blockX < 0 || blockZ < 0 || blockX > CHUNK_SIZE * CHUNK_COUNT - 1 || blockZ > CHUNK_SIZE * CHUNK_COUNT - 1 || blockY > CHUNK_HEIGHT - 1 || blockY < 0)
        {
            return false;
        }
        return true;
    }
    
    void makeTree(byte x, byte y, byte z)
    {
        for (int treeX = -2; treeX < 3; treeX++)
        {
            for (int treeY = 3; treeY < 5; treeY++)
            {
                for (int treeZ = -2; treeZ < 3; treeZ++)
                {
                    if (isInBounds(x + treeX, y + treeY, z + treeZ))
                    {
                        blocks[x + treeX][y + treeY][z + treeZ] = 8;
                    }
                }
            }
        }
        for (int treeX = -1; treeX < 2; treeX++)
        {
            for (int treeY = 5; treeY < 6; treeY++)
            {
                for (int treeZ = -1; treeZ < 2; treeZ++)
                {
                    if (isInBounds(x + treeX, y + treeY, z + treeZ))
                    {
                        blocks[x + treeX][y + treeY][z + treeZ] = 8;
                    }
                }
            }
        }
        if (isInBounds(x - 1, y, z - 1) && isInBounds(x + 1, y, z + 1))
        {
            blocks[x][y + 6][z] = 8;
            blocks[x - 1][y + 6][z] = 8;
            blocks[x][y + 6][z - 1] = 8;
            blocks[x][y + 6][z + 1] = 8;
            blocks[x + 1][y + 6][z] = 8;
        }
        blocks[x][y + 1][z] = 7;
        blocks[x][y + 2][z] = 7;
        blocks[x][y + 3][z] = 7;
        blocks[x][y + 4][z] = 7;
    }

    public byte getBlockType(int blockX, int blockY, int blockZ)
    {
        if (!chunksReady || !isInBounds((byte) blockX, (byte)blockY, (byte) blockZ))
        {
            return 0;
        }

        return blocks[blockX][blockY][blockZ];
    }

    public void addBlock(byte blockX, byte blockY, byte blockZ, byte type)
    {
        if (!chunksReady)
        {
            return;
        }
        int blockType = blocks[blockX][blockY][blockZ];
        if (blockType == 1)
        {
            return;
        }
        if(type == 0)
        {
            removeBlock(blockX, blockY, blockZ);
            return;
        }
        blocks[blockX][blockY][blockZ] = type;

        byte[] chunk = getBlockChunk(blockX, blockZ);
        buildMesh(chunk[0], chunk[1]);
    }
    
    public void removeBlock(byte blockX, byte blockY, byte blockZ)
    {
        int blockType = blocks[blockX][blockY][blockZ];
        if (!chunksReady || blockType < 2)
        {
            return;
        }
        blocks[blockX][blockY][blockZ] = 0;

        byte[] chunk = getBlockChunk(blockX, blockZ);
        buildMesh(chunk[0], chunk[1]);
        
        rerenderAdjacentChunks(blockX, blockZ);
    }

    public GameObject getChunk(byte chunkX, byte chunkY)
    {
        if (chunkX >= 0 && chunkX <= CHUNK_COUNT - 1 && chunkY >= 0 && chunkY <= CHUNK_COUNT - 1)
        {
            return chunks[chunkX][chunkY];
        }
        else
        {
            return null;
        }
    }

    public int[] positionToBlockPosition(Vector3 position)
    {
        return new int[] { Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y) + 1, Mathf.FloorToInt(position.z) };
    }

    public int getTotalChunksWidth()
    {
        return CHUNK_COUNT * CHUNK_SIZE;
    }

    public byte getChunkHeight()
    {
        return CHUNK_HEIGHT;
    }

    public byte getXZHeight(byte x, byte z)
    {
        for(byte y = (byte) (CHUNK_HEIGHT - 1); y > 0; y--)
        {
            if(blocks[x][y][z] != 0)
            {
                return y;
            }
        }
        return 0;
    }

    // Blocks
    byte[][] blockTable;
    bool blockTableFilled = false;

    public void fillBlockTable()
    {
        if(blockTableFilled)
        {
            return;
        }

        blockTable = new byte[][]
        {
            // transparency, topX, topY, sideX, sideY, bottomX, bottomY
            new byte[] { 1, 0, 0 }, // 0 = air
            new byte[] { 0, 1, 45 }, // 1 = bedrock
            new byte[] { 0, 0, 46, 3, 46, 2, 46 }, // 2 = grass
            new byte[] { 0, 1, 46 }, // 3 = stone
            new byte[] { 0, 0, 45 }, // 4 = cobblestone
            new byte[] { 0, 2, 46 }, // 5 = dirt
            new byte[] { 0, 4, 46 }, // 6 = wood
            new byte[] { 0, 20, 30, 23, 30, 20, 30 }, // 7 = log
            new byte[] { 2, 4, 43 }, // 8 = leaves
            new byte[] { 0, 7, 46 }, // 9 = brick
            new byte[] { 0, 9, 46, 8, 46, 10, 46 }, // 10 = tnt
            new byte[] { 0, 2, 45 }, // 11 = sand
            new byte[] { 0, 3, 45 }, // 12 = gravel
            new byte[] { 0, 6, 45 }, // 13 = ironblock
            new byte[] { 0, 7, 45 }, // 14 = goldblock
            new byte[] { 0, 8, 45 }, // 15 = diamondblock
            new byte[] { 0, 9, 45 }, // 16 = emeraldblock
            new byte[] { 0, 10, 45 }, // 17 = redstoneblock
            new byte[] { 0, 0, 44 }, // 18 = goldore
            new byte[] { 0, 1, 44 }, // 19 = ironore
            new byte[] { 0, 2, 44 }, // 20 = coalore
            new byte[] { 0, 4, 46, 3, 44, 4, 46 }, // 21 = bookshelf
            new byte[] { 0, 4, 44 }, // 22 = mossycobblestone
            new byte[] { 0, 5, 44 }, // 23 = obsidian
            new byte[] { 0, 0, 43 }, // 24 = sponge
            new byte[] { 3, 1, 43 }, // 25 = glass
            new byte[] { 0, 2, 43 }, // 26 = diamondore
            new byte[] { 0, 3, 43 }, // 27 = redstoneore
            new byte[] { 0, 5, 43 }, // 28 = denseleaves
            new byte[] { 0, 6, 43 }, // 29 = stonewall
            new byte[] { 0, 0, 42 }, // 30 = wool
            new byte[] { 2, 1, 42 }, // 31 = spawner
            new byte[] { 0, 2, 42 }, // 32 = snow
            new byte[] { 0, 3, 42 }, // 33 = ice
            new byte[] { 0, 2, 42, 4, 42, 2, 46 }, // 34 = snowgrass
            new byte[] { 0, 17, 40, 19, 40, 17, 40 }, // 35 = chest
        };
        blockTableFilled = true;
    }

    public byte[] getTextureOffsets(byte blockType)
    {
        return blockTable[blockType];
    }
}
