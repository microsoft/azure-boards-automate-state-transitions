using AutoStateTransitions.Models;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.Process.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoStateTransitions.Repos
{
    public class WorkItemRepo : IWorkItemRepo, IDisposable
    {
        private IOptions<AppSettings> _appSettings;

        public WorkItemRepo(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        public WorkItem GetWorkItem(VssConnection connection, int id)
        {
            WorkItemTrackingHttpClient client = connection.GetClient<WorkItemTrackingHttpClient>();

            try
            {
                WorkItem item = client.GetWorkItemAsync(id, null, null, WorkItemExpand.Relations).Result;

                return item;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~WorkItemRepo()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _appSettings = null;
            }
        }
    }

    public interface IWorkItemRepo
    {
        WorkItem GetWorkItem(VssConnection connection, int id);
    }
}
