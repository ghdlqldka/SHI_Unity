using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Reporter;

namespace _Base_Framework
{
    public class _UI_LogItem : MonoBehaviour
    {
        [ReadOnly]
        [SerializeField]
        protected Log _log;

        [Space(10)]
        [SerializeField]
        protected Image normalImage;
        [SerializeField]
        protected Image warningImage;
        [SerializeField]
        protected Image errorImage;

        [Space(5)]
        [SerializeField]
        protected TMP_Text conditionText;

        [Space(5)]
        [SerializeField]
        protected GameObject timePanelObj;
        [SerializeField]
        protected TMP_Text timeText;

        [Space(5)]
        [SerializeField]
        protected GameObject memoryPanelObj;
        [SerializeField]
        protected TMP_Text memoryText;
        protected bool _showMemory = false;

        protected virtual void Awake()
        {
            normalImage.gameObject.SetActive(false);
            warningImage.gameObject.SetActive(false);
            errorImage.gameObject.SetActive(false);

            memoryPanelObj.SetActive(false);
        }

        protected virtual void Update()
        {
            if (_showMemory != _LogViewer.Instance.ShowMemory)
            {
                _showMemory = _LogViewer.Instance.ShowMemory;
                memoryPanelObj.SetActive(_showMemory);
            }
        }

        public virtual void Set(Log log, string time, string memory)
        {
            _log = log;

            if (log.logType == _LogType.Log)
            {
                normalImage.gameObject.SetActive(true);
            }
            else if (log.logType == _LogType.Warning)
            {
                warningImage.gameObject.SetActive(true);
            }
            else
            {
                errorImage.gameObject.SetActive(true);
            }

            conditionText.text = log.condition;
            timeText.text = time;
            memoryText.text = memory;
        }

        public virtual void OnClick()
        {
            _LogViewer.Instance.SelectedLog = _log;
        }
    }
}