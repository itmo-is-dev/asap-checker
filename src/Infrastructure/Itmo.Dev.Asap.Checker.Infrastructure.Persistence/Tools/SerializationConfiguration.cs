using FluentSerialization;
using Itmo.Dev.Asap.Checker.Application.Models.CheckingResults;

namespace Itmo.Dev.Asap.Checker.Infrastructure.Persistence.Tools;

public class SerializationConfiguration : ISerializationConfiguration
{
    public void Configure(ISerializationConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Type<CodeBlock>(type =>
        {
            type.Property(x => x.FilePath).Called("file_path");
            type.Property(x => x.LineFrom).Called("line_from");
            type.Property(x => x.LineTo).Called("line_to");
            type.Property(x => x.Content).Called("content");
        });
    }
}