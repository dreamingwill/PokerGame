using System;
using System.Collections.Generic;

// --- 1. 枚举定义 ---
public enum Suit { Spades, Hearts, Diamonds, Clubs }
public enum Rank { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

// --- 2. 基础数据类 ---
public class Card
{
    public Suit Suit { get; private set; }
    public Rank Rank { get; private set; }

    public Card(Suit suit, Rank rank)
    {
        Suit = suit;
        Rank = rank;
    }

    public override string ToString()
    {
        return $"{Suit} - {Rank}";
    }
}

public class Deck
{
    private List<Card> cards;
    private Random random = new Random();

    public Deck()
    {
        cards = new List<Card>();
        Reset();
    }

    public void Reset()
    {
        cards.Clear();
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
            {
                cards.Add(new Card(suit, rank));
            }
        }
    }

    public void Shuffle()
    {
        int n = cards.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            Card value = cards[k];
            cards[k] = cards[n];
            cards[n] = value;
        }
    }

    public Card DrawCard()
    {
        Card drawnCard = cards[0];
        cards.RemoveAt(0);
        return drawnCard;
    }
}

// --- 3. Cactus Kev 核心整数转换 ---
public static class CactusKev
{
    private static readonly int[] Primes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41 };

    public static int GetCardInt(Card card)
    {
        int r = (int)card.Rank - 2; 
        int p = Primes[r]; 
        int s = 0;
        switch (card.Suit)
        {
            case Suit.Spades:   s = 1; break;
            case Suit.Hearts:   s = 2; break;
            case Suit.Diamonds: s = 4; break;
            case Suit.Clubs:    s = 8; break;
        }
        int b = 1 << r; 
        return (b << 16) | (s << 12) | (r << 8) | p;
    }
}

// --- 4. 判牌器：结合 Perfect Hash 与 Cactus Kev ---
public static class HandEvaluator
{
    public static int Evaluate7Cards(List<Card> sevenCards)
    {
        int bestScore = 9999; 

        for (int i = 0; i < 21; i++)
        {
            List<Card> current5Cards = new List<Card> {
                sevenCards[LookupTables.Perm7[i, 0]],
                sevenCards[LookupTables.Perm7[i, 1]],
                sevenCards[LookupTables.Perm7[i, 2]],
                sevenCards[LookupTables.Perm7[i, 3]],
                sevenCards[LookupTables.Perm7[i, 4]]
            };

            int currentScore = Evaluate5Cards(current5Cards);
            
            if (currentScore < bestScore)
            {
                bestScore = currentScore;
            }
        }

        return bestScore;
    }

    private static int Evaluate5Cards(List<Card> fiveCards)
    {
        int c1 = CactusKev.GetCardInt(fiveCards[0]);
        int c2 = CactusKev.GetCardInt(fiveCards[1]);
        int c3 = CactusKev.GetCardInt(fiveCards[2]);
        int c4 = CactusKev.GetCardInt(fiveCards[3]);
        int c5 = CactusKev.GetCardInt(fiveCards[4]);

        int q = (c1 | c2 | c3 | c4 | c5) >> 16;
        
        if ((c1 & c2 & c3 & c4 & c5 & 0xF000) != 0) 
        {
            return LookupTables.Flushes[q];
        }

        int uniqueScore = LookupTables.Unique5[q];
        if (uniqueScore != 0)
        {
            return uniqueScore;
        }

        int primeProduct = (c1 & 0xFF) * (c2 & 0xFF) * (c3 & 0xFF) * (c4 & 0xFF) * (c5 & 0xFF);
        
        return FindScoreByPerfectHash((uint)primeProduct);
    }

    private static int FindScoreByPerfectHash(uint u)
    {
        uint a, b, r;
        u += 0xe91aaa35;
        u ^= u >> 16;
        u += u << 8;
        u ^= u >> 4;
        b  = (u >> 8) & 0x1ff;
        a  = (u + (u << 2)) >> 19;
        
        // b 的最大值是 511，去 HashAdjust (大小为512) 里查修饰符
        r  = a ^ LookupTables.HashAdjust[b];
        
        // 最终返回 HashValues 里的得分
        return LookupTables.HashValues[r];
    }

    // 从 6 张牌中找出最大牌型的得分 (Turn 转牌圈使用)
    public static int Evaluate6Cards(List<Card> sixCards)
    {
        int bestScore = 9999;
        
        // 依次剔除第 i 张牌，把剩下的 5 张拿去算分
        for (int i = 0; i < 6; i++)
        {
            List<Card> current5Cards = new List<Card>();
            for (int j = 0; j < 6; j++)
            {
                if (i != j) 
                {
                    current5Cards.Add(sixCards[j]);
                }
            }

            int currentScore = Evaluate5Cards(current5Cards);
            if (currentScore < bestScore)
            {
                bestScore = currentScore;
            }
        }
        return bestScore;
    }

    // 将 Cactus Kev 的得分转换为人类可读的牌型名称
    public static string GetHandName(int score)
    {
        if (score == 0) return "无效牌型";
        if (score <= 10) return "同花顺 (Straight Flush)";
        if (score <= 166) return "四条 (Four of a Kind)";
        if (score <= 322) return "葫芦 (Full House)";
        if (score <= 1599) return "同花 (Flush)";
        if (score <= 1609) return "顺子 (Straight)";
        if (score <= 2467) return "三条 (Three of a Kind)";
        if (score <= 3325) return "两对 (Two Pair)";
        if (score <= 6185) return "一对 (One Pair)";
        if (score <= 7462) return "高牌 (High Card)";
        return "未知错误";
    }
}

// --- 5. 程序入口 ---
class Program
{
    static void Main(string[] args)
    {
        Deck deck = new Deck();
        deck.Shuffle();

        List<Card> playerA = new List<Card> { deck.DrawCard(), deck.DrawCard() };
        List<Card> playerB = new List<Card> { deck.DrawCard(), deck.DrawCard() };
        List<Card> community = new List<Card> { deck.DrawCard(), deck.DrawCard(), deck.DrawCard(), deck.DrawCard(), deck.DrawCard() };

        Console.WriteLine($"[公共牌]: {string.Join(" | ", community)}");
        Console.WriteLine($"[玩家 A]: {string.Join(" | ", playerA)}");
        Console.WriteLine($"[玩家 B]: {string.Join(" | ", playerB)}\n");

        List<Card> cardsA = new List<Card>(community); cardsA.AddRange(playerA);
        List<Card> cardsB = new List<Card>(community); cardsB.AddRange(playerB);

        int scoreA = HandEvaluator.Evaluate7Cards(cardsA);
        int scoreB = HandEvaluator.Evaluate7Cards(cardsB);

        Console.WriteLine($"玩家 A 牌型得分: {scoreA}");
        Console.WriteLine($"玩家 B 牌型得分: {scoreB}\n");

        if (scoreA < scoreB) Console.WriteLine("🏆 玩家 A 获胜！(分数越小牌越大)");
        else if (scoreB < scoreA) Console.WriteLine("🏆 玩家 B 获胜！");
        else Console.WriteLine("🤝 双方平局！(平分奖池)");
    }
}