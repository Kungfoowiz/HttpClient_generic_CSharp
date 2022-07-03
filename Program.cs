// Generic HttpClient to get data from an API.
// Returns your deserialised type.

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


// -------------------------------


class Pagination
{
    public int page { get; set; }

    public int per_page { get; set; }

    public int total { get; set; }

    public int total_pages { get; set; }

}


// -------------------------------


class Competitions : Pagination
{
    public CompetitionData[] data { get; set; }
}

class CompetitionData
{
    public string name { get; set; }

    public string country { get; set; }

    public int year { get; set; }

    public string winner { get; set; }

    public string runnerup { get; set; }
}


// -------------------------------

class Matches : Pagination
{
    public MatchData[] data { get; set; }
}

class MatchData
{
    public string competition { get; set; }

    public int year { get; set; }

    public string round { get; set; }

    public string team1 { get; set; }

    public string team2 { get; set; }

    public string team1goals { get; set; }

    public string team2goals { get; set; }
}

// -------------------------------


class Result
{

    // Generic function to get API data from the url.
    // Returns the deserialised type.
    public static async Task<IList<X>> getApiData<X>(string url) where X : Pagination
    {
        List<X> result = new List<X>();

        HttpClient client = new HttpClient();

        var currentPage = 1;

        var totalPages = 1;


        // -------------------------------


        // Get API data across all pages.
        while (currentPage <= totalPages)
        {
            // Get API data.
            var apiResult = await client.GetStreamAsync($"{url}&page={currentPage}");

            // Deserialise into X class.
            var jsonResult = await JsonSerializer.DeserializeAsync<X>(apiResult);

            // If on first page, update total pages.
            if (currentPage == 1)
            {
                totalPages = jsonResult.total_pages;
            }

            // Append result to result list.
            result.Add(jsonResult);

            // Check next page results.
            currentPage++;
        }

        return result;
    }

    public static async Task<int> getWinnerTotalGoals(string competition, int year)
    {
        var totalGoals = 0;

        var winner = "";


        // -------------------------------


        // 1. Get who won the competition?
        var competitionResult = await getApiData<Competitions>(
            $"https://jsonmock.hackerrank.com/api/football_competitions?year={year}&name={competition}"
        );

        winner = competitionResult.FirstOrDefault().data.FirstOrDefault().winner;


        // -------------------------------


        // 2. Get their home goals for that competition?
        var homeGoalsResults = await getApiData<Matches>(
            $"https://jsonmock.hackerrank.com/api/football_matches?competition={competition}&year={year}&team1={winner}"
        );

        foreach (var homeGoalsResult in homeGoalsResults)
        {
            totalGoals += homeGoalsResult.data.Sum(x => Int32.Parse(x.team1goals));
        }


        // -------------------------------


        // 3. Get their visiting goals for that competition?
        var visitingGoalsResults = await getApiData<Matches>(
            $"https://jsonmock.hackerrank.com/api/football_matches?competition={competition}&year={year}&team2={winner}"
        );

        foreach (var visitingGoalsResult in visitingGoalsResults)
        {
            totalGoals += visitingGoalsResult.data.Sum(x => Int32.Parse(x.team2goals));
        }


        // -------------------------------


        return totalGoals;
    }

}

class Solution
{
    public static async Task Main(string[] args)
    {
        string competition = Console.ReadLine();

        int year = Convert.ToInt32(Console.ReadLine().Trim());

        int result = await Result.getWinnerTotalGoals(competition, year);

        Console.WriteLine(result);
    }
}
