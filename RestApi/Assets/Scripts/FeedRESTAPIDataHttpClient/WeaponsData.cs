using System.Collections.Generic;
using System;

namespace FeedRESTAPIDataHttpClient
{
    [Serializable]
    public class WeaponsData
    {
        public List<Weapon> Weapons;
    }
 
    [Serializable]
    public class Stats
    {
        public int Attack;
        public int Defence;
        public int Speed;
    }
 
    [Serializable]
    public class Weapon
    {
        public string Name;
        public Stats Stats;
    }
}