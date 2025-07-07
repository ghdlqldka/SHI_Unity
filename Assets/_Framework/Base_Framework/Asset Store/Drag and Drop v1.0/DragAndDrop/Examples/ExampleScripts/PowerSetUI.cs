using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _DragAndDrop
{

    public class PowerSetUI : ObjectContainerArray
    {

        public PowerSet powerSet;

        // Use this for initialization
        void Start()
        {
            CreateSlots(powerSet.powers);
        }

        // can't change the contents of this one by dragging/dropping
        public override bool IsReadOnly()
        {
            return true;
        }
    }
}