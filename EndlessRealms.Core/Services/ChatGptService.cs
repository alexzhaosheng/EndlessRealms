using EndlessRealms.Core.Utility;
using EndlessRealms.Core.Utility.Extensions;
using EndlessRealms.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Services
{
    [Service]
    public class ChatGPTService
    {        
        private readonly ChatGptApiSetting _setting;
        private readonly HttpClient _httpClient;
        private readonly ApiCallSetting[] _apiCallSettings;
        private readonly SystemStatusManager _systemStatus;
        private readonly ILogService _logService;
        private readonly IPersistedDataProvider _persistedDataAccessor;
        private readonly SystemStatusManager _statusManager;

        public ChatGPTService(IOptions<ChatGptApiSetting> setting, IPersistedDataProvider statusLoader, SystemStatusManager systemStatus, ILogService logService, IPersistedDataProvider persistedDataAccessor, SystemStatusManager statusManager)
        {
            _setting = setting.Value;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_setting.ApiKey}");
            _apiCallSettings = statusLoader.LoadApiCallSettings().ToArray();
            _systemStatus = systemStatus;
            _logService = logService;
            _persistedDataAccessor = persistedDataAccessor;
            _statusManager = statusManager;
        }


        private ActionPrompt? _actionPrompt = null;
        public async Task<string> Call(
            Func<ActionPrompt, ActionPrompt.ActionPromptInfo> actionAccessor,
            params (string, string)[] parameters)
        {
            if (_actionPrompt == null)
            {
                _actionPrompt = _persistedDataAccessor.LoadActionPrompt();
            }

            var pmt = actionAccessor(_actionPrompt);
            var msg = pmt.Content;
            foreach(var p in parameters)
            {
                msg = msg.Replace(p.Item1, p.Item2);
            }
            return await Call(msg, pmt.ApiCallSettingName);
        }

        public async Task<(T, string)> Call<T>(Func<ActionPrompt, ActionPrompt.ActionPromptInfo> actionAccessor, params (string, string)[] parameters)
        {
            return await CallWithRetry<(T, string)>(async () =>
            {
                var yaml = await Call(actionAccessor, parameters);

                try
                {
                    return (yaml.YamlToObject<T>()!, yaml);
                }
                catch (Exception ex)
                {
                   throw new EndlessRealmsException($"Deserialize failed. \r{yaml}", ex);                    
                }
            });
        }
        public async Task<T[]?> CallForArray<T>(Func<ActionPrompt, ActionPrompt.ActionPromptInfo> actionAccessor, params (string, string)[] parameters)
        {
            return await CallWithRetry<T[]?>(async () =>
            {
                var yaml = await Call(actionAccessor, parameters);
                try
                {
                    return yaml.YamlToObject<T[]>();                    
                }
                catch (Exception ex)
                {
                    throw new EndlessRealmsException($"Deserialize failed. \r{yaml}", ex);                    
                }
            });

        }


        public async Task<string> Call(string prompt, string settingName)
        {
            _logService.Logger.Debug($"Call Chat GPT with {settingName}\nPrompt:\n-----------------------\n{prompt}");

            _systemStatus.PushStatus(SystemStatus.Working, "Call ChatGPT API...");
            try
            {
                var setting = _apiCallSettings.FirstOrDefault(t => t.Name == settingName);
                if (setting == null)
                {
                    throw new EndlessRealmsException("Can't find the api call setting by name " + settingName);
                }
                var dataObj = setting.DataObject.JsonClone();
                dataObj.prompt = prompt;
                var requestContent = new StringContent(dataObj.ToJsonString(), Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(setting.Url, requestContent);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();                    
                    var responseObj = responseContent.JsonToObject<ChatGptResponse>();
                    var strResponse = string.Join('\n', responseObj!.choices!.Select(c => c.text));
                    _logService.Logger.Debug($"Chat GPT response:\n----------------\n{strResponse}");
                    return strResponse;
                }
                else
                {
                    throw new EndlessRealmsException($"API request failed with status code {response.StatusCode}, {response.RequestMessage}");
                }
            }
            finally
            {
                _systemStatus.PopStatus();
            }
        }

        public async Task<T?> Call<T>(string prompt, string settingName)
        {
            return await CallWithRetry<T?>(async () =>
            {
                var yaml = await Call(prompt, settingName);

                try
                {
                    return yaml.YamlToObject<T>();
                }
                catch (Exception ex)
                {
                    throw new EndlessRealmsException($"Deserialize failed. \r{yaml}", ex);                    
                }
            });
        }
        public async Task<T[]?> CallForArray<T>(string prompt, string settingName)
        {
            return await CallWithRetry<T[]?>(async () =>
            {
                var yaml = await Call(prompt, settingName);
                try
                {                    
                    return yaml.YamlToObject<T[]>();
                }
                catch (Exception ex)
                {
                    throw new EndlessRealmsException($"Deserialize failed. \r{yaml}", ex);
                }
            });
            
        }

        private async Task<T> CallWithRetry<T>(Func<Task<T>> callback, int retryCount = 3)
        {
            var retried = 0;
            Exception? latestErr = null;
            while(retried < retryCount)
            {
                try
                {
                    var res  = await callback();
                    return res;
                }                
                catch(Exception ex)
                {
                    _logService.Logger.Warning($"Call ChatGPT API failed, {ex.Message}, retry ...", ex);                    
                    latestErr = ex;
                }

                
                retried++;                
                if(retried < retryCount)
                {
                    await Task.Delay(1000);
                }
            }
            
            throw new EndlessRealmsException("Call ChatGPT API failed, reaches the maximum retry count.", latestErr!);
        }
    }
 
}

class ArrayReponse<T>
{
    public T[]? ArrayResult { get; set; }
}

class ChatGptResponse
{
    public string? id { get; set; }            
    public List<ChatGptChoice>? choices { get; set; }    
}

class ChatGptChoice
{
    public string? text { get; set; }
}

