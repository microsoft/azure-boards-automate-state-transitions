using AutoStateTransitions.Misc;
using AutoStateTransitions.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AutoStateTransitions.Repos
{
    public class RulesRepo : IRulesRepo, IDisposable
    {
        private IOptions<AppSettings> _appSettings;
        private IHelper _helper;

        public RulesRepo(IOptions<AppSettings> appSettings, IHelper helper)
        {
            _appSettings = appSettings;
            _helper = helper;
        }

        public RulesModel ListRules(string wit)
        {
            string src = _appSettings.Value.SourceForRules;

            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString(src + "/rules." + wit.ToLower() + ".json");
                RulesModel rules = JsonConvert.DeserializeObject<RulesModel>(json);

                return rules;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RulesRepo()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _appSettings = null;
                _helper = null;
            }
        }
    }

    public interface IRulesRepo
    {
        RulesModel ListRules(string wit);
    }
}
