using System.Collections.Concurrent;
using System.Collections.Generic;
using FlaxEngine;

namespace Game.Game;

public class Chunk
{
    Int3 _position;
    public List<Model> Models = new List<Model>();
    public ConcurrentDictionary<VoxelType,ChunkPart> ChunkParts = new ConcurrentDictionary<VoxelType, ChunkPart>();
    private ConcurrentDictionary<Int3, VoxelType> voxelTypes = new();

    public Chunk(Int3 position)
    {
        this._position = position;
    }

    public void Initialize(Actor parent)
    {
        //generate voxel types
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < World.ChunkHeight; y++)
            {
                for (int z = 0; z < 16; z++)
                {
                    Int3 voxelPosition =new Int3(_position.X * 16 + x, _position.Y * World.ChunkHeight + y, _position.Z * 16 + z);
                    VoxelType voxelType=DetermineVoxelType(voxelPosition.X,voxelPosition.Y,voxelPosition.Z);
                    
                    voxelTypes.TryAdd(new Int3(x,y,z),voxelType);
                }
            }
        }

        for (int xInChunk = 0; xInChunk < 16; xInChunk++)
        {
            for (int yInChunk = 0; yInChunk < World.ChunkHeight; yInChunk++)
            {
                for (int zInChunk = 0; zInChunk < 16; zInChunk++)
                {
                    VoxelType voxelType = voxelTypes[new Int3(xInChunk, yInChunk, zInChunk)];
                    Int3 voxelPosition =new Int3(_position.X * 16 + xInChunk, _position.Y * World.ChunkHeight + yInChunk, _position.Z * 16 + zInChunk);
                    if(voxelType!=VoxelType.Air)
                    {
                        ChunkPart chunkPart=ChunkParts.GetOrAdd(voxelType,new ChunkPart());
                        bool[] facesVisible = new bool[6];

                        // Check visibility for each face
                        facesVisible[0] = IsAir(xInChunk, yInChunk + 1, zInChunk); // Top
                        facesVisible[1] = IsAir(xInChunk, yInChunk - 1, zInChunk); // Bottom
                        facesVisible[2] = IsAir(xInChunk - 1, yInChunk, zInChunk); // Left
                        facesVisible[3] = IsAir(xInChunk + 1, yInChunk, zInChunk); // Right
                        facesVisible[4] = IsAir(xInChunk, yInChunk, zInChunk + 1); // Front
                        facesVisible[5] = IsAir(xInChunk, yInChunk, zInChunk - 1); // Back
                        for (int fIndex = 0; fIndex < 6; fIndex++)
                        {
                            if(facesVisible[fIndex])
                                AddFaceData(voxelPosition.X,voxelPosition.Y,voxelPosition.Z, fIndex, chunkPart.vertices, chunkPart.triangles, chunkPart.uvs);
                        }
                    }
                }
            }
        }
        
        Scripting.InvokeOnUpdate(() =>
        {
            foreach (var chunkPart in ChunkParts)
            {
                
                    Actor actor = new EmptyActor();
                    var model = Content.CreateVirtualAsset<Model>();
                    model.SetupLODs([1]);
                    model.LODs[0].Meshes[0].UpdateMesh(chunkPart.Value.vertices,chunkPart.Value.triangles,null,null,chunkPart.Value.uvs);
                    var childModel = actor.GetOrAddChild<StaticModel>();
                    childModel.Model = model;
                    childModel.LocalScale = new Float3(100);
                    switch (chunkPart.Key)
                    {
                        case VoxelType.Stone:
                            childModel.SetMaterial(0, World.Instance.StoneMaterial);
                            break;    
                        case VoxelType.Dirt:
                            childModel.SetMaterial(0,World.Instance.GrassMaterial);
                            break;
                    }
                    Models.Add(model);
                    actor.Parent = parent;
                    actor.Name=chunkPart.Key.ToString();
            }
        });
    }
    
    bool IsAir(int x, int y, int z)
    {
        if (x < 0 || x > 15 || y < 0 || y > World.ChunkHeight-1 || z < 0 || z > 15)
        {
            //TODO fix
            // Float3 globalPosition = new Float3(position.X + x, position.Y + y, position.Z + z);
            // Chunk neighborChunk = CubePlacer.Instance.GetChunkAt(globalPosition);
            // if (neighborChunk != null)
            // {
            //     Int3 neighborVoxelPosition = new Int3(x<0?15:x>15?0:x,y<0 ? CubePlacer.ChunkHeight-1: y>CubePlacer.ChunkHeight-1?0:y, z<0?15:z>15?0:z);
            //     return neighborChunk.voxelTypes[neighborVoxelPosition]==VoxelType.Air;
            // }
            return true;
        }
        return voxelTypes[new Int3(x,y,z)]==VoxelType.Air;
    }
    
    VoxelType DetermineVoxelType(int x, int y, int z)
    {
        float noise = Noise.CalcPixel3D(x, y, z, 0.03f);
        switch (noise)
        {
            case < 98:
                return VoxelType.Stone;
            case < 196:
                return VoxelType.Dirt;
            default:
                return VoxelType.Air;
        }
    }
    
    private void AddFaceData(int x, int y, int z, int faceIndex,List<Float3> vertices,List<int> triangles,List<Float2> uvs)
    {
        if (faceIndex == 0) // Top Face
        {
            vertices.Add(new Float3(x,     y + 1, z    ));
            vertices.Add(new Vector3(x,     y + 1, z + 1)); 
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));
            vertices.Add(new Vector3(x + 1, y + 1, z    )); 
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 1));
        }

        if (faceIndex == 1) // Bottom Face
        {
            vertices.Add(new Vector3(x,     y, z    ));
            vertices.Add(new Vector3(x + 1, y, z    )); 
            vertices.Add(new Vector3(x + 1, y, z + 1));
            vertices.Add(new Vector3(x,     y, z + 1)); 
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(1, 0));
        }

        if (faceIndex == 2) // Left Face
        {
            vertices.Add(new Vector3(x, y,     z    ));
            vertices.Add(new Vector3(x, y,     z + 1));
            vertices.Add(new Vector3(x, y + 1, z + 1));
            vertices.Add(new Vector3(x, y + 1, z    ));
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(0, 1));
        }

        if (faceIndex == 3) // Right Face
        {
            vertices.Add(new Vector3(x + 1, y,     z + 1));
            vertices.Add(new Vector3(x + 1, y,     z    ));
            vertices.Add(new Vector3(x + 1, y + 1, z    ));
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(1, 0));
        }

        if (faceIndex == 4) // Front Face
        {
            vertices.Add(new Vector3(x,     y,     z + 1));
            vertices.Add(new Vector3(x + 1, y,     z + 1));
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));
            vertices.Add(new Vector3(x,     y + 1, z + 1));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(1, 1));
        }

        if (faceIndex == 5) // Back Face
        {
            vertices.Add(new Vector3(x + 1, y,     z    ));
            vertices.Add(new Vector3(x,     y,     z    ));
            vertices.Add(new Vector3(x,     y + 1, z    ));
            vertices.Add(new Vector3(x + 1, y + 1, z    ));
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(0, 0));
        
        }
            
        int vertCount = vertices.Count;

        // First triangle
        triangles.Add(vertCount - 4);
        triangles.Add(vertCount - 3);
        triangles.Add(vertCount - 2);
        
        // Second triangle
        triangles.Add(vertCount - 4);
        triangles.Add(vertCount - 2);
        triangles.Add(vertCount - 1);
    }

}