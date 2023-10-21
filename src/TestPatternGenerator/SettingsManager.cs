using System.Drawing.Drawing2D;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace TestPatternGenerator;

public class SettingsManager
{
    private readonly string _baseDir;

    private readonly Dictionary<Type, RootContainer> _roots = new();

    private readonly Dictionary<Type, SettingsTypeContainer> _types = new();

    public SettingsManager()
    {
        _baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FryderykHuang/TestPatternGenerator");

        Directory.CreateDirectory(_baseDir);
    }

    public IConfigurationBuilder AddFileLocations(IConfigurationBuilder builder)
    {
        foreach (var (key, value) in _roots) builder.AddJsonFile(value.FilePath);

        return builder;
    }


    public IServiceCollection Configure(IConfiguration configurationRoot, IServiceCollection services)
    {
        services.AddOptions();

        foreach (var (key, value) in _types)
        {
            var sect = configurationRoot.GetSection(value.ConfigurationSection);

            services.AddSingleton(typeof(IOptionsChangeTokenSource<>).MakeGenericType(key),
                Activator.CreateInstance(typeof(ManualChangeTokenSource<>).MakeGenericType(key), sect, this));

            services.AddSingleton(typeof(IConfigureOptions<>).MakeGenericType(key),
                Activator.CreateInstance(
                    typeof(NamedConfigureFromConfigurationOptions<>).MakeGenericType(key),
                    Options.DefaultName, sect, null) ?? throw new InvalidOperationException());

            services.AddSingleton(typeof(IOptionsMonitorCache<>).MakeGenericType(key),
                typeof(CustomOptionsMonitorCache<>).MakeGenericType(key));
        }

        return services;
    }

    private T? GetValue<T>() where T : class
    {
        if (!_types.TryGetValue(typeof(T), out var c))
            return null;
        return (T) c.GetValue();
    }

    public void RegisterType<TRoot, T>(Expression<Func<TRoot, T>> expression) where T : class
    {
        if (!_roots.ContainsKey(typeof(TRoot)))
            _roots[typeof(TRoot)] =
                new RootContainer(typeof(TRoot),
                    Activator.CreateInstance(typeof(TRoot)) ?? throw new InvalidOperationException(),
                    Path.Combine(_baseDir, typeof(TRoot).Name + ".json"));

        var body = (MemberExpression) expression.Body;
        var list = new List<MemberExpression>();
        for (var m = body; m != null; m = m.Expression as MemberExpression) list.Add(m);

        var pTRoot = Expression.Parameter(typeof(object));
        var pT = Expression.Parameter(typeof(object));

        Expression memberAccessExpr = Expression.Convert(pTRoot, typeof(TRoot));

        var sb = new StringBuilder();
        foreach (var m in list.AsEnumerable().Reverse())
        {
            sb.Append(m.Member.Name);
            sb.Append(':');
            memberAccessExpr = Expression.MakeMemberAccess(memberAccessExpr, m.Member);
        }

        sb.Remove(sb.Length - 1, 1);


        var section = sb.ToString();

        var setLambda = Expression.Lambda<Action<object, object>>(
            Expression.Assign(memberAccessExpr, Expression.Convert(pT, typeof(T))), pTRoot, pT);

        var getLambda =
            Expression.Lambda<Func<object, object>>(Expression.Convert(memberAccessExpr, typeof(object)), pTRoot);
        _types[typeof(T)] =
            new SettingsTypeContainer(this, section, getLambda.Compile(), setLambda.Compile(), typeof(TRoot));
    }

    public void SetSettings<T>(T settings)
    {
        if (!_types.TryGetValue(typeof(T), out var cont)) throw new NotSupportedException();

        cont.SetValueAndNotify(settings);
    }

    public void SaveSettings()
    {
        foreach (var (key, value) in _roots) value.SaveToFile();
    }

    public object? GetRoot(Type rootType)
    {
        if (_roots.TryGetValue(rootType, out var ret))
            return ret.RootObject;
        return null;
    }

    public void WriteDefaultsIfNotExists()
    {
        foreach (var (key, value) in _roots)
        {
            value.TryWriteDefaultSettings();
            try
            {
                value.LoadObject();
            }
            catch (Exception)
            {
                value.TryWriteDefaultSettings(true);
                value.LoadObject();
            }
        }
    }

    internal IChangeToken? GetChangeToken<T>()
    {
        if (!_types.TryGetValue(typeof(T), out var cont))
            return null;
        return cont.GetChangeToken();
    }

    public void DisableChanges<T>()
    {
        var c = _types[typeof(T)];
        c.SetIgnoreWrite(true);
    }

    public void EnableChanges<T>()
    {
        var c = _types[typeof(T)];
        c.SetIgnoreWrite(false);
    }

    public IDisposable IgnoreChangesOn<T>()
    {
        return new IgnoreChangesScope<T>(this);
    }

    public bool IsWriteIgnoredOn<T>()
    {
        return _types[typeof(T)].IgnoreWrite;
    }

    private class CustomOptionsMonitorCache<T> : OptionsCache<T> where T : class
    {
        private readonly SettingsManager _settingsManager;

        public CustomOptionsMonitorCache(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        public override T GetOrAdd(string? name, Func<T> createOptions)
        {
            try
            {
                return _settingsManager.GetValue<T>();
            }
            catch
            {
            }

            return base.GetOrAdd(name, createOptions);
        }
    }

    private class SettingsTypeContainer
    {
        private readonly Dictionary<Action<object?>, object?> _changeRegistry = new();
        private readonly Func<object, object> _getAction;
        private readonly Type _rootType;
        private readonly Action<object, object> _setAction;
        private readonly SettingsManager _settingsManager;

        private CancellationTokenSource _cts = new();

        public SettingsTypeContainer(SettingsManager settingsManager, string configurationSection,
            Func<object, object> getAction,
            Action<object, object> setAction, Type rootType)
        {
            ConfigurationSection = configurationSection;
            _setAction = setAction;
            _settingsManager = settingsManager;
            _getAction = getAction;
            _rootType = rootType;
        }

        public string ConfigurationSection { get; }

        public bool IgnoreWrite { get; private set; }

        private void NotifyCallbacks()
        {
            var prev = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
            prev.Cancel();
            //
            //
            // lock (_changeRegistry)
            // {
            //     try
            //     {
            //         foreach (var (key, value) in _changeRegistry)
            //         {
            //             key?.Invoke(value);
            //         }
            //     }
            //     catch (InvalidOperationException)
            //     {
            //     }
            // }
        }

        public void SetValueAndNotify(object? settings)
        {
            if (IgnoreWrite)
                return;

            _setAction(_settingsManager.GetRoot(_rootType) ?? throw new InvalidOperationException(), settings);
            NotifyCallbacks();
        }

        // public IDisposable AddCallback(Action<object?> callback, object? state)
        // {
        //     var cts = new CancellationToken().Register();
        //     lock (_changeRegistry)
        //     {
        //         _changeRegistry[callback] = state;
        //     }
        // }
        //
        // public void RemoveCallback(Action<object?> callback)
        // {
        //     lock (_changeRegistry)
        //     {
        //         _changeRegistry.Remove(callback);
        //     }
        // }

        public IChangeToken GetChangeToken()
        {
            return new CancellationChangeToken(_cts.Token);
        }

        public object GetValue()
        {
            var ret = _getAction(_settingsManager.GetRoot(_rootType) ?? throw new InvalidOperationException());
            return ret;
        }

        public void SetIgnoreWrite(bool b)
        {
            IgnoreWrite = b;
        }
    }

    private class RootContainer
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions =
            new(JsonSerializerDefaults.General)
            {
                TypeInfoResolver = SourceGenerationContext.Default,
                Converters = {new ColorJsonConverter(), new Drawing2DMatrixJsonConverter()}
            };

        private readonly Type _objectType;

        public RootContainer(Type objectType, object? rootObject, string filePath)
        {
            _objectType = objectType;
            RootObject = rootObject;
            FilePath = filePath;
        }

        public string FilePath { get; }

        public object? RootObject { get; private set; }

        public void TryWriteDefaultSettings(bool forceWrite = false)
        {
            if (!forceWrite && File.Exists(FilePath))
                return;

            using var fs = File.OpenWrite(FilePath);
            fs.SetLength(0);
            JsonSerializer.Serialize(fs, Activator.CreateInstance(_objectType), _objectType, JsonSerializerOptions);
        }

        public void LoadObject()
        {
            using var fs = File.OpenRead(FilePath);
            RootObject = JsonSerializer.Deserialize(fs, _objectType, JsonSerializerOptions);
        }

        public void SaveToFile()
        {
            using var fs = File.OpenWrite(FilePath);
            fs.SetLength(0);
            JsonSerializer.Serialize(fs, RootObject, JsonSerializerOptions);
        }

        public class ColorJsonConverter : JsonConverter<Color>
        {
            public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return ColorTranslator.FromHtml(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(ColorTranslator.ToHtml(value));
                // writer.WriteStringValue("#" + value.R.ToString("X2") + value.G.ToString("X2") +
                //                         value.B.ToString("X2").ToLower());
            }
        }

        public class Drawing2DMatrixJsonConverter : JsonConverter<Matrix>
        {
            public override Matrix Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var str = reader.GetString();
                if (string.IsNullOrWhiteSpace(str))
                    return null;

                var vals = str.Split(':', StringSplitOptions.TrimEntries).Select(float.Parse).ToList();
                return new Matrix(vals[0], vals[1], vals[2], vals[3], vals[4], vals[5]);
            }

            public override void Write(Utf8JsonWriter writer, Matrix value, JsonSerializerOptions options)
            {
                if (value == null)
                    writer.WriteNullValue();
                else
                    writer.WriteStringValue(
                        $"{value.Elements[0]}:{value.Elements[1]}:{value.Elements[2]}:{value.Elements[3]}:{value.Elements[4]}:{value.Elements[5]}");
            }
        }
    }

    private class IgnoreChangesScope<T> : IDisposable
    {
        private readonly SettingsManager _settingsManager;

        public IgnoreChangesScope(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            settingsManager.DisableChanges<T>();
        }

        public void Dispose()
        {
            _settingsManager.EnableChanges<T>();
        }
    }
}