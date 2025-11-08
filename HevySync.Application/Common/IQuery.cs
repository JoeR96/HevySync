namespace HevySync.Application.Common;

/// <summary>
/// Marker interface for queries.
/// Queries represent a request for data without changing state.
/// </summary>
public interface IQuery<out TResult>
{
}

