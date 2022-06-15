using System.Collections.Generic;
using UnicornHack.Utils;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Systems.Levels;

public class GameBranch : NotificationEntity
{
    private int _gameId;
    private Game _game;
    private string _name;
    private byte _length;
    private int _difficulty;

    public int GameId
    {
        get => _gameId;
        set => SetWithNotify(value, ref _gameId);
    }

    public Game Game
    {
        get => _game;
        set => SetWithNotify(value, ref _game);
    }

    public string Name
    {
        get => _name;
        set => SetWithNotify(value, ref _name);
    }

    public byte Length
    {
        get => _length;
        set => SetWithNotify(value, ref _length);
    }

    public int Difficulty
    {
        get => _difficulty;
        set => SetWithNotify(value, ref _difficulty);
    }

    public ICollection<LevelComponent> Levels
    {
        get;
    } = new ObservableHashSet<LevelComponent>();
}
