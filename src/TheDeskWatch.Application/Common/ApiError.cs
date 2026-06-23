namespace TheDeskWatch.Application.Common;

public sealed record ApiError(string Message, string? Code = null);
