using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chad.NBA.Stats
{
    public class NBAStatsUpdater : IDisposable
    {
        private NBAStatsDatabaseManager _db;

        #region Constructors

        public NBAStatsUpdater(string connectionString)
        {
            //TODO: Handle database connection failures
            _db = new NBAStatsDatabaseManager(connectionString);
        }

        #endregion

        #region Updaters

        public void UpdateForSeason(SeasonType type, int startingYear)
        {
            // get all games for the season
            var teamGameLogs = NBAStats.GetTeamGameLogs(startingYear, type, "0", null, null);
            
            // iterate through unique games
            var uniqueGamesIds = teamGameLogs.Tables[0].AsEnumerable().Select(r => r.Field<String>("GAME_ID")).Distinct();

            // Check if the game has been processes

            Dictionary<string, DataSet> gameResults = new Dictionary<string, DataSet>();

            Parallel.ForEach(uniqueGamesIds, new ParallelOptions { MaxDegreeOfParallelism = 1 }, (gameId) =>
            {
                gameResults.Add(gameId+"_BoxScoreSummary", NBAStats.GetBoxscoreSummary(gameId));
                gameResults.Add(gameId + "_BoxScoreWholeGame", NBAStats.GetBoxScoreTraditionalV2(gameId));
                //gameResults.Add(gameId + "_PlayByPlayV2", NBAStats.GetPlayByPlayV2(gameId));
                //gameResults.Add(gameId + "_PlayByPlay", NBAStats.GetPlayByPlay(gameId));

            });

            // Insert box scores in sequence
            foreach (KeyValuePair<string, DataSet> kvp in gameResults)
            {
                _db.InsertData(kvp.Value);
            }

            // Insert all games
            _db.InsertData(teamGameLogs);
        }

        #endregion

        #region Deconstructors

        public void Dispose()
        {
            try
            {
                _db.Dispose();
            } catch (Exception ex)
            {
                //TODO:
            }
        }

        #endregion
    }
}
