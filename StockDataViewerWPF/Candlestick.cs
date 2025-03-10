using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COP4365_Project1_ET
{
    #region Candlestick class data
    public class Candlestick
    {
        #region //Member variable definition
        /* Defining member variables + data binds for each variable in the Candlestick class:
         * Date: the date at which the stock data was taken
         * Open: the opening price of the stock (or its average if not daily)
         * Close: the closing price of the stock (or its average)
         * High: the highest price of the stock 
         * Low: the lowest price of the stock
         * Volume: how much of the stock was traded over the given period
         */
        public DateTime _date;
        public DateTime Date { get { return _date; } set { _date = value; } }
        public double _open;
        public double Open { get { return _open; } set { _open = value; } }
        public double _high;
        public double High { get { return _high; } set { _high = value; } }
        public double _low;
        public double Low { get { return _low; } set { _low = value; } }
        public double _close;
        public double Close { get { return _close; } set { _close = value; } }
        public ulong _volume;
        public ulong Volume { get { return _volume; } set { _volume = value; } }

        #endregion

        #region //Constructors

        public Candlestick()
        {

        }

        //Constructor where each variable is entered in manually
        public Candlestick(DateTime date, double open, double high, double low, double close, ulong volume)
        {
            this.Date = date;
            //Rounding each price variable to two double places
            this.Open = double.Round(open, 2);
            this.High = double.Round(high, 2);
            this.Close = double.Round(close, 2);
            this.Low = double.Round(low, 2);
            this.Volume = volume;
        }

        //Constructor where each variable is parsed from a string
        public Candlestick(string data)
        {
            //Defining separators
            char[] separators = new char[] { ',', '\"' };
            //Split each line using separators and remove any empty entries
            string[] dataSplit = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            //If the data input does not have 6 values, throw an exception, else continue
            if (dataSplit.Length != 6)
            {
                throw new ArgumentException("Invalid data format for candlestick, skipping");
            } else
            {
                //Parse the data for each field using respective parse functions for each type
                this.Date = DateTime.ParseExact(dataSplit[0], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                //Rounding each price variable to two double places
                this.Open = double.Round(double.Parse(dataSplit[1]), 2);
                this.High = double.Round(double.Parse(dataSplit[2]), 2);
                this.Low = double.Round(double.Parse(dataSplit[3]), 2);
                this.Close = double.Round(double.Parse(dataSplit[4]), 2);
                this.Volume = ulong.Parse(dataSplit[5]);
            }
        }

        #endregion

        #region //Methods

        //Method that displays each member variable of the candlestick and overrides the default ToString() method
        public override string ToString()
        {
            return $"Date: {Date}, Open: {Open}, High: {High}, Low: {Low}, Close: {Close} ";
        }

        #endregion
    }

    #endregion
}
