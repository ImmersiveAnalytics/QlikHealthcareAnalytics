﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChartAndGraph
{
    /// <summary>
    /// text formatting for an item label
    /// </summary>
    [Serializable]
    public class TextFormatting : IInternalSettings
    {

        [SerializeField]
        private string prefix = "";

        [SerializeField]
        private string suffix = "";


        public string Suffix
        {
            get { return suffix; }
            set
            {
                suffix = value;
                RaiseOnUpdate();
            }
        }

        public string Prefix
        {
            get { return prefix; }
            set
            {
                prefix = value;
                RaiseOnUpdate();
            }
        }

        private event EventHandler OnDataUpdate;
        private event EventHandler OnDataChanged;

        protected virtual void RaiseOnChanged()
        {
            if (OnDataChanged != null)
                OnDataChanged(this, EventArgs.Empty);
        }

        protected virtual void RaiseOnUpdate()
        {
            if (OnDataUpdate != null)
                OnDataUpdate(this, EventArgs.Empty);
        }

        #region Intenal Use
        event EventHandler IInternalSettings.InternalOnDataUpdate
        {
            add
            {
                OnDataUpdate += value;
            }

            remove
            {
                OnDataUpdate -= value;
            }
        }

        event EventHandler IInternalSettings.InternalOnDataChanged
        {
            add
            {
                OnDataChanged += value;
            }
            remove
            {
                OnDataChanged -= value;
            }
        }
        #endregion

        private string FormatKeywords(string str,string category,string group)
        {
            return str.Replace("<?category>", category).Replace("<?group>",group).Replace("\\n",Environment.NewLine);
        }
        private string ValidString(string str)
        {
            if (str == null)
                return "";
            return str;
        }
        public string Format(string data,string category,string group)
        {
            string tmp = ValidString(Prefix) + data + ValidString(Suffix);
            return FormatKeywords(tmp, category, group);
        }

    }
}
