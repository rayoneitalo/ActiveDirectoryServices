using System.Text.RegularExpressions;

namespace api_ldap.Utils
{
    public class FormattedDateHelper
    {
        public string FormattedDate(string date)
        {
            var createdAt = date.ToString().Split(".", 3);

            Regex pattern = new Regex(@"(\d{4})(\d{2})(\d{2})(\d{2})(\d{2})(\d{2})", RegexOptions.Compiled);

            MatchCollection matches = pattern.Matches(createdAt[0]);

            string year = matches[0].Groups[1].ToString();
            string month = matches[0].Groups[2].ToString();
            string day = matches[0].Groups[3].ToString();
            string hours = FormatterHelper(matches[0].Groups[4].ToString());
            string minutes = matches[0].Groups[5].ToString();
            string seconds = matches[0].Groups[6].ToString();

            /* ----- Received date format  ------

                            [0] e.g.: Full date    20220718135800
                            [1] Year    2022
                            [2] Month   07
                            [3] Day     18
                            [4] Hours   13
                            [5] Minutes 58
                            [6] Seconds 00

            */

            return String.Format("{0}/{1}/{2} {3}:{4}:{5}", day, month, year, hours, minutes, seconds); // UTC -3 hours

            /* ----- String Return -----

             "18/07/2022 13:58:00"

            */
        }

        public string FormattedFileTimeToDate(string fileTime)
        {

            /* ---- Received date format ----

               e.g. 133075489146545430s

            */

            if (fileTime == "0" || fileTime == "9223372036854775807") return null;

            DateTime fileTimeToDate = DateTime.FromFileTime(long.Parse(fileTime));

            string[] dateLessOneDay = fileTimeToDate.AddDays(-1).ToString().Split("/", 3); // Date -1 day

            string yearAndTime = dateLessOneDay[2];
            string month = dateLessOneDay[0];
            string day = dateLessOneDay[1];

            /* ----- Splited date format  ------

                         [0] e.g.: Full date  133075489146545430
                         [1] YearAndTime    2022 13:21:54
                         [2] Month          09
                         [3] Day            13
                         [4] Hours          13
                         [5] Minutes        21
                         [6] Seconds        54

            */

            return String.Format("{0}/{1}/{2}", day, month, yearAndTime);

            /* ----- String Return -----

                e.g. "13/09/2022 13:21:54"

            */
        }

        public string FormattedDateToFileTime(string date)
        {

            /* ---- Received date format ----

               e.g. 14/09/2022

            */

            if (date == null) return "0";

            string[] splitDate = date.Split("/", 3);

            int year = int.Parse(splitDate[2]);
            int month = int.Parse(splitDate[1]);
            int day = int.Parse(splitDate[0]);

            /* ----- Splited date format  ------

                         [0] e.g.: Full date  14/09/2022
                         [1] YearAndTime    2022
                         [2] Month          09
                         [3] Day            14
                         [4] Hours          13

            */


            DateTime dateToDateTime = new DateTime(year, month, day).AddDays(1);

            long dateToFileTime = dateToDateTime.ToFileTime();

            return dateToFileTime.ToString();

            /* ----- String Return -----

               e.g. "133076844000000000"

           */
        }

        private string FormatterHelper(string trick)
        {
            var hour = "";

            switch (int.Parse(trick))
            {
                case 0:
                    hour = String.Format("{0}", 21);
                    break;
                case 1:
                    hour = String.Format("{0}", 22);
                    break;
                case 2:
                    hour = String.Format("{0}", 23);
                    break;
                case 3:
                    hour = String.Format("{0}", 00);
                    break;
                case < 10:
                    hour = String.Format("{0}", int.Parse(trick) - 3);
                    break;
                default:
                    hour = String.Format("{0}", int.Parse(trick) - 3);
                    break;
            }

            return hour.Length < 2 ? $"0{hour}" : hour;
        }

    }
}