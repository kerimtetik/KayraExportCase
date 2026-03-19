namespace Auth.Infrastructure.Authorization;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string ProductManager = "ProductManager";
    public const string User = "User";

    public static readonly string[] All =
    [
        Admin,
        ProductManager,
        User
    ];
}
