using pokerapi.Interfaces;
using pokerapi.Models;

namespace pokerapi.Services
{
    public class WinService : IWinService
    {
        private readonly ILobbyRepository _lobbyRepository;
        private readonly IGameRepository _gameRepository;


        public WinService(ILobbyRepository lobbyRepository, IGameRepository gameRepository)
        {
            _lobbyRepository = lobbyRepository;
            _gameRepository = gameRepository;
        }


        public async Task CalculateWinAsync(int gameId)
        {
            var game = await _lobbyRepository.GetGameByIdAsync(gameId);
            if (game == null)
            {
                return;
            }
            var players = game.Players;

            // Get the community cards from the cards table
            var communityCards = await _lobbyRepository.GetCommCards(gameId);

            // Prepare the players' hands
            foreach (var player in players)
            {
                List<Card> hand =
                [
                    .. (await _lobbyRepository.GetPlayerCards(player.Id)).Select(p => new Card { CardNumber = p.CardNumber, Suit = p.Suit }),
                    .. communityCards.Select(c => new Card { CardNumber = c.CardNumber, Suit = c.Suit }),
                ];

                // Get all combinations of 5 cards
                var combinations = Combinations(hand, 5);

                // Initialize the score for the hand
                decimal score = 0;

                foreach (var combination in combinations)
                {
                    // Sort the cards by number
                    combination.Sort((a, b) => a.CardNumber - b.CardNumber);

                    // Check if the combination is a royal flush
                    if (IsRoyalFlush(combination))
                    {
                        score = Math.Max(score, 9);
                        continue;
                    }

                    // Check if the combination is a straight flush
                    if (IsStraightFlush(combination))
                    {
                        var highCard = GetHighCardForStraight(combination);
                        score = Math.Max(score, 8 + highCard * 0.01m);
                        continue;
                    }

                    // Check if the combination is four of a kind (quads)
                    if (IsFourOfAKind(combination))
                    {
                        var quadCard = GetQuadCard(combination);
                        var fifthCard = combination.First(card => card.CardNumber != quadCard).CardNumber;
                        score = Math.Max(score, 7 + quadCard * 0.01m + fifthCard * 0.0001m);
                        continue;
                    }

                    // Check if the combination is a full house
                    if (IsFullHouse(combination))
                    {
                        var tripleCard = GetTripleCard(combination);
                        var pairCard = GetPairCard(combination);
                        score = Math.Max(score, 6 + tripleCard * 0.01m + pairCard * 0.0001m);
                        continue;
                    }

                    // Check if the combination is a flush
                    if (IsFlush(combination))
                    {
                        score = Math.Max(score, 5 + CalculateCardScore(combination));
                        continue;
                    }

                    // Check if the combination is a straight
                    if (IsStraight(combination))
                    {
                        var highCard = GetHighCardForStraight(combination);
                        score = Math.Max(score, 4 + highCard * 0.01m);
                        continue;
                    }

                    // Check if the combination is three of a kind
                    if (IsThreeOfAKind(combination))
                    {
                        var tripleCard = GetTripleCard(combination);
                        var remainingCards = GetRemainingCards(combination, tripleCard);
                        score = Math.Max(score, 3 + tripleCard * 0.01m + CalculateCardScore(remainingCards));
                        continue;
                    }

                    // Check if the combination is two pair
                    if (IsTwoPair(combination))
                    {
                        var highPair = GetHighPair(combination);
                        var lowPair = GetLowPair(combination);
                        var fifthCard = GetFifthCard(combination, highPair, lowPair);
                        score = Math.Max(score, 2 + highPair * 0.01m + lowPair * 0.0001m + fifthCard * 0.000001m);
                        continue;
                    }

                    // Check if the combination is a pair
                    if (IsPair(combination))
                    {
                        var pairCard = GetPairCard(combination);
                        var remainingCards = GetRemainingCards(combination, pairCard);
                        score = Math.Max(score, 1 + pairCard * 0.01m + CalculateCardScore(remainingCards));
                        continue;
                    }

                    // If no other hand is found, it's a high card hand
                    score = Math.Max(score, CalculateCardScore(combination));
                }

                // Insert the winner into the winners table
                await _gameRepository.UpdateScore(player.Id, score);
            }
        }
        private List<List<T>> Combinations<T>(List<T> list, int length)
        {
            if (length == 0) return new List<List<T>> { new List<T>() };

            var combinations = new List<List<T>>();
            for (int i = 0; i <= list.Count - length; i++)
            {
                var head = list[i];
                var tailCombinations = Combinations(list.Skip(i + 1).ToList(), length - 1);
                foreach (var tailCombination in tailCombinations)
                {
                    tailCombination.Insert(0, head);
                    combinations.Add(tailCombination);
                }
            }
            return combinations;
        }

        private List<List<int>> ArrayCombinations(int m, int n)
        {
            if (n == 0) return new List<List<int>> { new List<int>() };
            if (m == 0) return new List<List<int>>();

            var combinations = ArrayCombinations(m - 1, n);
            foreach (var combination in ArrayCombinations(m - 1, n - 1))
            {
                combination.Add(m - 1);
                combinations.Add(combination);
            }
            return combinations;
        }

        private bool IsRoyalFlush(List<Card> hand)
        {
            var numbers = hand.Select(card => card.CardNumber).OrderBy(n => n).ToList();
            var suits = hand.Select(card => card.Suit).Distinct().ToList();
            return numbers.SequenceEqual(new[] { 1, 10, 11, 12, 13 }) && suits.Count == 1;
        }

        private bool IsStraightFlush(List<Card> hand)
        {
            return IsStraight(hand) && IsFlush(hand);
        }

        private bool IsFourOfAKind(List<Card> hand)
        {
            var counts = hand.GroupBy(card => card.CardNumber).Select(group => group.Count());
            return counts.Contains(4);
        }

        private bool IsFullHouse(List<Card> hand)
        {
            var counts = hand.GroupBy(card => card.CardNumber).Select(group => group.Count());
            return counts.Contains(3) && counts.Contains(2);
        }

        private bool IsFlush(List<Card> hand)
        {
            var suits = hand.Select(card => card.Suit).Distinct().ToList();
            return suits.Count == 1;
        }

        private bool IsStraight(List<Card> hand)
        {
            var numbers = hand.Select(card => card.CardNumber).OrderBy(n => n).ToList();
            return numbers.SequenceEqual(new[] { 1, 2, 3, 4, 5 }) || numbers.SequenceEqual(new[] { 1, 10, 11, 12, 13 }) ||
                Enumerable.Range(1, 5).All(i => numbers.Contains(i));
        }

        private bool IsThreeOfAKind(List<Card> hand)
        {
            var counts = hand.GroupBy(card => card.CardNumber).Select(group => group.Count());
            return counts.Contains(3);
        }

        private bool IsTwoPair(List<Card> hand)
        {
            var counts = hand.GroupBy(card => card.CardNumber).Select(group => group.Count());
            return counts.Count(c => c == 2) == 2;
        }

        private bool IsPair(List<Card> hand)
        {
            var counts = hand.GroupBy(card => card.CardNumber).Select(group => group.Count());
            return counts.Contains(2);
        }

        private int GetQuadCard(List<Card> hand)
        {
            var counts = hand.GroupBy(card => card.CardNumber).ToDictionary(group => group.Key, group => group.Count());
            return counts.FirstOrDefault(c => c.Value == 4).Key;
        }

        private int GetTripleCard(List<Card> hand)
        {
            var counts = hand.GroupBy(card => card.CardNumber).ToDictionary(group => group.Key, group => group.Count());
            return counts.FirstOrDefault(c => c.Value == 3).Key;
        }

        private int GetPairCard(List<Card> hand)
        {
            var counts = hand.GroupBy(card => card.CardNumber).ToDictionary(group => group.Key, group => group.Count());
            return counts.FirstOrDefault(c => c.Value == 2).Key;
        }

        private int GetHighCard(List<Card> hand, int excludeCard)
        {
            return hand.Where(card => card.CardNumber != excludeCard).Max(card => card.CardNumber);
        }

        private int GetHighPair(List<Card> hand)
        {
            var counts = hand.GroupBy(card => card.CardNumber).ToDictionary(group => group.Key, group => group.Count());
            return counts.Where(c => c.Value == 2).Max(c => c.Key);
        }

        private int GetLowPair(List<Card> hand)
        {
            var counts = hand.GroupBy(card => card.CardNumber).ToDictionary(group => group.Key, group => group.Count());
            return counts.Where(c => c.Value == 2).Min(c => c.Key);
        }

        private int GetFifthCard(List<Card> hand, int highPair, int lowPair)
        {
            return hand.First(card => card.CardNumber != highPair && card.CardNumber != lowPair).CardNumber;
        }

        private decimal CalculateCardScore(List<Card> hand)
        {
            var score = 0m;
            var sortedHand = hand.OrderByDescending(card => card.CardNumber == 1 ? 14 : card.CardNumber).ToList();
            for (var i = 0; i < sortedHand.Count; i++)
            {
                var cardValue = sortedHand[i].CardNumber == 1 ? 14 : sortedHand[i].CardNumber;
                score += (decimal)(cardValue * Math.Pow(0.01, i + 1));
            }
            return score;
        }

        private List<Card> GetRemainingCards(List<Card> hand, int excludeCard)
        {
            return hand.Where(card => card.CardNumber != excludeCard).ToList();
        }

        private int GetHighCardForStraight(List<Card> hand)
        {
            var numbers = hand.Select(card => card.CardNumber).ToList();
            if (numbers.Contains(1))
            {
                return numbers.Contains(2) ? 5 : 14;
            }
            return numbers.Max();
        }


            }

        }
