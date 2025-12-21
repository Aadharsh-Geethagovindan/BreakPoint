public static class OnlinePlayerIdentity
{
    public static int LocalTeamId { get; private set; } = -1; // 1 or 2
    public static bool HasTeam => LocalTeamId == 1 || LocalTeamId == 2;

    public static void SetTeam(int teamId)
    {
        LocalTeamId = teamId;
    }
}