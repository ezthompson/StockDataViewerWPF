# StockDataViewerWPF

A basic C# app done in Windows Presentation Foundation (WPF) that converts a Windows Forms app that allows for users to open stock data and have it display on a candlestick chart. The main differences from the Windows Forms implementation are as follows:

- Charts are displayed in tab rather than opening new forms
- Charts allow for zooming in and out along with dragging of the axes

Due to the lack of official chart support in WPF, ScottPlot 5.0 is used as the graphing method.
