using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace FtDSharp
{
    internal sealed class InMemoryAdditionalText : AdditionalText
    {
        private readonly SourceText _source;

        internal InMemoryAdditionalText(string path, string content)
        {
            Path = path;
            _source = SourceText.From(content, Encoding.UTF8);
        }

        public override string Path { get; }

        public override SourceText? GetText(CancellationToken cancellationToken = default) => _source;
    }
}
