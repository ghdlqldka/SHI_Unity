using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Michsky.MUIP
{
    public class _MUI_DemoElementSwayParent : DemoElementSwayParent
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[_MUI_DemoElementSwayParent]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");

            elements.Clear();
            foreach (Transform child in this.transform)
            {
                elements.Add(child.GetComponent<DemoElementSway>());
            }
        }

        public override void SetWindowManagerButton(int index)
        {
            // Debug.Log("SetWindowManagerButton(), index : " + index + ", elements.Count : " + elements.Count);

            if (elements.Count == 0)
            {
                StartCoroutine("SWMHelper", index);
                return;
            }

            for (int i = 0; i < elements.Count; ++i)
            {
                if (i == index) 
                { 
                    elements[i].WindowManagerSelect();
                }
                else
                {
                    if (elements[i].wmSelected == false)
                    { 
                        continue;
                    }
                    elements[i].WindowManagerDeselect();
                }
            }

            if (titleAnimator == null)
                return;

            elementTitleHelper.text = elements[prevIndex].gameObject.name;
            elementTitle.text = elements[index].gameObject.name;

            titleAnimator.Play("Idle");
            titleAnimator.Play("Transition");

            prevIndex = index;
        }
    }
}