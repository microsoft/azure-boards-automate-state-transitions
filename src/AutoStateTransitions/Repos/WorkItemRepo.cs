using AutoStateTransitions.Misc;
using AutoStateTransitions.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.Process.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoStateTransitions.Repos
{
    public class WorkItemRepo : IWorkItemRepo, IDisposable
    {
        private IOptions<AppSettings> _appSettings;
        private IHelper _helper;

        public WorkItemRepo(IOptions<AppSettings> appSettings, IHelper helper)
        {
            _appSettings = appSettings;
            _helper = helper;
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

        public List<WorkItem> ListChildWorkItemsForParent(VssConnection connection, int parentId)
        {
            WorkItemTrackingHttpClient client = connection.GetClient<WorkItemTrackingHttpClient>();

            WorkItem item = client.GetWorkItemAsync(parentId, null, null, WorkItemExpand.Relations).Result;

            IEnumerable<WorkItemRelation> children = item.Relations.Where<WorkItemRelation>(x => x.Rel.Equals("System.LinkTypes.Hierarchy-Forward"));
            IList<int> Ids = new List<int>();

            foreach (var child in children)
            {
                Ids.Add(this._helper.GetWorkItemIdFromUrl(child.Url));
            }

            string[] fields = new string[] { "System.State" };

            List<WorkItem> list = client.GetWorkItemsAsync(Ids, fields).Result;

            return list;
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
