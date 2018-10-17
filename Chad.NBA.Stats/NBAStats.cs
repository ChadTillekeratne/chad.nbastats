using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Chad.NBA.Stats
{

    public enum GameQuarter
    {
        Q1 = 1,
        Q2 = 2,
        Q3 = 3,
        Q4 = 4,
    }

    public struct GameQuarterDetails
    {
        
        public int StartRange { get; set; }
        public int EndRange { get; set; }
    }

    public enum SeasonType
    {
        Preseason,
        RegularSeason,
        Playoffs
    }


    public class NBAStats
    {

        private const string NBA_STATS_URI_ROOT = "https://stats.nba.com/stats/";


        public static DataSet GetBoxscoreSummary(string gameId)
        {
            var tablesToInclude = new List<string> { "GameInfo", "GameSummary", "InactivePlayers", "LineScore", "Officials", "OtherStats", "TeamGameLogs" };

            DataSet ds = NBAStats.ReadApiResultSet(NBAStats.StatsApiGet($"boxscoresummaryv2/?gameId={gameId}"), "teamgamelogs", tablesToInclude);

            ds = NBAStats.UpdateApiResultSetWithValues(ds, new Dictionary<string, object> { { "GAME_ID", gameId } });

            return ds;
        }

        public static DataSet GetBoxScoreTraditionalV2(string gameId)
        {
            // https://stats.nba.com/stats/boxscoretraditionalv2?EndPeriod=10&EndRange=28800&GameID=0011800076&RangeType=0&Season=2018-19&SeasonType=Pre+Season&StartPeriod=1&StartRange=0

            String s = $"boxscoretraditionalv2?EndPeriod=10&EndRange=28800&GameID={gameId}&RangeType=0&StartPeriod=1&StartRange=0";

            return NBAStats.ReadApiResultSet(NBAStats.StatsApiGet($"boxscoretraditionalv2?EndPeriod=10&EndRange=28800&GameID={gameId}&RangeType=0&StartPeriod=1&StartRange=0"), "boxscoretraditionalv2wholegame");
        }


        public static DataSet GetPlayByPlayV2(String gameId)
        {
            // https://stats.nba.com/stats/playbyplayv2?GAMEID=0011800018&StartPeriod=0&EndPeriod=0

            return NBAStats.ReadApiResultSet(NBAStats.StatsApiGet($"playbyplayv2?GAMEID={gameId}&StartPeriod=0&EndPeriod=0"), "playbyplayv2", new List<string> { "PlayByPlay" });
        }

        public static DataSet GetPlayByPlay(String gameId)
        {
            // https://stats.nba.com/stats/playbyplayv2?GAMEID=0011800018&StartPeriod=0&EndPeriod=0

            return NBAStats.ReadApiResultSet(NBAStats.StatsApiGet($"playbyplay?GAMEID={gameId}&StartPeriod=0&EndPeriod=0"), "playbyplay", new List<string> { "PlayByPlay" });
        }


        //public static DataSet GetShotChartDetail(String gameId)
        //{
        //    // https://stats.nba.com/stats/shotchartdetail?AheadBehind=&CFID=&CFPARAMS=&ClutchTime=&Conference=&ContextFilter=&ContextMeasure=FGA&DateFrom=05%2F31%2F2018&DateTo=&Division=&EndPeriod=10&EndRange=28800&GROUP_ID=&GameEventID=&GameID=&GameSegment=&GroupID=&GroupMode=&GroupQuantity=5&LastNGames=0&LeagueID=00&Location=&Month=0&OnOff=&OpponentTeamID=0&Outcome=&PORound=0&Period=0&PlayerID=2544&PlayerID1=&PlayerID2=&PlayerID3=&PlayerID4=&PlayerID5=&PlayerPosition=&PointDiff=&Position=&RangeType=0&RookieYear=&Season=2017-18&SeasonSegment=&SeasonType=Playoffs&ShotClockRange=&StartPeriod=1&StartRange=0&StarterBench=&TeamID=1610612739&VsConference=&VsDivision=&VsPlayerID1=&VsPlayerID2=&VsPlayerID3=&VsPlayerID4=&VsPlayerID5=&VsTeamID=

        //    String a = $"https://stats.nba.com/stats/shotchartdetail?AheadBehind=&CFID=&CFPARAMS=&ClutchTime=&Conference=&ContextFilter=&ContextMeasure=FGA&DateFrom=05%2F31%2F2018&DateTo=&Division=&EndPeriod=10&EndRange=28800&GROUP_ID=&GameEventID=&GameID=0011800018&GameSegment=&GroupID=&GroupMode=&GroupQuantity=5&LastNGames=0&LeagueID=00&Location=&Month=0&OnOff=&OpponentTeamID=0&Outcome=&PORound=0&Period=0&PlayerID=2544&PlayerID1=&PlayerID2=&PlayerID3=&PlayerID4=&PlayerID5=&PlayerPosition=&PointDiff=&Position=&RangeType=0&RookieYear=&Season=2017-18&SeasonSegment=&SeasonType=Playoffs&ShotClockRange=&StartPeriod=1&StartRange=0&StarterBench=&TeamID=1610612745&VsConference=&VsDivision=&VsPlayerID1=&VsPlayerID2=&VsPlayerID3=&VsPlayerID4=&VsPlayerID5=&VsTeamID=";
        //}

        // Get games for day
        // https://stats.nba.com/stats/scoreboardV2?DayOffset=0&LeagueID=00&gameDate=10%2F06%2F2018

        public static DataSet GetTeamGameLogs(int seasonStartingYear, SeasonType seasonType, string teamId, DateTime? fromDate, DateTime? toDate, String leagueId = "00")
        {
            String fromDateFormatted = FormatDateTimeToString(fromDate,"");
            String toDateFomatted = FormatDateTimeToString(toDate, "");

            String season = FormatSeason(seasonStartingYear);

            if (String.IsNullOrEmpty(teamId))
                teamId = "0";
            
            String uri = $"teamgamelogs?DateFrom={fromDate}&DateTo={toDate}&GameSegment=&LastNGames=0&LeagueID={leagueId}&Location=&MeasureType=Base&Month=0&OpponentTeamID={teamId}&Outcome=&PORound=0&PaceAdjust=N&PerMode=Totals&Period=0&PlusMinus=N&Rank=N&Season={season}&SeasonSegment=&SeasonType={FormatSeasonType(seasonType)}&ShotClockRange=&TeamID=&VsConference=&VsDivision=";

            return NBAStats.ReadApiResultSet(NBAStats.StatsApiGet(uri), "teamgamelogs");
        }

        public string GetBoxscoreByQuarter(string season, SeasonType seasonType, string gameId, int quarter)
        {
            // https://stats.nba.com/stats/boxscoretraditionalv2?EndPeriod=10&EndRange=7200&GameID=0011800076&RangeType=2&Season=2018-19&SeasonType=Pre+Season&StartPeriod=1&StartRange=0
            // "GAME_ID","TEAM_ID","TEAM_ABBREVIATION","TEAM_CITY","PLAYER_ID","PLAYER_NAME","START_POSITION","COMMENT","MIN","FGM","FGA","FG_PCT","FG3M","FG3A","FG3_PCT","FTM","FTA","FT_PCT","OREB","DREB","REB","AST","STL","BLK","TO","PF","PTS","PLUS_MINUS"
            /*
            Q1: https://stats.nba.com/stats/boxscoretraditionalv2?EndPeriod=10&EndRange=7200&GameID=0011800076&RangeType=2&Season=2018-19&SeasonType=Pre+Season&StartPeriod=1&StartRange=0
            Q2: https://stats.nba.com/stats/boxscoretraditionalv2?EndPeriod=10&EndRange=14400&GameID=0011800076&RangeType=2&Season=2018-19&SeasonType=Pre+Season&StartPeriod=1&StartRange=7200
            Q3: https://stats.nba.com/stats/boxscoretraditionalv2?EndPeriod=10&EndRange=21600&GameID=0011800076&RangeType=2&Season=2018-19&SeasonType=Pre+Season&StartPeriod=1&StartRange=14400
            Q4: https://stats.nba.com/stats/boxscoretraditionalv2?EndPeriod=10&EndRange=28800&GameID=0011800076&RangeType=2&Season=2018-19&SeasonType=Pre+Season&StartPeriod=1&StartRange=21600
            OT1:  https://stats.nba.com/stats/boxscoretraditionalv2?EndPeriod=10&EndRange=31800&GameID=0011800041&RangeType=2&Season=2018-19&SeasonType=Pre+Season&StartPeriod=1&StartRange=28800

             */

            // Advanced
            // https://stats.nba.com/stats/boxscoreadvancedv2?EndPeriod=10&EndRange=28800&GameID=0011800076&RangeType=0&Season=2018-19&SeasonType=Pre+Season&StartPeriod=1&StartRange=0
            // "GAME_ID","TEAM_ID","TEAM_ABBREVIATION","TEAM_CITY","PLAYER_ID","PLAYER_NAME","START_POSITION","COMMENT","MIN","E_OFF_RATING","OFF_RATING","E_DEF_RATING","DEF_RATING","E_NET_RATING","NET_RATING","AST_PCT","AST_TOV","AST_RATIO","OREB_PCT","DREB_PCT","REB_PCT","TM_TOV_PCT","EFG_PCT","TS_PCT","USG_PCT","E_USG_PCT","E_PACE","PACE","PIE"


            return "boxscoretraditionalv2?EndPeriod=10&EndRange=7200&GameID=0&RangeType=2&Season=2018-19&SeasonType=Pre+Season&StartPeriod=1&StartRange=0";   
        }


        public static GameQuarterDetails GetQuarterDetails(GameQuarter quarter)
        {
            switch (quarter)
            {
                case GameQuarter.Q1:
                    return new GameQuarterDetails { StartRange = 1, EndRange = 1 };
                case GameQuarter.Q2:
                    return new GameQuarterDetails { StartRange = 1, EndRange = 1 };
                case GameQuarter.Q3:
                    return new GameQuarterDetails { StartRange = 1, EndRange = 1 };
                case GameQuarter.Q4:
                    return new GameQuarterDetails { StartRange = 1, EndRange = 1 };
            }

            throw new NotImplementedException();
        }
        
        #region Formatting Functions

        private static string FormatSeason(int seasonStartingYear)
        {
            return seasonStartingYear + "-" + (seasonStartingYear + 1).ToString().Substring(seasonStartingYear.ToString().Length-2);
        }

        private static string FormatDateTimeToString(DateTime? dateTime, String returnIfInvalid = "")
        {
            if (dateTime.HasValue)
            {
                return dateTime.ToString(); // TODO: Update to the correct format
            }
            else
            {
                return returnIfInvalid;
            }
        }

        public static string FormatSeasonType(SeasonType seasonType)
        {
            switch (seasonType)
            {

                case SeasonType.Preseason:
                    return "Pre Season";
                case SeasonType.RegularSeason:
                    return "Regular Season";
                case SeasonType.Playoffs:
                    return "Playoffs";
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #region API Results Reader

        public static DataSet UpdateApiResultSetWithValues(DataSet ds, Dictionary<string, object> dictValues)
        {
            foreach (DataTable dt in ds.Tables)
            {
                foreach (KeyValuePair<String, object> kvp in dictValues)
                {
                    if (!dt.Columns.Contains(kvp.Key))
                    {
                        // Add the column
                        dt.Columns.Add(kvp.Key, kvp.Value.GetType());

                        // add the value
                        foreach(DataRow row in dt.Rows)
                        {
                            row[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }

            return ds;
        }


        public static DataSet ReadApiResultSet(String apiResultValue, String tableNamePreface, List<String> onlyIncludeDataTablesWithNames = null)
        {
            DataSet ds = new DataSet();

            JObject r = JObject.Parse(apiResultValue);

            ds.DataSetName = (string)r["resource"];

            // foreach resultset
            foreach (var s in r["resultSets"])
            {
                DataTable dt = new DataTable();

                if (String.IsNullOrEmpty(tableNamePreface))
                {
                    dt.TableName = (string)s["name"];
                }
                else
                {
                    dt.TableName = tableNamePreface + "_" + (string)s["name"];
                }

                if(onlyIncludeDataTablesWithNames != null)
                {
                    if (!onlyIncludeDataTablesWithNames.Contains(s["name"].ToString()))
                        continue;
                }

                // create columns
                int colNum = 0;
                foreach (var columnName in s["headers"])
                {
                    Type rowDataType = "".GetType();

                    // Set the datacolumn type if the value exists
                    if (s["rowSet"].HasValues)
                    {
                        Object o = (s["rowSet"][0].ToObject<object[]>())[colNum];

                        if(o != null)
                            rowDataType = o.GetType();
                    }

                    dt.Columns.Add(columnName.ToString(), rowDataType);

                    colNum++;
                }

                // add data
                foreach (var valueRow in s["rowSet"])
                {
                    object[] values = (object[])valueRow.ToObject<object[]>();

                    dt.Rows.Add(values);
                }

                ds.Tables.Add(dt);
            }

            return ds;
        }

        #endregion

        #region API HTTP

        public static Dictionary<string,string> StatsApiGetBulk(List<string> apiUris, int degreeOfParrallelism, bool useLocalCache = true)
        {
            Dictionary<string, string> apiResults = new Dictionary<string, string>();

            Parallel.ForEach(apiUris, new ParallelOptions { MaxDegreeOfParallelism = 1 }, (apiUri) =>
            {
                //TODO: Add resiliency
                apiResults.Add(apiUri, StatsApiGet(apiUri, useLocalCache));
            });

            return apiResults;
        }

        public static string StatsApiGet(String apiUri, bool useLocalCache = true)
        {
            String requestUri = NBA_STATS_URI_ROOT + apiUri;

            // maintain a local cache of requests
            String cacheLocation = "ApiCache";
            String filePath = "";

            if(useLocalCache)
            {
                // Create the directory if it doesn't exist
                Directory.CreateDirectory(cacheLocation);

                filePath = cacheLocation + @"\" + GetMD5Hash(requestUri);

                if (File.Exists(filePath))
                    return File.ReadAllText(filePath);
            }

            WebRequest req = WebRequest.Create(requestUri);

            req.Headers.Add("User-Agent", Guid.NewGuid() + " NBAStatsAPI 0.1 ");
            req.Headers.Add("Accept", "*/*");

            var response = (HttpWebResponse)req.GetResponse();

            // TODO: Handle failures
            String results = "";
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                results = sr.ReadToEnd();
            }

            if(useLocalCache)
            {
                File.WriteAllText(filePath, results);
            }

            return results;
        }


        #endregion

        #region Other

        public static string GetMD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }


        #endregion

    }
}
