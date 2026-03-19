namespace Auth.Infrastructure.Authorization;

public static class AppPolicies
{
    public const string AdminOnlyPolicy = "AdminOnlyPolicy";
    public const string ProductWritePolicy = "ProductWritePolicy";
    public const string LogsReadPolicy = "LogsReadPolicy";
}
