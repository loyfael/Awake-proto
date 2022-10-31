namespace TeamAwake.Core.Network.Dispatchers;

/// <summary>Specifies the result of a dispatch operation.</summary>
public enum DispatchResults
{
    /// <summary>When the message is dispatched successfully.</summary>
    Success,

    /// <summary>When the message is not dispatched (due to error).</summary>
    Failure,

    /// <summary>When the message is not dispatched because it is not mapped.</summary>
    Unhandled
}