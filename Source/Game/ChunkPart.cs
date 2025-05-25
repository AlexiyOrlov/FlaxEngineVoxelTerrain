using System.Collections.Generic;
using FlaxEngine;

namespace Game.Game
{
    public struct ChunkPart
    {
        public List<Float3> vertices;
        public List<int> triangles ;
        public List<Float2> uvs ;
        // public  Voxel[,,] voxels;

        public ChunkPart()
        {
            vertices = new List<Float3>();
            triangles = new List<int>();
            uvs = new List<Float2>();
            // voxels = new Voxel[World.Instance.chunkSize, World.Instance.chunkSize, World.Instance.chunkSize];
        }
    }
}