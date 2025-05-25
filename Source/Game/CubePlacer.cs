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
    
    /// <inheritdoc/>
    public override void OnStart()
    {
        var model = Content.CreateVirtualAsset<Model>();
        model.SetupLODs([1]);
        CreateMesh(model.LODs[0].Meshes[0]);

        var childModel = Actor.GetOrAddChild<StaticModel>();
        childModel.Model = model;
        childModel.LocalScale = new Float3(100);
        childModel.SetMaterial(0, GrassMaterial);
        models.Add(model);
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

    private void CreateMesh(Mesh mesh)
    {
        var vertices = new List<Float3>();
        var triangles =new List<int>();
        var uvs = new List<Float2>();
        List<Float3> normals = new ();
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                for (int z = 0; z < 64; z++)
                {
                    VoxelType voxelType = DetermineVoxelType(x, y, z);
                    if (voxelType != VoxelType.Air)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            AddFaceData(x, y, z, i, vertices, triangles, uvs);
                        }
                    }
                }
            }
        }
        if(vertices.Count>0)
            mesh.UpdateMesh(vertices.ToArray(), triangles.ToArray(),null,null,uvs.ToArray());
        else
        {
            Debug.Log("No vertices");
        }
    }
    
    private void AddFaceData(int x, int y, int z, int faceIndex,List<Float3> vertices,List<int> triangles,List<Float2> uvs)
    {
        // Based on faceIndex, determine vertices and triangles
        // Add vertices and triangles for the visible face
        // Calculate and add corresponding UVs
        // float3 pos=new float3(x,y,z);
        // ChunkPart chunkPart=positionToChunkParts[pos];
        // x+=(int)position.x;
        // y+=(int)position.y;
        // z+=(int)position.z;
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
            case var f when f< 64:
                return VoxelType.Stone;
            case var f when f < 196:
                return VoxelType.Dirt;
            default:
                return VoxelType.Air;
        }
    }
}