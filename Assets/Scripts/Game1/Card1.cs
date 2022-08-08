using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card1 
{
    public string suit;
    public string rank;
    public Card1(string suit,string rank)
    {
        this.suit = suit;
        this.rank = rank;
    }
    public override string ToString()
    {
        return rank + " " + suit;
    }

}
