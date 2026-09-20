using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using ClassIsland.Core.Models.Localization;

namespace ClassIsland.Core.Services;

/// <summary>
/// ClassIsland本地化服务。用于管理应用程序的语言和翻译资源。
/// </summary>
public static class LocalizationService
{
    /// <summary>
    /// 表示自动选择语言的特殊标识符。当用户未指定语言时，应用程序将根据系统语言或其他逻辑自动选择合适的语言。
    /// </summary>
    public const string AutomaticLanguage = "auto";
    /// <summary>
    /// 表示默认的回退语言为简体中文（zh-Hans）。当应用程序无法找到指定语言的资源时，将使用此语言作为回退选项。
    /// </summary>
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

    /// <summary>
    /// 获取当前应用程序使用的文化名称。如果未指定语言或无法解析请求的语言，将使用<see cref="FallbackCultureName"/>作为默认值。
    /// </summary>
    public static string CurrentCultureName { get; private set; } = FallbackCultureName;

    public static IReadOnlyList<SupportedLanguage> SupportedLanguages => RegisteredLanguages.Values
        .OrderBy(x => x.CultureName, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    /// <summary>
    /// 使用请求的文化名称和指定的资源程序集初始化本地化服务。此方法将加载资源程序集中的本地化资源，并根据请求的文化名称或系统文化设置当前应用程序的语言环境。
    /// </summary>
    /// <param name="requestedCulture"></param>
    /// <param name="resourceAssemblies"></param>
    public static void Initialize(string? requestedCulture, params Assembly[] resourceAssemblies)
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            var requestedAssemblyName = new AssemblyName(args.Name);

            if (string.IsNullOrEmpty(requestedAssemblyName.CultureName) || requestedAssemblyName.CultureName == "neutral") return null;

            var basePath = AppContext.BaseDirectory;
            var cultureName = requestedAssemblyName.CultureName;
            var assemblyName = requestedAssemblyName.Name;
            var satelliteAssemblyPath = Path.Combine(basePath, "Assets", "Localization", cultureName, $"{assemblyName}.resources.dll");

            if (File.Exists(satelliteAssemblyPath)) return Assembly.LoadFrom(satelliteAssemblyPath);
            return null;
        };
        var systemCulture = CultureInfo.CurrentUICulture;
        var systemFormattingCulture = CultureInfo.CurrentCulture; // 使得应用程序的时间日期格式与系统一致，而不跟随UI语言变化
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
    /// <summary>
    /// 根据指定的键获取对应的本地化字符串。如果找不到对应的资源，将返回原始键值。此方法使用<see cref="CurrentCultureName"/>进行资源查找。
    /// </summary>
    /// <param name="key">本地化字符串ID</param>
    /// <returns></returns>
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
    /// <summary>
    /// 根据指定的键和参数获取对应的本地化字符串，并使用当前文化格式化字符串。如果找不到对应的资源，将返回原始键值。此方法使用<see cref="CurrentCultureName"/>进行资源查找。
    /// </summary>
    /// <param name="key">本地化字符串ID</param>
    /// <param name="arguments">格式化参数</param>
    /// <returns></returns>
    public static string Translate(string key, params object?[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, Translate(key), arguments);

    /// <summary>
    /// 将由C# UI代码创建的静态字符串进行翻译。
    /// </summary>
    /// <param name="source">要翻译的源字符串</param>
    /// <returns></returns>
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
                    throw new InvalidOperationException($"重复的本地化资源ID: {key}");
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
