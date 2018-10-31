using System.Linq;
using RobiGroup.AskMeFootball.Core.Handlers;
using RobiGroup.AskMeFootball.Models.Games;

namespace RobiGroup.AskMeFootball.Core.Game
{
    public class GameManager : IGameManager
    {
        private GamersHandler _gamersHandler;

        public GameManager(GamersHandler gamersHandler)
        {
            _gamersHandler = gamersHandler;
        }

        public GameModel TryStartGame(string gamerId, int cardId)
        {
            _gamersHandler.WebSocketConnectionManager.Connections.Values.OrderBy(c => c.ConnectedTime);

            throw new System.NotImplementedException();
        }
    }
}