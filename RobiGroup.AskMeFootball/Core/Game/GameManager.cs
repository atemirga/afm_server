using System.Linq;
using RobiGroup.AskMeFootball.Core.Handlers;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Models.Games;

namespace RobiGroup.AskMeFootball.Core.Game
{
    public class GameManager : IGameManager
    {
        private GamersHandler _gamersHandler;
        private readonly ApplicationDbContext _dbContext;

        public GameManager(GamersHandler gamersHandler, ApplicationDbContext dbContext)
        {
            _gamersHandler = gamersHandler;
            _dbContext = dbContext;
        }

        public GameModel TryStartGame(string gamerId, int cardId)
        {
            _gamersHandler.WebSocketConnectionManager.Connections.Values.OrderBy(c => c.ConnectedTime);

            throw new System.NotImplementedException();
        }
    }
}