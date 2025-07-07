using System.Collections.Generic;
using UnityEngine;

namespace _Base_Framework
{

    [System.Serializable]
    public class AnimalStatsRowData : BaseSheetRowData
    {
        // private static string LOG_FORMAT = "<color=#37AFB6><b>[AnimalStats]</b></color> {0}";

        [ReadOnly]
        [SerializeField]
        private string _name;
        public string _Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        [ReadOnly]
        [SerializeField]
        private int health;
        public int Health
        {
            get
            {
                return health;
            }
            set
            {
                health = value;
            }
        }

        [ReadOnly]
        [SerializeField]
        private int attack;
        public int Attack
        {
            get
            {
                return attack;
            }
            set
            {
                attack = value;
            }
        }

        [ReadOnly]
        [SerializeField]
        private int defence;
        public int Defence
        {
            get
            {
                return defence;
            }
            set
            {
                defence = value;
            }
        }

        [ReadOnly]
        [SerializeField]
        private string items;
        public string Items
        {
            get
            {
                return items;
            }
            set
            {
                items = value;
            }
        }

        public AnimalStatsRowData(string _name, int health, int attack, int defence, string items)
        {
            _Name = _name;
            Health = health;
            Attack = attack;
            Defence = defence;
            Items = items;
        }

    }

}