using pokerapi.Models;
public interface IHubInteractionService
{
    Task ExecuteAction(Func<string, int, Task<IEnumerable<GameAction>>> actionFunc, string username, int amount = 0);
    Task HandleAutoCheck(string username);
    Task HandleBotAction(string botName);
}
