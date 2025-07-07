using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoogleSheetsToUnity
{
    public class _AnimalObject : AnimalObject
    {
#if false
        public Animal animal;


        public Text nameTxt, healthTxt, attackTxt, defenceTxt;

        // Use this for initialization
        void Start()
        {
            BuildAnimalInfo();
        }

        public void BuildAnimalInfo()
        {
            nameTxt.text = animal.name;
            healthTxt.text = "Health" + animal.health;
            attackTxt.text = "Attack" + animal.attack;
            defenceTxt.text = "Defence" + animal.defence;
        }
#endif

        public _Animal _animal
        {
            get
            {
                return animal as _Animal;
            }
        }

        protected override void Start()
        {
            Debug.Assert(animal != null);

            BuildAnimalInfo();
        }

        public override void BuildAnimalInfo()
        {
            // Debug.Log("" + _animal);
            nameTxt.text = _animal.name;
            healthTxt.text = "Health" + _animal.health;
            attackTxt.text = "Attack" + _animal.attack;
            defenceTxt.text = "Defence" + _animal.defence;
        }
    }
}