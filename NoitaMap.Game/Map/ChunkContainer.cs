﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;
using NoitaMap.Game.Graphics;
using NoitaMap.Game.Materials;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osuTK;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NoitaMap.Game.Map;

public partial class ChunkContainer : Drawable, ITexturedShaderDrawable
{
    private static readonly Regex ChunkPositionRegex = new Regex("world_(?<x>-?\\d+)_(?<y>-?\\d+)\\.png_petri", RegexOptions.Compiled);

    public IShader TextureShader { get; protected set; }

    [Resolved]
    private MaterialProvider MaterialProvider { get; set; }

    public Dictionary<Vector2, Chunk> Chunks = new Dictionary<Vector2, Chunk>();

    public ConcurrentQueue<Chunk> FinishedChunks = new ConcurrentQueue<Chunk>();

    [BackgroundDependencyLoader]
    private void Load(ShaderManager shaders)
    {
        TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE);
    }

    public void LoadChunk(string chunkFilePath)
    {
        byte[] decompressedData = NoitaDecompressor.ReadAndDecompressChunk(chunkFilePath);

        using MemoryStream ms = new MemoryStream(decompressedData);
        using BinaryReader reader = new BinaryReader(ms);

        int version = reader.ReadBEInt32();
        int width = reader.ReadBEInt32();
        int height = reader.ReadBEInt32();

        if (version != 24 || width != Chunk.ChunkWidth || height != Chunk.ChunkHeight)
        {
            throw new InvalidDataException($"Chunk header was not correct. Version = {version} Width = {width} Height = {height}");
        }

        Vector2 chunkPosition = GetChunkPositionFromPath(chunkFilePath);

        Chunk chunk = new Chunk(chunkPosition, MaterialProvider!);

        chunk.Deserialize(reader);

        FinishedChunks.Enqueue(chunk);
    }

    protected override DrawNode CreateDrawNode()
    {
        return new ChunkContainerDrawNode(this);
    }

    private static Vector2 GetChunkPositionFromPath(string filePath)
    {
        string fileName = Path.GetFileName(filePath);

        Match match = ChunkPositionRegex.Match(fileName);

        return new Vector2(int.Parse(match.Groups["x"].Value), int.Parse(match.Groups["y"].Value));
    }
}
