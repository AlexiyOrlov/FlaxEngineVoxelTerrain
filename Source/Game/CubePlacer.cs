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
    public MaterialBase StoneMaterial,GrassMaterial;
    ConcurrentDictionary<Int3,Chunk> chunks = new ConcurrentDictionary<Int3, Chunk>();

    public const int ChunkHeight = 16;
    public static CubePlacer Instance;
    [Tooltip("Generate N chunks along X")]
    public int ChunksX;
    [Tooltip("Generate N chunks along Z")]
    public int ChunksZ;
    public Actor Player;

    public int ChunkLoadRange = 16;
    [Tooltip("In chunks")]
    public int WorldHeight = 4;
    
    /// <inheritdoc/>
    public override void OnStart()
    {
        Instance = this;
        // for (int x = 0; x<ChunksX; x++)
        // {
        //     for (int z = 0; z < ChunksZ; z++)
        //     {
        //         for (int y = 0; y < 1; y++)
        //         {
        //             var position = new Int3(x,y,z);
        //             Chunk chunk = new Chunk(position);
        //             chunk.Initialize(Actor);
        //
        //             chunks.TryAdd(position,chunk);
        //         }
        //     }
        // }
        
        
        // var next=new Int3(1,0,0);
        // var nextChunk=new Chunk(next);
        // nextChunk.Initialize(Actor);
        // chunks.TryAdd(next,nextChunk);
        //
        // var last = new Int3(1, 0, 1);
        // var lastChunk=new Chunk(last);
        // lastChunk.Initialize(Actor);
        // chunks.TryAdd(last,lastChunk);
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
        var playerChunkPosition = PosToChunkCoordinate(Player.Position);
        LoadChunksAround(playerChunkPosition);
    }

    private void LoadChunksAround(Int3 position)
    {
        for (int cx = -ChunkLoadRange; cx <= ChunkLoadRange; cx++)
        {
            for (int cz = -ChunkLoadRange; cz <= ChunkLoadRange; cz++)
            {
                for (int cy = -3; cy <= 3; cy++)
                {
                    Int3 chunkPosition = new Int3(position.X + cx, Mathf.Clamp(position.Y + cy,0,WorldHeight), position.Z + cz);
                    if (!chunks.ContainsKey(chunkPosition))
                    {
                        Chunk chunk = new Chunk(chunkPosition);
                        chunk.Initialize(Actor);
                        chunks.TryAdd(chunkPosition, chunk);
                    }
                }
            }
        }
    }

    public override void OnDestroy()
    {
        chunks.ForEach(pair => pair.Value.models.ForEach(model => Destroy(model)));
    }


    

    public Int3 PosToChunkCoordinate(Float3 pos)
    {
        return new Int3(Mathf.FloorToInt(pos.X/100 / 16), Mathf.FloorToInt(pos.Y/100/ChunkHeight), Mathf.FloorToInt(pos.Z/100 / 16));
    }

    public Chunk GetChunkAt(Float3 globalPosition)
    {
        return chunks.GetValueOrDefault(PosToChunkCoordinate(globalPosition));
    }
}