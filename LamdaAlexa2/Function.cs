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

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace LamdaAlexa2
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
                return MakeSkillResponse(
                        $"Hello Infotec! This is the first response from your Alexa skill using c sharp.",
                        true);
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

    }
}
