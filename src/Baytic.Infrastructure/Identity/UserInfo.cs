namespace Baytic.Domain.Identity;

public sealed class UserInfo
{
    public UserId UserId { get; }
    public string Name { get; }
    public string[] Roles { get; }
    public string[] Groups { get; }
    public UserInfo(UserId userId, string name, string[] roles, string[] groups)
    {
        UserId = userId;
        Name = name;
        Roles = roles;
        Groups = groups;
    }
}