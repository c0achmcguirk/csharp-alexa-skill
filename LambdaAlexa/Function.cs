using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace LambdaAlexa
{
    public class Function
    {
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static HttpClient _httpClient;
        public const string INVOCATION_NAME = "Country Info";

        public Function()
        {
            _httpClient = new HttpClient();
        }

        public async Task<SkillResponse> FunctionHandler(SkillRequest input, ILambdaContext context)
        {

            var requestType = input.GetRequestType();
            if (requestType == typeof(IntentRequest))
            {
                var intentRequest = input.Request as IntentRequest;
                var countryRequested = intentRequest?.Intent?.Slots["Country"].Value;

                if (countryRequested == null)
                {
                    context.Logger.LogLine($"The country was not understood.");
                    return MakeSkillResponse("I'm sorry, but I didn't understand the country you were asking for. Please ask again.", false);
                }

                var countryInfo = await GetCountryInfo(countryRequested, context);
                var outputText = $"About {countryInfo.name}. The capitol is {countryInfo.capital} and the population is {countryInfo.population}.";
                return MakeSkillResponse(outputText, true);
            }
            else
            {
                return MakeSkillResponse(
                        $"I don't know how to handle this intent. Please say something like Alexa, ask {INVOCATION_NAME} about Canada.",
                        true);
            }
        }


        private SkillResponse MakeSkillResponse(string outputSpeech, 
            bool shouldEndSession, 
            string repromptText = "Just say, tell me about Canada to learn more. To exit, say, exit.")
        {
            var response = new ResponseBody
            {
                ShouldEndSession = shouldEndSession,
                OutputSpeech = new PlainTextOutputSpeech {Text = outputSpeech}
            };

            if (repromptText != null)
            {
                response.Reprompt = new Reprompt() {OutputSpeech = new PlainTextOutputSpeech() {Text = repromptText}};
            }

            var skillResponse = new SkillResponse
            {
                Response = response,
                Version = "1.0"
            };
            return skillResponse;
        }


        private async Task<Country> GetCountryInfo(string countryName, ILambdaContext context)
        {
            countryName = countryName.ToLowerInvariant();
            var countries = new List<Country>();

            // search by "North Korea" or "Vatican City" gives us poor results
            // instead search by both "North" and "Korea" to get better results
            var countryPartNames = countryName.Split(' ');
            if (countryPartNames.Length > 1)
            {
                foreach (var searchPart in countryPartNames)
                {
                    // The United States of America results in too many search requests.
                    if (searchPart != "the" || searchPart != "of")
                    {
                        countries.AddRange(await GetResultsForCountrySearch(searchPart, context));
                    }
                }
            }
            else
            {
                countries.AddRange(await GetResultsForCountrySearch(countryName, context));
            }

            // try to find a match on the name "korea" could return both north korea and south korea
            var bestMatch = (from c in countries
                where c.name.ToLowerInvariant() == countryName ||
                c.demonym.ToLowerInvariant() == $"{countryName}n"   // north korea hack (name is not North Korea, by demonym is North Korean)
                orderby c.population descending 
                select c).FirstOrDefault();

            var match = bestMatch ?? (from c in countries
                where c.name.ToLowerInvariant().IndexOf(countryName) > 0 
                || c.demonym.ToLowerInvariant().IndexOf(countryName) > 0
                orderby c.population descending 
                select c).FirstOrDefault();

            if (match == null && countries.Count > 0)
            {
                match = countries.FirstOrDefault();
            }

            return match;
        }

        private async Task<List<Country>> GetResultsForCountrySearch(string countryName, ILambdaContext context)
        {
            List<Country> countries = new List<Country>();
            var uri = new Uri($"https://restcountries.eu/rest/v2/name/{countryName}");
            context.Logger.LogLine($"Attempting to fetch data from {uri.AbsoluteUri}");
            try
            {
                var response = await _httpClient.GetStringAsync(uri);
                context.Logger.LogLine($"Response from URL:\n{response}");
                // TODO: (PMO) Handle bad requests
                countries = JsonConvert.DeserializeObject<List<Country>>(response);
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"\nException: {ex.Message}");
                context.Logger.LogLine($"\nStack Trace: {ex.StackTrace}");
            }
            return countries;
        }
    }

    public class Country
    {
        public string name { get; set; }
        public string[] topLevelDomain { get; set; }
        public string alpha2Code { get; set; }
        public string alpha3Code { get; set; }
        public string[] callingCodes { get; set; }
        public string capital { get; set; }
        public string[] altSpellings { get; set; }
        public string region { get; set; }
        public int population { get; set; }
        public float[] latlng { get; set; }
        public string demonym { get; set; }
        public float area { get; set; }
        public float? gini { get; set; }
        public string[] timezones { get; set; }
        public string[] borders { get; set; }
        public string nativeName { get; set; }
        public string numericCode { get; set; }
        public Currency[] currencies { get; set; }
        public Language[] languages { get; set; }
        public Translations translations { get; set; }
    }

    public class Translations
    {
        public string de { get; set; }
        public string es { get; set; }
        public string fr { get; set; }
        public string ja { get; set; }
        public string it { get; set; }
        public string br { get; set; }
        public string pt { get; set; }
    }

    public class Currency
    {
        public string code { get; set; }
        public string name { get; set; }
        public string symbol { get; set; }
    }

    public class Language
    {
        public string iso639_1 { get; set; }
        public string iso639_2 { get; set; }
        public string name { get; set; }
        public string nativeName { get; set; }
    }
}