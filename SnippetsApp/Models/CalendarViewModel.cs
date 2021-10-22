// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

// <CalendarViewModelSnippet>
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Graph;

namespace SnippetsApp.Models
{
    public class CalendarViewModel
    {
        private DateTime _startOfWeek;
        private List<CalendarViewEvent> _events;

        public CalendarViewModel()
        {
            _startOfWeek = DateTime.MinValue;
            _events = new List<CalendarViewEvent>();
        }

        public CalendarViewModel(DateTime startOfWeek, IEnumerable<Event> events)
        {
            _startOfWeek = startOfWeek;
            _events = new List<CalendarViewEvent>();

            if (events != null)
            {
              foreach (var item in events)
              {
                  _events.Add(new CalendarViewEvent(item));
              }
            }
        }

        // Get the start - end dates of the week
        public string TimeSpan()
        {
            return $"{_startOfWeek.ToString("MMMM d, yyyy")} - {_startOfWeek.AddDays(6).ToString("MMMM d, yyyy")}";
        }

        // Property accessors to pass to the daily view partial
        // These properties get all events on the specific day
        public DailyViewModel Sunday
        {
            get
            {
                return new DailyViewModel(
                  _startOfWeek,
                  GetEventsForDay(System.DayOfWeek.Sunday));
            }
        }

        public DailyViewModel Monday
        {
            get
            {
                return new DailyViewModel(
                  _startOfWeek.AddDays(1),
                  GetEventsForDay(System.DayOfWeek.Monday));
            }
        }

        public DailyViewModel Tuesday
        {
            get
            {
                return new DailyViewModel(
                  _startOfWeek.AddDays(2),
                  GetEventsForDay(System.DayOfWeek.Tuesday));
            }
        }

        public DailyViewModel Wednesday
        {
            get
            {
                return new DailyViewModel(
                  _startOfWeek.AddDays(3),
                  GetEventsForDay(System.DayOfWeek.Wednesday));
            }
        }

        public DailyViewModel Thursday
        {
            get
            {
                return new DailyViewModel(
                  _startOfWeek.AddDays(4),
                  GetEventsForDay(System.DayOfWeek.Thursday));
            }
        }

        public DailyViewModel Friday
        {
            get
            {
                return new DailyViewModel(
                  _startOfWeek.AddDays(5),
                  GetEventsForDay(System.DayOfWeek.Friday));
            }
        }

        public DailyViewModel Saturday
        {
            get
            {
                return new DailyViewModel(
                  _startOfWeek.AddDays(6),
                  GetEventsForDay(System.DayOfWeek.Saturday));
            }
        }

        private IEnumerable<CalendarViewEvent> GetEventsForDay(System.DayOfWeek day)
        {
            return _events.Where(e => e.Start.DayOfWeek.Equals(day));
        }
    }
}
// </CalendarViewModelSnippet>
