using RobiGroup.AskMeFootball.Models.Games;

namespace RobiGroup.AskMeFootball.Core.Game
{
    public interface IGameManager
    {
        GameModel TryStartGame(string gamerId, int cardId);
    }
}