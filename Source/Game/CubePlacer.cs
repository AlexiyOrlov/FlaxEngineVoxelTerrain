using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using FlaxEngine;
using FlaxEngine.Utilities;

namespace Game.Game;

public class CubePlacer : Script
{
    enum VoxelType
    {
        Air,
        Stone,
        Dirt
    }
    
    public MaterialBase StoneMaterial,GrassMaterial;
    ConcurrentDictionary<VoxelType,ChunkPart> chunkParts = new ConcurrentDictionary<VoxelType, ChunkPart>();
    List<Model> models = new List<Model>();

    private const int ChunkHeight = 16;
    
    /// <inheritdoc/>
    public override void OnStart()
    {
        for (int x = 0; x < 128; x++)
        {
            for (int y = 0; y < ChunkHeight; y++)
            {
                for (int z = 0; z < 128; z++)
                {
                    VoxelType voxelType=DetermineVoxelType(x,y,z);
                    if(voxelType!=VoxelType.Air)
                    {
                        ChunkPart chunkPart=chunkParts.GetOrAdd(voxelType,new ChunkPart());
                        for (int fIndex = 0; fIndex < 6; fIndex++)
                        {
                            AddFaceData(x, y, z, fIndex, chunkPart.vertices, chunkPart.triangles, chunkPart.uvs);
                        }
                    }
                }
            }
        }
        
        foreach (var chunkPart in chunkParts)
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
                    childModel.SetMaterial(0, StoneMaterial);
                    break;    
                case VoxelType.Dirt:
                    childModel.SetMaterial(0, GrassMaterial);
                    break;
            }
            models.Add(model);
            actor.Parent = Actor;
            actor.Name=chunkPart.Key.ToString();
        }
    }
    
    /// <inheritdoc/>
    public override void OnEnable()
    {
        // Here you can add code that needs to be called when script is enabled (eg. register for events)
    }

    /// <inheritdoc/>
    public override void OnDisable()
    {
        // Here you can add code that needs to be called when script is disabled (eg. unregister from events)
    }

    /// <inheritdoc/>
    public override void OnUpdate()
    {
        // Here you can add code that needs to be called every frame
    }

    public override void OnDestroy()
    {
        models.ForEach(model => Destroy(model));
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
}