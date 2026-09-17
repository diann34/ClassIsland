using Avalonia.Metadata;
using ClassIsland.Core.Services;

namespace ClassIsland.Core.MarkupExtensions;

public sealed class TrExtension
{
    [ConstructorArgument(nameof(Key))]
    public string Key { get; set; } = "";

    public TrExtension()
    {
    }

    public TrExtension(string key)
    {
        Key = key;
    }

    public string ProvideValue(IServiceProvider serviceProvider) => LocalizationService.Translate(Key);
}
