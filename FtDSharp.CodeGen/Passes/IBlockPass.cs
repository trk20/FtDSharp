using FtDSharp.CodeGen.Models;

namespace FtDSharp.CodeGen.Passes;

/// <summary>
/// Interface for transformation passes in the code generation pipeline.
/// </summary>
public interface IBlockPass
{
    void Process(List<BlockDefinition> blocks);
}
