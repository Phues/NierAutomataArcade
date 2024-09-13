using System;
using System.Linq;
using UnityEngine;

namespace TinyGiantStudio.ModularToDoLists
{
    [System.Serializable]
    public class TGSTime
    {
        public int year = 2000;
        public int month = 1;
        public int day = 1;
        public string dayOfTheWeek;
        public int minute;
        public int hour;

        //
        public int hourBeforeCountingAMPM;
        public int timeIndex;//am vs pm

        [SerializeField]
        private TwelveHourFormat _hourInTweleveHourFormat;
        public TwelveHourFormat HourInTweleveHourFormat
        {
            get
            {
                if (_hourInTweleveHourFormat == null)
                    _hourInTweleveHourFormat = new TGSTime.TwelveHourFormat();

                return _hourInTweleveHourFormat;
            }
            set
            {
                if (value.hour < 0)
                    value.hour = 0;
                else if (value.hour > 12)
                    value.hour = 12;

                _hourInTweleveHourFormat = value;

                if (value.format == TimeFormat.AM)
                {
                    if (value.hour == 12)
                        hour = 0;
                    else hour = value.hour;
                }
                else if (value.format == TimeFormat.PM)
                {
                    if (value.hour == 12)
                        hour = 12;
                    else hour = value.hour + 12;
                }
            }
        }
        [System.Serializable]
        public class TwelveHourFormat
        {
            public int hour = 0;
            public TimeFormat format = TimeFormat.AM;
        }

        public void UpdateHourInTweleveHourFormat()
        {
            if (hour > 0 && hour < 12)
            {
                HourInTweleveHourFormat.hour = hour;
                HourInTweleveHourFormat.format = TimeFormat.AM;
            }
            else
            {
                HourInTweleveHourFormat.hour = hour - 12;
                HourInTweleveHourFormat.format = TimeFormat.PM;
            }
        }

        public DateTime dateTime
        {
            get
            {
                VerifyValues();

                return new DateTime(year, month, day, hour, minute, 0);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="time"></param>
        public TGSTime(DateTime time)
        {
            year = time.Year;
            month = time.Month;
            day = time.Day;
            dayOfTheWeek = time.DayOfWeek.ToString();
            minute = time.Minute;
            hour = time.Hour;
        }

        public enum TimeFormat
        {
            AM, PM
        }


        /// <summary>
        /// Check for illegal values
        /// </summary>
        public void VerifyValues()
        {
            if (year < 1900) year = 1900;
            if (year > 5000) year = 5000;

            if (month < 1) month = 1;
            if (month > 12) month = 12;

            if (day < 1) day = 1;
            if (day > DateTime.DaysInMonth(year, month)) day = DateTime.DaysInMonth(year, month);

            if (hour < 0) hour = 0;
            if (hour > 23) hour = 23;

            if (minute < 0) minute = 0;
            if (minute > 59) minute = 59;
        }


        public void UpdateDayOfTheWeek()
        {
            try
            {
                DateTime dateTime = new DateTime(year, month, day);
                dayOfTheWeek = dateTime.DayOfWeek.ToString();
            }
            catch { }
        }


        public bool TimeIsToday(DateTime now)
        {
            if (now.Year == year && now.Month == month && now.Day == day)
                return true;
            else
                return false;
        }
        public bool NotTimeYet()
        {
           return  dateTime > DateTime.Now;
        }


        //TODO
        //days from current time only checks 24hour difference. doesn't compare 10pm today vs 2am tomorrow as different day
        public int DaysFromCurrentTime() //isn't working
        {
            var now = DateTime.Now;
            try
            {
                TimeSpan timeSpan = new DateTime(year, month, day) - now;

                return timeSpan.Days;
            }
            catch
            {
                return 100;
            }
        }


        #region Get strings
        public override string ToString()
        {
            return $"{year}:{month}:{day}:{hour}:{minute}:{dayOfTheWeek}";
        }

        /// <summary>
        /// Example: 2023:8:9:4:43:Wednesday
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TGSTime Parse(string value)
        {
            try
            {
                TGSTime time = new TGSTime(new DateTime());
                var parts = value.Split(':');
                int.TryParse(parts[0], out time.year);
                int.TryParse(parts[1], out time.month);
                int.TryParse(parts[2], out time.day);
                int.TryParse(parts[3], out time.hour);
                int.TryParse(parts[4], out time.minute);
                time.dayOfTheWeek = parts[5];
                return time;
            }
            catch { Debug.Log(value + " parsing failed."); return null; }
        }
        public string GetTime()
        {
            string myTime = string.Empty;
            if (hour < 12)
            {
                myTime = hour.ToString("00") + ":" + minute.ToString("00") + " AM";
            }
            else if (hour == 12)
            {
                myTime = 12 + ":" + minute.ToString("00") + " PM";
            }
            if (hour > 12)
            {
                myTime = (hour - 12).ToString("00") + ":" + minute.ToString("00") + " AM";
            }
            return myTime;
        }
        public string GetDate()
        {
            return day.ToString("00") + "/" + month.ToString("00") + "/" + year.ToString("0000");
        }

        public string GetFullTime()
        {
            string myTime = string.Empty;

            if (DaysFromCurrentTime() < 8)
                myTime += "This ";
            else
                myTime += "On a ";

            myTime += dayOfTheWeek + ", ";

            myTime += GetTime() + " " + GetDate();

            return myTime;
        }
        public string GetDueTime()
        {
            //string myTime = string.Empty;
            var now = DateTime.Now;
            if (TimeIsToday(now)) //today
            {
                if (NotTimeYet())
                    return "Due today at " + GetTime();
                else
                    return "Was due today at " + WrittenTime();
            }
            return WrittenDateMonth();
        }
        public string GetShortDueTime()
        {
            //string myTime = string.Empty;
            var now = DateTime.Now;
            if (TimeIsToday(now)) //today
            {
                if (NotTimeYet())
                    return "Due at " + GetTime();
                else
                    return "Was due at " + WrittenTime();
            }
            return ShortWrittenDateMonth();
        }

        /// <summary>
        /// Returns dd/mm/year
        /// </summary>
        /// <returns></returns>
        string ShortWrittenDateMonth()
        {
            return day.ToString("00") + "/" + month.ToString("00") + "/" + (year % 100).ToString("00");
            //var dateTime = new DateTime(year, month, day, hour, minute, 0);
            //return dateTime.ToShortDateString();
        }
        string WrittenDateMonth()
        {
            VerifyValues();

            //return dayOfTheWeek + ", " + MonthName(month) + " " + day + ", " + (year % 100).ToString("00");
            var dateTime = new DateTime(year, month, day, hour, minute, 0);
            return dateTime.ToLongDateString();
        }
        string WrittenTime()
        {
            var dateTime = new DateTime(year, month, day, hour, minute, 0);
            return dateTime.ToShortTimeString();
        }

        //string MonthName(int index)
        //{
        //    if (month == 1)
        //        return "January";
        //    if (month == 2)
        //        return "February";
        //    if (month == 3)
        //        return "March";
        //    if (month == 4)
        //        return "April";
        //    if (month == 5)
        //        return "May";
        //    if (month == 6)
        //        return "June";
        //    if (month == 7)
        //        return "July";
        //    if (month == 8)
        //        return "August";
        //    if (month == 9)
        //        return "September";
        //    if (month == 10)
        //        return "Octobor";
        //    if (month == 11)
        //        return "Novembor";
        //    if (month == 12)
        //        return "December";
        //    return "Uknown";
        //}
        #endregion Get strings

    }
}