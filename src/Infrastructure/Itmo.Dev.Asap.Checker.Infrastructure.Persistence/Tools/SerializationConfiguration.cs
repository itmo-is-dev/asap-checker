using FluentSerialization;
using Itmo.Dev.Asap.Checker.Application.Models.CheckingResults;

namespace Itmo.Dev.Asap.Checker.Infrastructure.Persistence.Tools;

public class SerializationConfiguration : ISerializationConfiguration
{
    public void Configure(ISerializationConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Type<CodeBlock>(type =>
        {
            type.Property(x => x.FilePath).Called("code_block_file_path");
            type.Property(x => x.LineFrom).Called("code_block_line_from");
            type.Property(x => x.LineTo).Called("code_block_line_to");
            type.Property(x => x.Content).Called("code_block_content");
        });
    }
}