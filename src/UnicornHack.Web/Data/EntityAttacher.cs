using Microsoft.EntityFrameworkCore;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Data;

public static class EntityAttacher
{
    public static void Attach(Game game, GameDbContext context)
    {
        var gameEntry = context.Entry(game);
        if (gameEntry.State != EntityState.Detached)
        {
            return;
        }

        gameEntry.State = EntityState.Unchanged;

        var randomEntry = gameEntry.Reference(g => g.Random).TargetEntry!;
        if (randomEntry.State != EntityState.Detached)
        {
            return;
        }

        randomEntry.State = EntityState.Unchanged;

        var manager = game.Manager;
        foreach (var entity in manager.GetEntities())
        {
            Attach(entity, context);
        }
    }

    public static void Attach(GameEntity entity, GameDbContext context)
    {
        var entityEntry = context.Entry(entity);
        if (entityEntry.State != EntityState.Detached)
        {
            return;
        }

        entityEntry.State = EntityState.Unchanged;

        entity.ForEachComponent(context, (c, id, component) =>
        {
            switch (id)
            {
                case (int)EntityComponent.Level:
                    Attach((LevelComponent)component, c);
                    break;
                case (int)EntityComponent.Player:
                    Attach((PlayerComponent)component, c);
                    break;
                default:
                    Attach(component, c);
                    break;
            }
        });
    }

    public static void Attach(object component, GameDbContext context)
    {
        var connectionEntry = context.Entry(component);
        if (connectionEntry.State != EntityState.Detached)
        {
            return;
        }

        connectionEntry.State = EntityState.Unchanged;
    }

    public static void Attach(LevelComponent level, GameDbContext context)
    {
        var levelEntry = context.Entry(level);
        if (levelEntry.State != EntityState.Detached)
        {
            return;
        }

        levelEntry.State = EntityState.Unchanged;

        var randomEntry = levelEntry.Reference(l => l.GenerationRandom).TargetEntry!;
        if (randomEntry.State != EntityState.Detached)
        {
            return;
        }

        randomEntry.State = EntityState.Unchanged;

        Attach(level.Branch, context);
    }

    public static void Attach(GameBranch branch, GameDbContext context)
    {
        var branchEntry = context.Entry(branch);
        if (branchEntry.State != EntityState.Detached)
        {
            return;
        }

        branchEntry.State = EntityState.Unchanged;

        Attach(branch.Game, context);
    }

    public static void Attach(PlayerComponent player, GameDbContext context)
    {
        var actorEntry = context.Entry(player);
        if (actorEntry.State != EntityState.Detached)
        {
            return;
        }

        actorEntry.State = EntityState.Unchanged;

        foreach (var logEntry in player.LogEntries)
        {
            Attach(logEntry, context);
        }

        foreach (var playerCommand in player.CommandHistory)
        {
            Attach(playerCommand, context);
        }
    }

    public static void Attach(LogEntry logEntry, GameDbContext context)
    {
        var logEntityEntry = context.Entry(logEntry);
        if (logEntityEntry.State != EntityState.Detached)
        {
            return;
        }

        logEntityEntry.State = EntityState.Unchanged;
    }

    public static void Attach(PlayerCommand playerCommand, GameDbContext context)
    {
        var commandEntry = context.Entry(playerCommand);
        if (commandEntry.State != EntityState.Detached)
        {
            return;
        }

        commandEntry.State = EntityState.Unchanged;
    }
}
