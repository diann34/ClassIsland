using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using ClassIsland.Core.Models.Localization;

namespace ClassIsland.Core.Services;

public static class LocalizationService
{
    public const string AutomaticLanguage = "auto";
    public const string FallbackCultureName = "zh-Hans";

    private static readonly List<ResourceManager> ResourceManagers = [];

    private static readonly Dictionary<string, ResourceManager> ResourceManagersByKey =
        new(StringComparer.Ordinal);

    private static readonly Dictionary<string, SupportedLanguage> RegisteredLanguages =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [FallbackCultureName] = CreateLanguage(FallbackCultureName),
            ["zh-Hant"] = CreateLanguage("zh-Hant")
        };

    private static readonly Dictionary<string, string> FallbackKeys =
        new(StringComparer.Ordinal);

    public static string CurrentCultureName { get; private set; } = FallbackCultureName;

    public static IReadOnlyList<SupportedLanguage> SupportedLanguages => RegisteredLanguages.Values
        .OrderBy(x => x.CultureName, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    public static void Initialize(string? requestedCulture, params Assembly[] resourceAssemblies)
    {
        // Keep the operating system's formatting culture separate from the UI language.
        // Selecting a language such as zh-Hant must not change the user's date/time
        // conventions (for example, 12-hour versus 24-hour time).
        var systemCulture = CultureInfo.CurrentUICulture;
        var systemFormattingCulture = CultureInfo.CurrentCulture;
        ResourceManagers.Clear();
        ResourceManagersByKey.Clear();
        FallbackKeys.Clear();

        foreach (var assembly in resourceAssemblies.Distinct())
        {
            var resourcePrefix = $"{assembly.GetName().Name}.Assets.Localization.";
            var resourceBaseNames = assembly.GetManifestResourceNames()
                .Where(x => x.StartsWith(resourcePrefix, StringComparison.Ordinal) &&
                            x.EndsWith(".resources", StringComparison.Ordinal))
                .Select(x => x[..^".resources".Length])
                .OrderBy(x => x, StringComparer.Ordinal);

            ResourceManagers.AddRange(resourceBaseNames.Select(x => new ResourceManager(x, assembly)));
        }

        LoadFallbackKeys();

        CurrentCultureName = ResolveCulture(requestedCulture, systemCulture);
        var uiCulture = CultureInfo.GetCultureInfo(CurrentCultureName);
        var formattingCulture = systemFormattingCulture.IsNeutralCulture
            ? CultureInfo.CreateSpecificCulture(systemFormattingCulture.Name)
            : systemFormattingCulture;
        CultureInfo.CurrentCulture = formattingCulture;
        CultureInfo.CurrentUICulture = uiCulture;
        CultureInfo.DefaultThreadCurrentCulture = formattingCulture;
        CultureInfo.DefaultThreadCurrentUICulture = uiCulture;
    }

    public static string Translate(string key)
    {
        var culture = CultureInfo.GetCultureInfo(CurrentCultureName);
        if (ResourceManagersByKey.TryGetValue(key, out var resourceManager) &&
            resourceManager.GetString(key, culture) is { } value)
        {
            return value;
        }

        return key;
    }

    public static string Translate(string key, params object?[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, Translate(key), arguments);

    /// <summary>
    /// Translates a static string created by C# UI code. AXAML should use <c>{ci:Tr ...}</c> instead.
    /// </summary>
    public static string TranslateText(string source) =>
        FallbackKeys.TryGetValue(source, out var key) ? Translate(key) : source;

    private static string ResolveCulture(string? requestedCulture, CultureInfo systemCulture)
    {
        if (!string.IsNullOrWhiteSpace(requestedCulture) &&
            !string.Equals(requestedCulture, AutomaticLanguage, StringComparison.OrdinalIgnoreCase) &&
            TryResolveSupportedCulture(requestedCulture, out var selectedCulture))
        {
            return selectedCulture;
        }

        if (TryResolveSupportedCulture(systemCulture.Name, out var systemLanguage))
        {
            return systemLanguage;
        }

        if (systemCulture.TwoLetterISOLanguageName == "zh")
        {
            var traditionalRegions = new[] { "TW", "HK", "MO" };
            if (systemCulture.Name.Contains("Hant", StringComparison.OrdinalIgnoreCase) ||
                traditionalRegions.Any(x => systemCulture.Name.EndsWith($"-{x}", StringComparison.OrdinalIgnoreCase)))
            {
                return RegisteredLanguages.ContainsKey("zh-Hant") ? "zh-Hant" : FallbackCultureName;
            }
        }

        return RegisteredLanguages.ContainsKey(FallbackCultureName)
            ? FallbackCultureName
            : RegisteredLanguages.Keys.FirstOrDefault() ?? FallbackCultureName;
    }

    private static bool TryResolveSupportedCulture(string cultureName, out string supportedCulture)
    {
        if (RegisteredLanguages.ContainsKey(cultureName))
        {
            supportedCulture = RegisteredLanguages.Keys.First(x =>
                string.Equals(x, cultureName, StringComparison.OrdinalIgnoreCase));
            return true;
        }

        var languageName = cultureName.Split('-')[0];
        var candidates = RegisteredLanguages.Keys.Where(x =>
            string.Equals(x.Split('-')[0], languageName, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (candidates.Length == 1)
        {
            supportedCulture = candidates[0];
            return true;
        }

        supportedCulture = "";
        return false;
    }

    private static void LoadFallbackKeys()
    {
        foreach (var resourceManager in ResourceManagers)
        {
            var resourceSet = resourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, false);
            if (resourceSet == null)
            {
                continue;
            }

            foreach (DictionaryEntry entry in resourceSet)
            {
                if (entry is not { Key: string key, Value: string value })
                {
                    continue;
                }

                if (!ResourceManagersByKey.TryAdd(key, resourceManager))
                {
                    throw new InvalidOperationException($"Duplicate localization resource ID: {key}");
                }

                FallbackKeys.TryAdd(value, key);
            }
        }
    }

    private static SupportedLanguage CreateLanguage(string cultureName)
    {
        var culture = CultureInfo.GetCultureInfo(cultureName);
        return new SupportedLanguage(culture.Name, culture.NativeName);
    }
}
