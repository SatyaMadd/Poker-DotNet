using pokerapi.Interfaces;
using pokerapi.Models;


namespace pokerapi.Services{
    public class BotService : IBotService
    {
        private readonly IGameService _gameService;

        public BotService(IGameService gameService)
        {
            _gameService = gameService;
        }

        public async Task<IEnumerable<GameAction>> BotMove(string username){
            var game = await _gameService.GetGameAsync(username);
            if(game == null){
                return [];
            }
            if(username.StartsWith("BotLeave")){
                if(game.Showdown){
                    return await _gameService.MuckCardsAsync(username);
                }else{
                    if(game.BetHasOccurred){
                        return await _gameService.FoldAsync(username); 
                    }else{
                        return await _gameService.CheckAsync(username); 
                    }
                }   
            }
            return [];
        }
    }
}