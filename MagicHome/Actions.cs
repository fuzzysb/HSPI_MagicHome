using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HomeSeerAPI;
using Scheduler;

namespace HSPI_MagicHome
{
    public class Actions
    {
        private const string m_pageName = "Events";

        public int ActionCount => 0;

        
        private class ActionType
        {
            public string Name { get; set; }

            public ActionType(string name)
            {
                Name = name;
            }
        }
    }
}
