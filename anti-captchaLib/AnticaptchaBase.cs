﻿using System;
using System.Threading;
using Anticaptcha_example.ApiResponse;
using Anticaptcha_example.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Anticaptcha_example
{
    public abstract class AnticaptchaBase
    {
        public enum ProxyTypeOption
        {
            Http,
            Socks4,
            Socks5
        }

        private const string Host = "api.anti-captcha.com";
        private const SchemeType Scheme = SchemeType.Https;
        public string ErrorMessage { get; private set; }
        public int TaskId { get; private set; }
        public string ClientKey { set; private get; }
        public TaskResultResponse TaskInfo { get; protected set; }
        public abstract JObject GetPostData();

        /// <summary>
        /// Specify softId to earn 10% commission with your app.
        /// Get your softId here:
        /// https://anti-captcha.com/clients/tools/devcenter
        /// </summary>
        public int SoftId { set; private get; }

        public bool CreateTask()
        {
            var taskJson = GetPostData();

            if (taskJson == null)
            {
                DebugHelper.Out("A task preparing error.", DebugHelper.Type.Error);

                return false;
            }

            // if you don't need to see the raw JSON request - comment the next line out
            DebugHelper.Out(taskJson.ToString(), DebugHelper.Type.Info);

            var jsonPostData = new JObject();
            jsonPostData["clientKey"] = ClientKey;
            jsonPostData["softId"] = SoftId;
            jsonPostData["task"] = taskJson;

           


            var t = taskJson.ToString(Formatting.None);


            PostDataGambi teste = new PostDataGambi();
            teste.clientKey = ClientKey;
            teste.softId = SoftId;
            teste.task = t;


            var tt = System.Text.Json.JsonSerializer.Serialize(teste);
            var tt3 = System.Text.Json.JsonSerializer.Serialize(jsonPostData);


            DebugHelper.Out("Connecting to " + Host, DebugHelper.Type.Info);
            dynamic postResult = JsonPostRequest(ApiMethod.CreateTask, jsonPostData, teste);

            if (postResult == null || postResult.Equals(false))
            {
                DebugHelper.Out("API error", DebugHelper.Type.Error);

                return false;
            }

            var response = new CreateTaskResponse(postResult);

            if (!response.ErrorId.Equals(0))
            {
                ErrorMessage = response.ErrorDescription;

                DebugHelper.Out(
                    "API error " + response.ErrorId + ": " + response.ErrorDescription,
                    DebugHelper.Type.Error
                );

                return false;
            }

            if (response.TaskId == null)
            {
                DebugHelper.JsonFieldParseError("taskId", postResult);

                return false;
            }

            TaskId = (int)response.TaskId;
            DebugHelper.Out("Task ID: " + TaskId, DebugHelper.Type.Success);

            return true;
        }

        public bool WaitForResult(int maxSeconds = 120, int currentSecond = 0)
        {
            if (currentSecond >= maxSeconds)
            {
                DebugHelper.Out("Time's out.", DebugHelper.Type.Error);

                return false;
            }

            if (currentSecond.Equals(0))
            {
                DebugHelper.Out("Waiting for 3 seconds...", DebugHelper.Type.Info);
                Thread.Sleep(3000);
            }
            else
            {
                Thread.Sleep(1000);
            }

            DebugHelper.Out("Requesting the task status", DebugHelper.Type.Info);

            var jsonPostData = new JObject
            {
                ["clientKey"] = ClientKey,
                ["taskId"] = TaskId
            };

            dynamic postResult = JsonPostRequest(ApiMethod.GetTaskResult, jsonPostData, null);

            if (postResult == null || postResult.Equals(false))
            {
                DebugHelper.Out("API error", DebugHelper.Type.Error);

                return false;
            }

            TaskInfo = new TaskResultResponse(postResult);

            if (!TaskInfo.ErrorId.Equals(0))
            {
                ErrorMessage = TaskInfo.ErrorDescription;

                DebugHelper.Out("API error " + TaskInfo.ErrorId + ": " + ErrorMessage, DebugHelper.Type.Error);

                return false;
            }

            if (TaskInfo.Status.Equals(TaskResultResponse.StatusType.Processing))
            {
                DebugHelper.Out("The task is still processing...", DebugHelper.Type.Info);

                return WaitForResult(maxSeconds, currentSecond + 1);
            }

            if (TaskInfo.Status.Equals(TaskResultResponse.StatusType.Ready))
            {
                if (TaskInfo.Solution.GRecaptchaResponse == null && TaskInfo.Solution.Text == null
                    && TaskInfo.Solution.Answers == null && TaskInfo.Solution.Token == null &&
                    TaskInfo.Solution.Challenge == null && TaskInfo.Solution.Seccode == null &&
                    TaskInfo.Solution.Validate == null && TaskInfo.Solution.CellNumbers.Count == 0
                    && TaskInfo.Solution.LocalStorage == null && TaskInfo.Solution.Cookies == null
                    && TaskInfo.Solution.Fingerprint == null)
                {
                    DebugHelper.Out("Got no 'solution' field from API", DebugHelper.Type.Error);

                    return false;
                }

                DebugHelper.Out("The task is complete!", DebugHelper.Type.Success);

                return true;
            }

            ErrorMessage = "An unknown API status, please update your software";
            DebugHelper.Out(ErrorMessage, DebugHelper.Type.Error);

            return false;
        }

        private dynamic JsonPostRequest(ApiMethod methodName, JObject jsonPostData, PostDataGambi teste)
        {
            string error;
            var methodNameStr = char.ToLowerInvariant(methodName.ToString()[0]) + methodName.ToString().Substring(1);

            //var json2 = JsonConvert.SerializeObject(teste, Formatting.Indented);

            //var json = JsonConvert.SerializeObject(jsonPostData, Formatting.Indented);
            var tt3 = System.Text.Json.JsonSerializer.Serialize(jsonPostData);


            dynamic data = HttpHelper.Post(
                new Uri(Scheme + "://" + Host + "/" + methodNameStr), tt3, out error

            );

            if (string.IsNullOrEmpty(error))
            {
                if (data == null)
                {
                    error = "Got empty or invalid response from API";
                }
                else
                {
                    return data;
                }
            }
            else
            {
                error = "HTTP or JSON error: " + error;
            }

            DebugHelper.Out(error, DebugHelper.Type.Error);

            return false;
        }

        public double? GetBalance()
        {
            var jsonPostData = new JObject();
            jsonPostData["clientKey"] = ClientKey;

            dynamic postResult = JsonPostRequest(ApiMethod.GetBalance, jsonPostData, null);

            if (postResult == null || postResult.Equals(false))
            {
                DebugHelper.Out("API error", DebugHelper.Type.Error);

                return null;
            }

            var balanceResponse = new BalanceResponse(postResult);

            if (!balanceResponse.ErrorId.Equals(0))
            {
                ErrorMessage = balanceResponse.ErrorDescription;

                DebugHelper.Out(
                    "API error " + balanceResponse.ErrorId + ": " + balanceResponse.ErrorDescription,
                    DebugHelper.Type.Error
                );

                return null;
            }

            return balanceResponse.Balance;
        }

        private enum ApiMethod
        {
            CreateTask,
            GetTaskResult,
            GetBalance
        }

        private enum SchemeType
        {
            Http,
            Https
        }
    }
}