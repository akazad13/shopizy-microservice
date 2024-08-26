namespace Shopizy.Security.Permissions;

public static partial class Permissions
{
    public static class Product
    {
        public const string Create = "create:product";
        public const string Get = "get:product";
        public const string Modify = "modify:product";
        public const string Delete = "delete:product";
    }
}