namespace UnicornHack.Utils.MessagingECS;

/// <summary>
///     A message consumer.
///     Should contain no state.
/// </summary>
public interface IMessageConsumer<TMessage>
{
    MessageProcessingResult Process(TMessage message, IEntityManager manager);
}

/// <summary>
///     A message consumer using a concrete entity-manager type.
/// </summary>
/// <typeparam name="TMessage">The message type</typeparam>
/// <typeparam name="TManager">The entity-manager type</typeparam>
// ReSharper disable TypeParameterCanBeVariant
public interface IMessageConsumer<TMessage, TManager> : IMessageConsumer<TMessage>
    where TManager : IEntityManager
{
    /// <summary>
    ///     Process the supplied message
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="manager">Entity manager</param>
    /// <returns><c>true</c> if other consumers shouldn't receive this message</returns>
    MessageProcessingResult Process(TMessage message, TManager manager);

    MessageProcessingResult IMessageConsumer<TMessage>.Process(TMessage message, IEntityManager manager)
        => Process(message, (TManager)manager);
}
