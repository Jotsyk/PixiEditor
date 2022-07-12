﻿using ChunkyImageLib.DataHolders;

namespace ChunkyImageLib.Operations;

internal interface IDrawOperation : IOperation
{
    bool IgnoreEmptyChunks { get; }
    void DrawOnChunk(Chunk chunk, VecI chunkPos);
    HashSet<VecI> FindAffectedChunks(VecI imageSize);
    IDrawOperation AsMirrored(int? verAxisX, int? horAxisY);
}
