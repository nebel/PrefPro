using Dalamud.Game.Text.SeStringHandling.Payloads;
using PrefPro.Settings;

namespace PrefPro;

public class NameHandlerCache
{
    private readonly Configuration _configuration;

    private bool _isValid;
    private NameHandlerData _data;

    public NameHandlerCache(Configuration configuration)
    {
        _configuration = configuration;
        Invalidate();
    }

    public NameHandlerData Data
    {
        get
        {
            if (!_isValid)
                TryRebuild();
            return _data;
        }
    }

    public void Invalidate()
    {
        _isValid = false;
        _data = new NameHandlerData
        {
            Apply = false
        };
    }

    private void TryRebuild()
    {
        var localPlayer = DalamudApi.ClientState.LocalPlayer;
        if (localPlayer == null)
            return;

        var data = new NameHandlerData();
        var config = _configuration.GetOrDefault();
        var playerName = localPlayer.Name.TextValue;

        if (config.Name != playerName)
        {
            data.ApplyFull = true;
            data.ApplyFirst = true;
            data.ApplyLast = true;
        }
        else
        {
            data.ApplyFull = config.FullName != NameSetting.FirstLast;
            data.ApplyFirst = config.FirstName != NameSetting.FirstOnly;
            data.ApplyLast = config.LastName != NameSetting.LastOnly;
        }

        if (data is { ApplyFirst: false, ApplyLast: false, ApplyFull: false })
        {
            data.Apply = false;
        }
        else
        {
            data.Apply = true;

            data.NameFull = new TextPayload(GetNameText(playerName, config.Name, config.FullName));
            data.NameFirst = new TextPayload(GetNameText(playerName, config.Name, config.FirstName));
            data.NameLast = new TextPayload(GetNameText(playerName, config.Name, config.LastName));
        }

        _data = data;
        _isValid = true;
    }

    private static string GetNameText(string playerName, string configName, NameSetting setting)
    {
        switch (setting) {
            case NameSetting.FirstLast:
                return configName;
            case NameSetting.FirstOnly:
                return configName.Split(' ')[0];
            case NameSetting.LastOnly:
                return configName.Split(' ')[1];
            case NameSetting.LastFirst:
                var split = configName.Split(' ');
                return $"{split[1]} {split[0]}";
            default:
                return playerName;
        }
    }
}

public class NameHandlerData
{
    public bool Apply;
    public bool ApplyFull;
    public bool ApplyFirst;
    public bool ApplyLast;
    public TextPayload NameFull;
    public TextPayload NameFirst;
    public TextPayload NameLast;
}