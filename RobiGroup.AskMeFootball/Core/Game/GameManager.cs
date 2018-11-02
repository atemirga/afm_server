using System;
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
            var enemy = _gamersHandler.WebSocketConnectionManager.Connections.Values.Where(c => !c.Away && !c.IsBusy && c.UserId != gamerId).OrderByDescending(c => c.ConnectedTime).FirstOrDefault();
            var model = new GameModel();

            if (enemy != null)
            {
                var game = new Data.Match()
                {
                    CardId = cardId,
                    CreateTime = DateTime.Now,
                };
                _dbContext.Matches.Add(game);
                _dbContext.SaveChanges();

                _dbContext.MatchParticipants.Add(new MatchParticipant
                {
                    MacthId = game.Id,
                    GamerId = gamerId
                });
                _dbContext.MatchParticipants.Add(new MatchParticipant
                {
                    MacthId = game.Id,
                    GamerId = enemy.UserId
                });
                _dbContext.SaveChanges();

                model.Id = game.Id;
                model.Found = true;
            }

            return model;
        }
    }
}