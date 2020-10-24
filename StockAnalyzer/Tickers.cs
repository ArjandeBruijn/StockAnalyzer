 
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace emoreauDemoYahooCS
{
    [XmlRoot(ElementName = "tickers")]
    public class Tickers
    {
        [XmlElement(ElementName = "stock")]
        public List<string> Stock { get; set; }
    }

}
