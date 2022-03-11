/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using NodaTime;
using ProtoBuf;
using QuantConnect.Data;
using QuantConnect.Util;

namespace QuantConnect.DataSource
{
    /// <summary>
    /// Universe Selection helper class for QuiverWikipedia dataset
    /// </summary>
    public class QuiverWikipediaUniverse : BaseData
    {
        /// <summary>
        /// The date of the Page View count
        /// </summary>
        [ProtoMember(10)]
        [JsonProperty(PropertyName = "Date")]
        [JsonConverter(typeof(DateTimeJsonConverter), "yyyy-MM-dd")]
        public DateTime Date { get; set; }

        /// <summary>
        /// The company's Wikipedia Page Views on the given date
        /// </summary>
        [ProtoMember(11)]
        [JsonProperty(PropertyName = "Views")]
        public decimal? PageViews { get; set; }

        /// <summary>
        /// The view count % change over the week prior to the date.
        /// Represented as a whole number (e.g. 100% = 100.0)
        /// </summary>
        [ProtoMember(12)]
        [JsonProperty(PropertyName = "pct_change_week")]
        public decimal? WeekPercentChange { get; set; }

        /// <summary>
        /// The view count % change over the month prior to the date
        /// Represented as a whole number (e.g. 100% = 100.0)
        /// </summary>
        [ProtoMember(13)]
        [JsonProperty(PropertyName = "pct_change_month")]
        public decimal? MonthPercentChange { get; set; }

        /// <summary>
        /// The period of time that occurs between the starting time and ending time of the data point
        /// </summary>
        [ProtoMember(14)]
        public TimeSpan Period { get; set; } = TimeSpan.FromDays(1);

        /// <summary>
        /// The time the data point ends at and becomes available to the algorithm
        /// </summary>
        public override DateTime EndTime => Time + Period;

        /// <summary>
        /// Return the URL string source of the file. This will be converted to a stream
        /// </summary>
        /// <param name="config">Configuration object</param>
        /// <param name="date">Date of this source file</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>String URL of source file.</returns>
        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            return new SubscriptionDataSource(
                Path.Combine(
                    Globals.DataFolder,
                    "alternative",
                    "quiver",
                    "wikipedia",
                    "universe",
                    $"{date.ToStringInvariant(DateFormat.EightCharacter)}.csv"
                ),
                SubscriptionTransportMedium.LocalFile
            );
        }

        /// <summary>
        /// Parses the data from the line provided and loads it into LEAN
        /// </summary>
        /// <param name="config">Subscription configuration</param>
        /// <param name="line">Line of data</param>
        /// <param name="date">Date</param>
        /// <param name="isLiveMode">Is live mode</param>
        /// <returns>New instance</returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
        {
            var csv = line.Split(',');
            var pageViews = decimal.Parse(csv[2], NumberStyles.Any, CultureInfo.InvariantCulture);

            return new QuiverWikipediaUniverse
            {
                PageViews = pageViews,
                WeekPercentChange = decimal.Parse(csv[3], NumberStyles.Any, CultureInfo.InvariantCulture),
                MonthPercentChange = decimal.Parse(csv[4], NumberStyles.Any, CultureInfo.InvariantCulture),

                Symbol = new Symbol(SecurityIdentifier.Parse(csv[0]), csv[1]),
                Time = date - Period,
                Value = pageViews
            };
        }
    }
}