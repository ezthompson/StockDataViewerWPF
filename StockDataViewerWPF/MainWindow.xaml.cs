using COP4365_Project1_ET;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//Using ScottPlot for charting
using ScottPlot;
using ScottPlot.WPF;
using System.Diagnostics;

namespace StockDataViewerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Initialization of member variables

        //Holding a 2D list of candlesticks 
        List<List<Candlestick>> holdCandlesticks = new List<List<Candlestick>>();
        List<List<DateTime>> holdTimes = new List<List<DateTime>>();
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        #region File handling operations

        private void ShowFileDialog(object sender, RoutedEventArgs e)
        {
            //Creating an open file dialog for file selection
            OpenFileDialog fileDialog = new OpenFileDialog();
            //Setting the filter to only show .csv files by default
            fileDialog.Filter = "*.csv|*.csv|All Files|*.*";
            fileDialog.FilterIndex = 0;
            //Allowing for multiple files to be selected
            fileDialog.Multiselect = true;
            //Setting general title
            fileDialog.Title = "Select Stock Data";

            //Showing the dialog and setting its return value to a bool
            bool? fileOk = fileDialog.ShowDialog();
            //If the dialog returns true (meaning at least one file was selected)
            if (fileOk == true)
            {
                //Iterate through each file that was selected
                foreach (string filePath in fileDialog.FileNames)
                {
                    //Skip variable that determines if the current file should not be processed
                    bool skip = false;
                    //Obtaining the file name
                    string fileName = filePath.Split('\\').Last();
                    //Look through the tabs and see if the file has already been loaded in
                    foreach (TabItem tab in TabControl_Charts.Items)
                    {
                        //If detected, display a message then set skip to true
                        if (fileName == tab.Header.ToString())
                        {
                            MessageBox.Show($"{fileName} has already been loaded and will be skipped over.", "File Already Loaded", MessageBoxButton.OK);
                            skip = true;
                            break;
                        }
                    }
                    //If skip is false (the file has not been loaded already), proceed with processing
                    if (!skip)
                    {
                        //Generating initial candlestick list
                        List<Candlestick> cs = generateCandlesticks(filePath);
                        //Add full candlestick list to list collection
                        holdCandlesticks.Add(cs);
                        //Add time values to list collection
                        holdTimes.Add([StartOfRange_DatePicker.DisplayDate, EndOfRange_DatePicker.DisplayDate]);
                        //Filtering list by range of dates
                        List<Candlestick> filtered = filterCandlesticksByDate(cs);
                        //Add data to grid
                        setDataGrid(filtered);
                        //Creating a new tab in the tab control
                        addTab(filtered, fileName);   
                    }
                }
                //Set the selected tab to the last in the list
                TabControl_Charts.SelectedIndex = TabControl_Charts.Items.Count - 1;
            }
        }

        #endregion

        #region Candlestick generation and filtering
        private List<Candlestick> generateCandlesticks(string fileName)
        {
            //Creating a list of candlesticks from the file
            List<Candlestick> cs = new List<Candlestick>();

            //Use a StreamReader to read through the file
            using (StreamReader reader = new StreamReader(fileName))
            {
                //Skipping over the first line which will only contain the headers
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    //Generate a candlestick from the current line
                    Candlestick c = new Candlestick(reader.ReadLine());
                    //Add the candlestick to the list
                    cs.Add(c);
                }
            }

            //If the list is ordered by most recent first, reverse the list
            if (cs[0].Date > cs[1].Date)
            {
                cs.Reverse();
            }

            return cs;
        }

        private List<Candlestick> filterCandlesticksByDate(List<Candlestick> cs)
        {
            //Creating a candlestick list that contains all the 
            List<Candlestick> filtered = new List<Candlestick>();

            //Getting the current start and end dates from the picker
            DateTime start = StartOfRange_DatePicker.DisplayDate;
            DateTime end = EndOfRange_DatePicker.DisplayDate;

            //Iterate through each candlestick in the initial list
            foreach (Candlestick c in cs)
            {
                //If the current candlestick's date is before the start date, skip to the next one
                if (c.Date < start)
                    continue;

                //If the current candlestick's date is greater than the end date, stop the loop
                if (c.Date > end)
                    break;

                //If the current candlestick's date lies in the selected range, add it to the list
                if (c.Date >= start && c.Date <= end)
                    filtered.Add(c);
            }

            return filtered;
        }

        #endregion

        #region Date time operations
        private void startOfRange_DateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //Set the minimum date of the end date picker to the new start date
                EndOfRange_DatePicker.DisplayDateStart = StartOfRange_DatePicker.DisplayDate;
                //If there has been stock data added, update the charts accordingly
                if (holdCandlesticks.Count > 0)
                {
                    //Filtering list by range of dates
                    List<Candlestick> filtered = filterCandlesticksByDate(holdCandlesticks[TabControl_Charts.SelectedIndex]);
                    //Update the data grid
                    setDataGrid(filtered);
                    //Creating a new tab in the tab control
                    updateTab(filtered, (TabItem)TabControl_Charts.SelectedItem);
                }
            } catch (NullReferenceException) 
            {
                return;
            }
            
        }

        private void endOfRange_DateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //Set the minimum date of the end date picker to the new start date
                StartOfRange_DatePicker.DisplayDateEnd = EndOfRange_DatePicker.DisplayDate;
                //If there has been stock data added, update the charts accordingly
                if (holdCandlesticks.Count > 0)
                {
                    //Filtering list by range of dates
                    List<Candlestick> filtered = filterCandlesticksByDate(holdCandlesticks[TabControl_Charts.SelectedIndex]);
                    //Update the data grid
                    setDataGrid(filtered);
                    //Creating a new tab in the tab control
                    updateTab(filtered, (TabItem)TabControl_Charts.SelectedItem);
                }
            }
            catch (NullReferenceException)
            {
                return;
            }
            
        }
        #endregion

        #region Data grid operations
        private void setDataGrid(List<Candlestick> cs)
        {
            DataGrid_StockData.DataContext = cs;
        }

        #endregion

        #region Tab addition and updating

        private WpfPlot setDateTicks(WpfPlot plot, List<OHLC> stockData)
        {
            //Add the OHLC data to the chart and display dates on the bottom
            var candlePlot = plot.Plot.Add.Candlestick(stockData);
            //Set the plot to only display data in sequential order without gaps
            candlePlot.Sequential = true;

            //Displaying the candlesticks without gaps in between for unrecorded dates
            //(Sourced from the ScottPlot website's cookbook from "Candlestick Chart Without Gaps")

            //Determining a few candles to display ticks for
            int tickCount = 5;
            int tickDelta = stockData.Count / tickCount;
            DateTime[] tickDates = stockData
                .Where((x, i) => i % tickDelta == 0)
                .Select(x => x.DateTime)
                .ToArray();

            //By default, horizontal tick labels will be numbers (1, 2, 3...)
            //We can use a manual tick generator to display dates on the horizontal axis
            double[] tickPositions = Generate.Consecutive(tickDates.Length, tickDelta);
            string[] tickLabels = tickDates.Select(x => x.ToString("MM/dd/yyyy")).ToArray();
            ScottPlot.TickGenerators.NumericManual tickGen = new(tickPositions, tickLabels);
            plot.Plot.Axes.Bottom.TickGenerator = tickGen;

            return plot;
        }

        private void updateTab(List<Candlestick> filtered, TabItem tabIndex)
        {
            //Obtain current plot from the tab
            WpfPlot wpfPlot = (WpfPlot)tabIndex.Content;

            //Clear the old data
            wpfPlot.Plot.Clear();

            //Creating an OHLC list to display in the chart
            List<OHLC> stockData = new List<OHLC>();

            //Converting each candlestick to OHLC format
            foreach (Candlestick c in filtered)
            {
                //Using member variables of the candlestick to create an OHLC object
                OHLC stock = new OHLC(c.Open, c.High, c.Low, c.Close, c.Date, TimeSpan.FromDays(1.0));
                stockData.Add(stock);
            }

            //Call function to set date ticks for the chart
            wpfPlot = setDateTicks(wpfPlot, stockData);

            //Setting the axes of the plot to automatically scale to the new data range
            wpfPlot.Plot.Axes.AutoScale();

            //Refreshing the chart
            wpfPlot.Refresh();

            //Updating content of the tab to the new plot
            tabIndex.Content = wpfPlot;
        }

        private void addTab(List<Candlestick> filtered, string fileName)
        {
            //Generating a new tab item
            TabItem tabItem = new TabItem();

            //Creating a new chart plot
            WpfPlot wpfPlot = new WpfPlot();

            //Creating an OHLC list to display in the chart
            List<OHLC> stockData = new List<OHLC>();

            //Converting each candlestick to OHLC format
            foreach (Candlestick c in filtered)
            {
                //Using member variables of the candlestick to create an OHLC object
                OHLC stock = new OHLC(c.Open, c.High, c.Low, c.Close, c.Date, TimeSpan.FromDays(1.0));
                stockData.Add(stock);
            }

            //Call function to set date ticks for the chart
            wpfPlot = setDateTicks(wpfPlot, stockData);

            //Setting labels for the axes
            wpfPlot.Plot.Axes.Left.Label.Text = "Stock Price ($)";
            wpfPlot.Plot.Axes.Bottom.Label.Text = "Date";

            //Setting the title of the chart
            wpfPlot.Plot.Axes.Title.Label.Text = fileName;

            //Set the content of the tab equal to the plot
            tabItem.Content = wpfPlot;

            //Setting the header name of the tab
            tabItem.Header = fileName;

            //Add the new tab to the tab control
            TabControl_Charts.Items.Add(tabItem);
        }

        private void tabChanged(object sender, SelectionChangedEventArgs e)
        {
            //Filtering list by range of dates
            List<Candlestick> filtered = filterCandlesticksByDate(holdCandlesticks[TabControl_Charts.SelectedIndex]);
            //Update the data grid
            setDataGrid(filtered);
            //Creating a new tab in the tab control
            updateTab(filtered, (TabItem)TabControl_Charts.SelectedItem);
        }

        #endregion

    }
}