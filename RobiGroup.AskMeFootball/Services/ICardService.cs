using System.Collections.Generic;
using System.Linq;
using RobiGroup.AskMeFootball.Data;
using RobiGroup.AskMeFootball.Models.Cards;
using RobiGroup.AskMeFootball.Models.Leaderboard;

namespace RobiGroup.AskMeFootball.Services
{
    public interface ICardService
    {
        IQueryable<LeaderboardCardGamerModel> GetLeaderboard(int cardId);

        IQueryable<CardWinnerModel> GetWinners(int cardId);
    }

    public class CardService : ICardService
    {
        private readonly ApplicationDbContext _dbContext;

        public CardService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<LeaderboardCardGamerModel> GetLeaderboard(int cardId)
        {
            return (from gc in _dbContext.GamerCards
                join u in _dbContext.Users on gc.GamerId equals u.Id
                where gc.CardId == cardId && gc.IsActive && u.Bot == 0
                select new LeaderboardCardGamerModel
                {
                    Id = u.Id,
                    PhotoUrl = u.PhotoUrl,
                    Nickname = u.NickName,
                    CardScore = gc.Score,
                    CurrentScore = u.Score,
                    TotalScore = u.TotalScore,
                    IsBot = u.Bot > 0,
                    Coins = _dbContext.UserCoins.Any(uc => uc.GamerId == u.Id) ?
                       _dbContext.UserCoins.FirstOrDefault(uc => uc.GamerId == u.Id).Coins : 0,
                    IsPlaying = _dbContext.MatchGamers.Any(mg => mg.GamerId == gc.GamerId && mg.IsPlay),
                    Raiting = _dbContext.GamerCards.Where(gcr => gcr.CardId == cardId && gcr.IsActive)
                                  .Count(gr => gr.Score > gc.Score) + 1,
                }).OrderBy(r => r.Raiting).ThenBy(r => r.CurrentScore);
        }

        public IQueryable<CardWinnerModel> GetWinners(int cardId)
        {
            return (from cw in _dbContext.CardWinners
                join u in _dbContext.Users on cw.GamerId equals u.Id
                where cw.CardId == cardId
                orderby cw.CardEndTime descending 
                select new CardWinnerModel
                {
                    Id = cw.Id,
                    GamerId = cw.GamerId,
                    GamerCardScore = cw.GamerCardScore,
                    Prize = cw.Prize,
                    CardEndTime = cw.CardEndTime,
                    CardStartTime = cw.CardStartTime,
                    GamerNickName = u.NickName,
                    GamerPhotoUrl = u.PhotoUrl
                });
        }
    }
}