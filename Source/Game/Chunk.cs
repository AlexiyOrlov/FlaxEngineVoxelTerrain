using System.Collections.Concurrent;
using System.Collections.Generic;
using FlaxEngine;

namespace Game.Game;

public class Chunk
{
    Int3 position;
    public List<Model> models = new List<Model>();
    public ConcurrentDictionary<VoxelType,ChunkPart> ChunkParts = new ConcurrentDictionary<VoxelType, ChunkPart>();
    private ConcurrentDictionary<Int3, VoxelType> voxelTypes = new();

    public Chunk(Int3 position)
    {
        this.position = position;
    }

    public void Initialize(Actor parent)
    {
        //generate voxel types
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < CubePlacer.ChunkHeight; y++)
            {
                for (int z = 0; z < 16; z++)
                {
                    Int3 voxelPosition =new Int3(position.X * 16 + x, position.Y * CubePlacer.ChunkHeight + y, position.Z * 16 + z);
                    VoxelType voxelType=DetermineVoxelType(voxelPosition.X,voxelPosition.Y,voxelPosition.Z);
                    
                    voxelTypes.TryAdd(new Int3(x,y,z),voxelType);
                }
            }
        }

        for (int xInChunk = 0; xInChunk < 16; xInChunk++)
        {
            for (int yInChunk = 0; yInChunk < CubePlacer.ChunkHeight; yInChunk++)
            {
                for (int zInChunk = 0; zInChunk < 16; zInChunk++)
                {
                    VoxelType voxelType = voxelTypes[new Int3(xInChunk, yInChunk, zInChunk)];
                    Int3 voxelPosition =new Int3(position.X * 16 + xInChunk, position.Y * CubePlacer.ChunkHeight + yInChunk, position.Z * 16 + zInChunk);
                    if(voxelType!=VoxelType.Air)
                    {
                        ChunkPart chunkPart=ChunkParts.GetOrAdd(voxelType,new ChunkPart());
                        for (int fIndex = 0; fIndex < 6; fIndex++)
                        {
                            AddFaceData(voxelPosition.X,voxelPosition.Y,voxelPosition.Z, fIndex, chunkPart.vertices, chunkPart.triangles, chunkPart.uvs);
                        }
                    }
                }
            }
        }
        
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
                    childModel.SetMaterial(0, CubePlacer.Instance.StoneMaterial);
                    break;    
                case VoxelType.Dirt:
                    childModel.SetMaterial(0,CubePlacer.Instance.GrassMaterial);
                    break;
            }
            models.Add(model);
            actor.Parent = parent;
            actor.Name=chunkPart.Key.ToString();
        }
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