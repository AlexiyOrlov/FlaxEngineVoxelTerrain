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
    
    /// <inheritdoc/>
    public override void OnStart()
    {
        Instance = this;
        for (int x = 0; x<ChunksX; x++)
        {
            for (int z = 0; z < ChunksZ; z++)
            {
                for (int y = 0; y < 1; y++)
                {
                    var position = new Int3(x,y,z);
                    Chunk chunk = new Chunk(position);
                    chunk.Initialize(Actor);
        
                    chunks.TryAdd(position,chunk);
                }
            }
        }
        
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
        // Here you can add code that needs to be called every frame
    }

    public override void OnDestroy()
    {
        chunks.ForEach(pair => pair.Value.models.ForEach(model => Destroy(model)));
    }


    

    public Int3 PosToChunkCoordinate(Float3 pos)
    {
        return new Int3(Mathf.FloorToInt(pos.X / 16f), Mathf.FloorToInt(pos.Y/ChunkHeight), Mathf.FloorToInt(pos.Z / 16f));
    }

    public Chunk GetChunkAt(Float3 globalPosition)
    {
        return chunks.GetValueOrDefault(PosToChunkCoordinate(globalPosition));
    }
}