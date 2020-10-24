using MathNet.Numerics;
using MathNet.Numerics.LinearRegression;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using YahooFinanceApi;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml;

namespace emoreauDemoYahooCS
{
    public partial class GraphForm : Form
    {
        public GraphForm()
        {
            InitializeComponent();

            string xmlFileName = @"StockAbbreviations.xml";
             
            XmlSerializer serializer 
                = new XmlSerializer(typeof(Tickers));

            Tickers tickers = 
                (Tickers)serializer.Deserialize(new XmlTextReader(xmlFileName));

            cbStockAbbreviations.Items
                .AddRange(tickers.Stock.ToArray());

            cbStockAbbreviations.SelectedIndexChanged += 
                CbStockAbbreviations_SelectedIndexChanged;

            cbStockAbbreviations.SelectedIndex = 0;
        }

        private async void CbStockAbbreviations_SelectedIndexChanged(object sender, EventArgs e)
        {
            await GetPrices();
        }

        double[] Exponential(double[] x, double[] y)
        {
            double[] y_hat = Generate.Map(y, Math.Log);

            double[] p_hat = Fit.LinearCombination(x, y_hat, DirectRegressionMethod.QR, t => 1.0, t => t);

            return new[] { Math.Exp(p_hat[0]), p_hat[1] };
        }
        private async void btnGetPrices_Click(object sender, EventArgs e)
        {
            

        }

        private async Task GetPrices()
        {
            IReadOnlyList<YahooFinanceApi.Candle>
                            results = await Yahoo.GetHistoricalAsync(cbStockAbbreviations.Text, new DateTime(2000, 1, 1),
                            DateTime.Now, Period.Daily, new System.Threading.CancellationToken());

            this.chart1.Series.Clear();

            this.chart1.Titles.Add("");

            Series series = this.chart1.Series.Add(cbStockAbbreviations.Text);

            series.ChartType = SeriesChartType.Spline;

            Series seriesFit = this.chart1.Series.Add($"{cbStockAbbreviations.Text} - FIT");

            seriesFit.ChartType = SeriesChartType.Spline;

            double[] x = results.Select(r => (double)r.DateTime.Ticks).ToArray();
            double[] y = results.Select(r => (double)r.Close).ToArray();
            double[] p = Exponential(x, y); // a=1.017, r=0.687
            double[] yh = Generate.Map(x, k => p[0] * Math.Exp(p[1] * k));

            for (int c = 0; c < results.Count(); c++)
            {
                Candle candle = results[c];

                seriesFit.Points.AddXY(candle.DateTime, yh[c]);

                series.Points.AddXY(candle.DateTime, candle.Close);
            }
        }


        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
