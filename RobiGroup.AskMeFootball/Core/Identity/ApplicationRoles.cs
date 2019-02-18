namespace RobiGroup.AskMeFootball.Core.Identity
{
    public class ApplicationRoles
    {
        public const string Admin = "Admin";
        public const string Gamer = "Gamer";

        public static readonly string[] Roles = new[] { Gamer, Admin };
    }
}