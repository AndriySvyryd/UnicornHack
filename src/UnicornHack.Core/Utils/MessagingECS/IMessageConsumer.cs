namespace UnicornHack.Utils.MessagingECS;

/// <summary>
///     A message consumer.
///     Should contain no state.
/// </summary>
/// <typeparam name="TMessage">The message type</typeparam>
/// <typeparam name="TState">The state type</typeparam>
public interface IMessageConsumer<TMessage, TState>
{
    /// <summary>
    ///     Process the supplied message
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="state">State</param>
    /// <returns><c>true</c> if other consumers shouldn't receive this message</returns>
    MessageProcessingResult Process(TMessage message, TState state);
}
