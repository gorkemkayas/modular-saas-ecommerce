namespace Store.Domain.Rules
{
    public static class SlugReservedRules
    {
        public static readonly HashSet<string> ReservedSlugs = new()
        {
            // system
            "admin",
            "api",
            "app",
            "system",
            "internal",

            // auth
            "login",
            "logout",
            "register",
            "signup",
            "signin",
            "auth",
            "account",
            "accounts",
            "profile",
            "me",

            // web basics
            "www",
            "root",
            "home",
            "index",
            "default",

            // ecommerce core
            "store",
            "stores",
            "shop",
            "shops",
            "product",
            "products",
            "category",
            "categories",
            "catalog",
            "cart",
            "checkout",
            "order",
            "orders",
            "payment",
            "payments",

            // user actions
            "search",
            "explore",
            "discover",
            "feed",

            // system pages
            "settings",
            "dashboard",
            "panel",
            "manage",
            "management",
            "control",

            // support
            "help",
            "support",
            "contact",
            "about",
            "terms",
            "privacy",
            "policy",

            // files
            "assets",
            "static",
            "media",
            "images",
            "img",
            "css",
            "js",

            // infrastructure
            "cdn",
            "files",
            "uploads",
            "download",

            // misc
            "status",
            "health",
            "metrics",
            "debug"
        };
    }
}
