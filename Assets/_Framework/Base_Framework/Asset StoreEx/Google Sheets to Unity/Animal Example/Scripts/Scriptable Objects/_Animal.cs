using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

namespace GoogleSheetsToUnity
{
    public class _Animal : Animal
    {
        private static string LOG_FORMAT = "<color=#00E1FF><b>[_Animal]</b></color> {0}";

#if false
        [HideInInspector]
        public string associatedSheet = "1GVXeyWCz0tCjyqE1GWJoayj92rx4a_hu4nQbYmW_PkE";
        [HideInInspector]
        public string associatedWorksheet = "Stats";

        public int health;
        public int attack;
        public int defence;
        public List<string> items = new List<string>();

        internal void UpdateStats(List<GSTU_Cell> list)
        {
            items.Clear();

            for (int i = 0; i < list.Count; i++)
            {
                switch (list[i].columnId)
                {
                    case "Health":
                        {
                            health = int.Parse(list[i].value);
                            break;
                        }
                    case "Attack":
                        {
                            attack = int.Parse(list[i].value);
                            break;
                        }
                    case "Defence":
                        {
                            defence = int.Parse(list[i].value);
                            break;
                        }
                    case "Items":
                        {
                            items.Add(list[i].value.ToString());
                            break;
                        }
                }
            }
        }

        internal void UpdateStats(GstuSpreadSheet ss)
        {
            items.Clear();
            health = int.Parse(ss[name, "Health"].value);
            attack = int.Parse(ss[name, "Attack"].value);
            defence = int.Parse(ss[name, "Defence"].value);
            items.Add(ss[name, "Items"].value.ToString());
        }

        internal void UpdateStats(GstuSpreadSheet ss, bool mergedCells)
        {
            items.Clear();
            health = int.Parse(ss[name, "Health"].value);
            attack = int.Parse(ss[name, "Attack"].value);
            defence = int.Parse(ss[name, "Defence"].value);

            //I know that my items column may contain multiple values so we run a for loop to ensure they are all added
            foreach (var value in ss[name, "Items", true])
            {
                items.Add(value.value.ToString());
            }
        }
#endif
        public string _associatedSheet
        {
            get
            {
                associatedSheet = "19bBi51z-6VwLQ4BCBXf9xx1VRX9zEagpfHrHb-udTP0";
                return associatedSheet;
            }
        }

        [SerializeField]
        protected string _name;
        public string Name
        {
            get
            {
                return _name;
            }
        }

        // public void UpdateStats(GstuSpreadSheet ss)
        public override void UpdateStats(GstuSpreadSheet sheet)
        {
            Debug.LogFormat(LOG_FORMAT, "UpdateStats(), _name : <b>" + _name + "</b>");

            _GstuSpreadSheet ss = sheet as _GstuSpreadSheet;
            items.Clear();

            health = int.Parse(ss[_name, "Health"].value);
            attack = int.Parse(ss[_name, "Attack"].value);
            defence = int.Parse(ss[_name, "Defence"].value);
            items.Add(ss[_name, "Items"].value.ToString());
        }

        // public void UpdateStats(GstuSpreadSheet ss, bool mergedCells)
        public override void UpdateStats(GstuSpreadSheet sheet, bool mergedCells)
        {
            _GstuSpreadSheet ss = sheet as _GstuSpreadSheet;
            items.Clear();

            health = int.Parse(ss[_name, "Health"].value);
            attack = int.Parse(ss[_name, "Attack"].value);
            defence = int.Parse(ss[_name, "Defence"].value);

            //I know that my items column may contain multiple values so we run a for loop to ensure they are all added
            foreach (GSTU_Cell value in ss[_name, "Items", true])
            {
                items.Add(value.value.ToString());
            }
        }
    }
}