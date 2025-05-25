using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using FlaxEngine;
using FlaxEngine.Utilities;

namespace Game.Game;

public class World : Script
{
    public MaterialBase StoneMaterial,GrassMaterial;
    ConcurrentDictionary<Int3,Chunk> _chunks = new ConcurrentDictionary<Int3, Chunk>();

    public int ChunkHeight = 16;
    public static World Instance;
    [Tooltip("Generate N chunks along X")]
    public int ChunksX;
    [Tooltip("Generate N chunks along Z")]
    public int ChunksZ;
    public Actor Player;

    public int ChunkLoadRange = 16;
    [Tooltip("In chunks")]
    public int WorldHeight = 4;
    private bool _runGenerationThread = true;
    private long _genThreadLabel=-1;
    
    /// <inheritdoc/>
    public override void OnStart()
    {
        Instance = this;
        _genThreadLabel=JobSystem.Dispatch(arg0 =>
        {
            while (_runGenerationThread)
            {
                var playerChunkPosition = PosToChunkCoordinate(Player.Position);
                LoadChunksAround(playerChunkPosition);
                UnloadChunksAround(playerChunkPosition);
            }
        });
        // MakeChunksTest(PosToChunkCoordinate(Player.Position));
    }

    private void MakeChunksTest(Int3 at)
    {
        for (int i = 0; i < ChunksX; i++)
        {
            for (int j = 0; j < ChunksZ; j++)
            {
                for (int k = 0; k < WorldHeight; k++)
                {
                    CreateChunk(at,i,k,j);
                }
            }
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
        
    }

    private void LoadChunksAround(Int3 position)
    {
        //parallelization is slow here
        for (int cx = -ChunkLoadRange; cx <= ChunkLoadRange; cx++)
        {
            for (int cz = -ChunkLoadRange; cz <= ChunkLoadRange; cz++)
            {
                if (_runGenerationThread)
                {
                    for (int cy = 0; cy <= WorldHeight; cy++)
                    {
                        CreateChunk(position, cx, cy, cz);
                    }
                }
            }
        }
    }

    private void CreateChunk(Int3 position, int cx, int cy, int cz)
    {
        Int3 chunkPosition = new Int3(position.X + cx, Mathf.Clamp(position.Y + cy, 0, WorldHeight),
            position.Z + cz);
        if (!_chunks.ContainsKey(chunkPosition))
        {
            Chunk chunk = new Chunk(chunkPosition);
            chunk.Initialize(Actor);
            _chunks.TryAdd(chunkPosition, chunk);
        }
    }

    private void UnloadChunksAround(Int3 position)
    {
        foreach (var keyValuePair in _chunks)
        {
            Int3 chunkPosition = keyValuePair.Key;
            var distanceSquared = Float3.DistanceSquared(new Float3(chunkPosition.X,chunkPosition.Y,chunkPosition.Z), new Float3(position.X,position.Y,position.Z));
        }
    }

    public override void OnDestroy()
    {
        _runGenerationThread = false;
        if(_genThreadLabel!=-1)
            JobSystem.Wait(_genThreadLabel);
        _chunks.ForEach(pair => pair.Value.Models.ForEach(model => Destroy(model)));
    }

    public Int3 PosToChunkCoordinate(Float3 pos)
    {
        return new Int3(Mathf.FloorToInt(pos.X/100 / 16), Mathf.FloorToInt(pos.Y/100/ChunkHeight), Mathf.FloorToInt(pos.Z/100 / 16));
    }

    public Chunk GetChunkAt(Float3 globalPosition)
    {
        return _chunks.GetValueOrDefault(PosToChunkCoordinate(globalPosition));
    }
}