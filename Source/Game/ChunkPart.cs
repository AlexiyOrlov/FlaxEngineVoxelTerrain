using System.Collections.Generic;
using FlaxEngine;

namespace Game.Game
{
    public struct ChunkPart
    {
        public List<Float3> vertices;
        public List<int> triangles ;
        public List<Vector2> uvs ;
        // public  Voxel[,,] voxels;
        // public VoxelType voxelType;

        public ChunkPart()
        {
            // voxelType = type;
            vertices = new List<Float3>();
            triangles = new List<int>();
            uvs = new List<Vector2>();
            // voxels = new Voxel[World.Instance.chunkSize, World.Instance.chunkSize, World.Instance.chunkSize];
        }
    }
}