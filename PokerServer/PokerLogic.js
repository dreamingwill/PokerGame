// PokerLogic.js
const LookupTables = require('./LookupTables');

class Card {
    constructor(suit, rank) {
        this.suit = suit;
        this.rank = rank;
    }
    toString() { return `${this.suit} - ${this.rank}`; }
}

class Deck {
    constructor() {
        this.cards = [];
        this.reset();
        this.shuffle();
    }
    reset() {
        const suits = ['Spades', 'Hearts', 'Diamonds', 'Clubs'];
        const ranks = ['2', '3', '4', '5', '6', '7', '8', '9', 'T', 'J', 'Q', 'K', 'A']; 
        this.cards = [];
        for (let suit of suits) {
            for (let rank of ranks) {
                this.cards.push(new Card(suit, rank));
            }
        }
    }
    shuffle() {
        let n = this.cards.length;
        while (n > 1) {
            n--;
            let k = Math.floor(Math.random() * (n + 1));
            let temp = this.cards[k];
            this.cards[k] = this.cards[n];
            this.cards[n] = temp;
        }
    }
    drawCard() {
        if (this.cards.length === 0) return null;
        return this.cards.shift();
    }
}

// Cactus Kev 辅助对象
const CactusKev = {
    Primes: [ 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41 ],
    RankMap: { '2':0, '3':1, '4':2, '5':3, '6':4, '7':5, '8':6, '9':7, 'T':8, 'J':9, 'Q':10, 'K':11, 'A':12 },
    SuitMap: { 'Spades':1, 'Hearts':2, 'Diamonds':4, 'Clubs':8 },
    
    getCardInt: function(card) {
        let r = this.RankMap[card.rank];
        let p = this.Primes[r];
        let s = this.SuitMap[card.suit];
        let b = 1 << r;
        return (b << 16) | (s << 12) | (r << 8) | p;
    }
};

// 判牌器
const HandEvaluator = {
    evaluate7Cards: function(sevenCards) {
        let bestScore = 9999;
        for (let i = 0; i < 21; i++) {
            let current5Cards = [
                sevenCards[LookupTables.Perm7[i][0]],
                sevenCards[LookupTables.Perm7[i][1]],
                sevenCards[LookupTables.Perm7[i][2]],
                sevenCards[LookupTables.Perm7[i][3]],
                sevenCards[LookupTables.Perm7[i][4]]
            ];
            let currentScore = this.evaluate5Cards(current5Cards);
            if (currentScore < bestScore) {
                bestScore = currentScore;
            }
        }
        return bestScore;
    },

    evaluate5Cards: function(fiveCards) {
        let c1 = CactusKev.getCardInt(fiveCards[0]);
        let c2 = CactusKev.getCardInt(fiveCards[1]);
        let c3 = CactusKev.getCardInt(fiveCards[2]);
        let c4 = CactusKev.getCardInt(fiveCards[3]);
        let c5 = CactusKev.getCardInt(fiveCards[4]);

        let q = (c1 | c2 | c3 | c4 | c5) >>> 16; 
        
        if ((c1 & c2 & c3 & c4 & c5 & 0xF000) !== 0) {
            return LookupTables.Flushes[q];
        }

        let uniqueScore = LookupTables.Unique5[q];
        if (uniqueScore !== 0) {
            return uniqueScore;
        }

        let primeProduct = (c1 & 0xFF) * (c2 & 0xFF) * (c3 & 0xFF) * (c4 & 0xFF) * (c5 & 0xFF);
        return this.findScoreByPerfectHash(primeProduct);
    },

    // JS 版 Paul Senzee 完美哈希 (注意 >>> 0 防止符号位导致负数)
    findScoreByPerfectHash: function(u) {
        u = u >>> 0;
        u = (u + 0xe91aaa35) >>> 0;
        u ^= (u >>> 16);
        u = (u + (u << 8)) >>> 0;
        u ^= (u >>> 4);
        let b = (u >>> 8) & 0x1ff;
        let a = (u + (u << 2)) >>> 19;
        let r = a ^ LookupTables.HashAdjust[b];
        return LookupTables.HashValues[r];
    }
};

module.exports = { Card, Deck, HandEvaluator };