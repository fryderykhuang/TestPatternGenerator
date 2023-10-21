using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace TestPatternGenerator;

internal class ManualChangeTokenSource<T> : IOptionsChangeTokenSource<T>
{
    private readonly IConfiguration _config;
    private readonly SettingsManager _settingsManager;

    /// <summary>
    ///     Constructor taking the <see cref="IConfiguration" /> instance to watch.
    /// </summary>
    /// <param name="config">The configuration instance.</param>
    public ManualChangeTokenSource(IConfiguration config, SettingsManager settingsManager) : this(Options.DefaultName,
        config)
    {
        _settingsManager = settingsManager;
    }

    /// <summary>
    ///     Constructor taking the <see cref="IConfiguration" /> instance to watch.
    /// </summary>
    /// <param name="name">The name of the options instance being watched.</param>
    /// <param name="config">The configuration instance.</param>
    public ManualChangeTokenSource(string? name, IConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        Name = name ?? Options.DefaultName;
    }

    /// <summary>
    ///     The name of the option instance being changed.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Returns the reloadToken from the <see cref="IConfiguration" />.
    /// </summary>
    /// <returns></returns>
    public IChangeToken GetChangeToken()
    {
        var mtoken = _settingsManager.GetChangeToken<T>();
        if (mtoken != null)
            return new CompositeChangeToken(new[] {mtoken, _config.GetReloadToken()});
        return _config.GetReloadToken();
    }
}