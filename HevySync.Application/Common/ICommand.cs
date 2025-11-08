namespace HevySync.Application.Common;

/// <summary>
/// Marker interface for commands.
/// Commands represent an intent to change state.
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Marker interface for commands that return a result.
/// </summary>
public interface ICommand<out TResult> : ICommand
{
}

