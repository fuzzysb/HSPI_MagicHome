﻿using System;

namespace MagicHomeAPI
{
    [Flags]
    public enum TimerDays
    {
        None = 0x00,
        Monday = 0x02,
        Tuesday = 0x04,
        Wednesday = 0x08,
        Thursday = 0x10,
        Friday = 0x20,
        Saturday = 0x40,
        Sunday = 0x80,

        Everyday = Monday | Tuesday | Wednesday | Thursday | Friday | Saturday | Sunday,
        Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday
    }
}